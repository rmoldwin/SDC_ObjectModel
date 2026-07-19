using SDC.Schema;
using SDC.Schema.Extensions;
using System.Reflection;

namespace SDC.ScriptEngine.BlazorAsyncTests.Shared;

/// <summary>
/// Builds SDC trees programmatically. All methods are synchronous.
///
/// IMPORTANT: Call <see cref="BaseType.ResetLastTopNode"/> before calling any Build method
/// if other trees have been constructed in this session. Each Build method resets the
/// LastTopNode slot internally (either explicitly or via the deserializer), so sequential
/// calls produce independent trees.
///
/// <see cref="CloneViaXml"/> is a "build barrier" — it calls ResetLastTopNode() internally via
/// the serializer. Never call it while another tree build is in progress.
/// </summary>
public static class SdcTreeBuilder
{
    /// <summary>
    /// Creates a <see cref="FormDesignType"/> with <paramref name="sectionCount"/> sections,
    /// <paramref name="questionsPerSection"/> questions each, with string response fields.
    ///
    /// <paramref name="formId"/> must be a valid XML NCName (no hyphens, no spaces; underscores are fine).
    ///
    /// This method calls <see cref="BaseType.ResetLastTopNode"/> at the start so that
    /// sequential calls produce independent trees.
    /// </summary>
    public static FormDesignType BuildForm(string formId, int sectionCount = 3, int questionsPerSection = 5)
    {
        BaseType.ResetLastTopNode();
        var fd = new FormDesignType(null, formId);
        fd.AddBody();

        for (int s = 0; s < sectionCount; s++)
        {
            var section = fd.Body.AddChildSection($"{formId}_S{s}", $"Section_{s}");
            for (int q = 0; q < questionsPerSection; q++)
            {
                section.AddChildQuestionResponse($"{formId}_S{s}_Q{q}", out DataTypes_DEType _, $"Question_{s}_{q}");
            }
        }

        return fd;
    }

    /// <summary>
    /// Parses a <see cref="FormDesignType"/> from an XML string.
    /// This is synchronous — load the XML string async BEFORE calling this.
    ///
    /// NOTE: This calls <see cref="BaseType.ResetLastTopNode"/> internally via the deserializer.
    /// Complete any in-progress tree builds before calling this.
    /// </summary>
    public static FormDesignType BuildFromXmlString(string xml)
        => FormDesignType.DeserializeFromXml(xml);

    /// <summary>
    /// Deep-clones a tree via XML serialization round-trip.
    ///
    /// BUILD BARRIER: calls <see cref="BaseType.ResetLastTopNode"/> internally.
    /// Never call while another tree is being constructed.
    /// </summary>
    public static FormDesignType CloneViaXml(FormDesignType original)
    {
        string xml = original.GetXml();
        return FormDesignType.DeserializeFromXml(xml);
    }

    /// <summary>
    /// Applies a mutation to <paramref name="tree"/>. Returns a description of the change.
    ///
    /// <paramref name="targetId"/>: the sGuid or name of the node to mutate (null = first matching node).
    /// </summary>
    public static string ApplyMutation(FormDesignType tree, MutationType mutation, string? targetId = null)
        => mutation switch
        {
            MutationType.AddSection       => ApplyAddSection(tree, targetId),
            MutationType.RemoveQuestion   => ApplyRemoveQuestion(tree, targetId),
            MutationType.MoveSection      => ApplyMoveSection(tree, targetId),
            MutationType.ChangeAttribute  => ApplyChangeAttribute(tree, targetId),
            MutationType.BulkAddMoveRemove => ApplyBulkMutation(tree),
            _ => throw new ArgumentOutOfRangeException(nameof(mutation))
        };

    public enum MutationType
    {
        AddSection,
        RemoveQuestion,
        MoveSection,
        ChangeAttribute,
        BulkAddMoveRemove
    }

    /// <summary>
    /// Loads an embedded XML resource by its <c>LogicalName</c> (as declared in the .csproj
    /// <c>&lt;EmbeddedResource&gt;</c> item). Searches all loaded assemblies in the current
    /// AppDomain so it works from a shared library context regardless of which client project
    /// owns the embedded resource.
    /// Throws <see cref="InvalidOperationException"/> if the resource is not found.
    /// </summary>
    public static string LoadEmbeddedXml(string logicalName)
    {
        foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
        {
            Stream? stream = null;
            try { stream = asm.GetManifestResourceStream(logicalName); }
            catch { continue; }

            if (stream is not null)
            {
                using (stream)
                using (var reader = new StreamReader(stream))
                    return reader.ReadToEnd();
            }
        }

        var available = string.Join(", ", AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a => { try { return a.GetManifestResourceNames(); } catch { return Array.Empty<string>(); } }));
        throw new InvalidOperationException(
            $"Embedded resource '{logicalName}' not found in any loaded assembly. Available: [{available}]");
    }

    // ── Mutation helpers ────────────────────────────────────────────────────────

    private static string ApplyAddSection(FormDesignType tree, string? targetId)
    {
        var body = tree.Body ?? tree.AddBody();
        var newId = targetId ?? $"NewSection_{Guid.NewGuid().ToString("N")[..8]}";
        body.AddChildSection(newId, "Added_Section");
        return $"AddSection: added '{newId}'";
    }

    private static string ApplyRemoveQuestion(FormDesignType tree, string? targetId)
    {
        QuestionItemType? q = null;
        if (targetId is not null)
        {
            q = tree.Nodes.Values.OfType<QuestionItemType>()
                    .FirstOrDefault(n => n.name == targetId || n.sGuid == targetId);
        }
        q ??= tree.Nodes.Values.OfType<QuestionItemType>().FirstOrDefault();

        if (q is null) return "RemoveQuestion: no question found";
        var name = q.name;
        // cancelIfChildNodes=false: force recursive removal including ResponseField children
        bool ok = q.RemoveRecursive(cancelIfChildNodes: false);
        return ok ? $"RemoveQuestion: removed '{name}'" : $"RemoveQuestion: remove failed for '{name}'";
    }

    private static string ApplyMoveSection(FormDesignType tree, string? targetId)
    {
        var body = tree.Body;
        if (body is null) return "MoveSection: no Body section";

        var sections = body.ChildItemsNode?.ChildItemsList?.OfType<SectionItemType>().ToList();
        if (sections is null || sections.Count < 2) return "MoveSection: fewer than 2 sections, skip";

        var toMove = targetId is null
            ? sections[^1]
            : sections.FirstOrDefault(s => s.name == targetId || s.sGuid == targetId) ?? sections[^1];

        var childItems = body.GetChildItemsNode();
        bool ok = toMove.Move(childItems, 0);
        return ok ? $"MoveSection: moved '{toMove.name}' to position 0" : $"MoveSection: move failed for '{toMove.name}'";
    }

    private static string ApplyChangeAttribute(FormDesignType tree, string? targetId)
    {
        SectionItemType? section = null;
        if (targetId is not null)
        {
            section = tree.Nodes.Values.OfType<SectionItemType>()
                          .FirstOrDefault(n => n.name == targetId || n.sGuid == targetId);
        }
        section ??= tree.Nodes.Values.OfType<SectionItemType>().FirstOrDefault();

        if (section is null) return "ChangeAttribute: no section found";
        var prev = section.title ?? section.name;
        section.title = (section.title ?? section.name) + "_Changed";
        return $"ChangeAttribute: '{prev}' -> '{section.title}'";
    }

    private static string ApplyBulkMutation(FormDesignType tree)
    {
        var r1 = ApplyAddSection(tree, null);
        var r2 = ApplyRemoveQuestion(tree, null);
        var r3 = ApplyChangeAttribute(tree, null);
        return $"Bulk: [{r1}] | [{r2}] | [{r3}]";
    }
}
