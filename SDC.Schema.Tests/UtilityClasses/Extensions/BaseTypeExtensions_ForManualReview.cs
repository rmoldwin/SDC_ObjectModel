using CSharpVitamins;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SDC.Schema;
using SDC.Schema.Extensions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace SDC.Schema.Tests.UtilityClasses.Extensions
{
    /// <summary>
    /// Output-heavy diagnostic tests designed for manual review of printed results.
    /// These tests print extensive structured output to the console/debug stream
    /// intended to be read by a developer, not asserted programmatically.
    /// They DO fail on unexpected exceptions so regressions are still visible.
    /// </summary>
    [TestClass]
    public class BaseTypeExtensions_ForManualReview
    {
        // ─── Shared helper ───────────────────────────────────────────────────────────

        /// <summary>
        /// Builds a two-level attribute dictionary for every IET node in <paramref name="topNode"/>.
        /// Key: IET sGuid → subNode sGuid → serialized <see cref="AttributeInfo"/> list.
        /// Used by <see cref="CompareVersions"/> and can be reused by other manual-review methods.
        /// </summary>
        public static SortedList<string, Dictionary<string, List<AttributeInfo>>>
            GetXmlAttributesFilledTree(ITopNode topNode)
        {
            SortedList<string, Dictionary<string, List<AttributeInfo>>> dictAttr = new();

            foreach (IdentifiedExtensionType iet in topNode.IETnodes)
            {
                Dictionary<string, List<AttributeInfo>> dlai = new();
                var sublist = SdcUtil.GetSortedNonIETsubtreeList(iet, -1, 0, false);
                if (sublist is not null)
                {
                    foreach (var subNode in sublist)
                    {
                        var lai = subNode.GetXmlAttributesSerialized();
                        dlai.Add(subNode.sGuid, lai);
                    }
                    dictAttr.Add(iet.sGuid, dlai);
                }
            }
            return dictAttr;
        }

        // ─── Manual-review tests ─────────────────────────────────────────────────────

        /// <summary>
        /// Compares two versions of a breast-staging SDC XML document attribute-by-attribute,
        /// printing changed, added, and removed attributes to the debug/console output.
        /// Review the printed diff table manually to identify inter-version attribute drift.
        /// This test FAILS if an unexpected exception is thrown; it does NOT assert the diff is empty.
        /// </summary>
        [TestMethod]
        public void CompareVersions()
        {
            Setup.TimerStart($"==>{Setup.CallerName()} Compare Setup Started");

            var pathV2 = Path.Combine("..", "..", "..", "Test files", "BreastStagingTest2v2.xml");
            var pathV1 = Path.Combine("..", "..", "..", "Test files", "BreastStagingTest2v1.xml");

            FormDesignType fdV2 = FormDesignType.DeserializeFromXml(File.ReadAllText(pathV2));
            FormDesignType fdV1 = FormDesignType.DeserializeFromXml(File.ReadAllText(pathV1));

            var slAttV2 = GetXmlAttributesFilledTree(fdV2);
            var slAttV1 = GetXmlAttributesFilledTree(fdV1);

            ConcurrentDictionary<string, DifNodeIET> dDifNodeIET = new();

            Setup.TimerPrintSeconds("  seconds: ", $"\r\n<=={Setup.CallerName()} Compare Setup Complete");
            Setup.TimerStart($"==>{Setup.CallerName()} Compare Started");

            var locker = new object();
            var eqAttCompare = new SdcSerializedAttComparer();

            slAttV2.AsParallel().ForAll(kv2 =>
            {
                string sGuidIET = kv2.Key;
                Guid GuidIET = ShortGuid.Decode(sGuidIET);
                bool isParChangedIET = false;
                bool isMovedIET = false;
                bool isNewIET = false;
                bool isRemovedIET = false;
                bool isAttListChanged = false;

                List<AttInfoDif> laiDif = new();
                Dictionary<string, List<AttInfoDif>> dlaiDif = new();
                dlaiDif.Add(sGuidIET, laiDif);

                IdentifiedExtensionType? ietV1;
                if (fdV1.Nodes.TryGetValue(GuidIET, out BaseType? value))
                    ietV1 = value as IdentifiedExtensionType;
                else ietV1 = null;

                if (ietV1 is not null)
                {
                    var ietV2 = fdV2.Nodes[GuidIET] as IdentifiedExtensionType;

                    if (ietV1.ParentNode?.sGuid != ietV2?.ParentNode?.sGuid)
                        isParChangedIET = true;

                    lock (locker)
                        if (ietV1.GetNodePreviousSib()?.sGuid != ietV2!.GetNodePreviousSib()?.sGuid)
                            isMovedIET = true;

                    if (slAttV1.TryGetValue(kv2.Key, out var dlaiV1))
                    {
                        var dlaiV2 = kv2.Value;
                        foreach (var sGuidV2 in dlaiV2.Keys)
                        {
                            var aiHashV1IET = new HashSet<SdcSerializedAtt>(eqAttCompare);
                            var aiHashV2IET = new HashSet<SdcSerializedAtt>(eqAttCompare);

                            dlaiV1.TryGetValue(sGuidV2, out var laiV1);

                            foreach (var aiV2 in dlaiV2[sGuidV2])
                            {
                                aiHashV2IET.Add(new(sGuidV2, aiV2));

                                if (laiV1 is not null)
                                {
                                    var aiV1 = laiV1.FirstOrDefault(a => a.Name == aiV2.Name);
                                    if (aiV1 != default)
                                    {
                                        aiHashV1IET.Add(new(sGuidV2, aiV1));
                                        if (aiV1.Value?.ToString() != aiV2.Value?.ToString())
                                        {
                                            laiDif.Add(new AttInfoDif(sGuidV2, aiV1, aiV2));
                                            isAttListChanged = true;
                                        }
                                    }
                                    else
                                    {
                                        laiDif.Add(new AttInfoDif(sGuidV2, default, aiV2));
                                        isAttListChanged = true;
                                    }
                                }
                                else
                                {
                                    laiDif.Add(new AttInfoDif(sGuidV2, default, aiV2));
                                    isAttListChanged = true;
                                }
                            }

                            foreach (var rem in aiHashV1IET.Except(aiHashV2IET, eqAttCompare))
                            {
                                laiDif.Add(new AttInfoDif(rem.sGuid, aiV1: rem.ai, default));
                                isAttListChanged = true;
                            }
                        }
                    }
                    else
                    {
                        // V1 attribute dictionary unexpectedly missing — log to debug output
                        Debug.Print($"WARNING: No V1 attribute dictionary found for IET sGuid={sGuidIET}");
                    }
                }
                else
                {
                    isNewIET = true;
                }

                DifNodeIET difNodeIET = new(sGuidIET, isParChangedIET, isMovedIET, isNewIET, isRemovedIET, isAttListChanged, dlaiDif);
                dDifNodeIET.AddOrUpdate(sGuidIET, difNodeIET, (_, _) => difNodeIET);
            });

            // Print summary for manual review
            int changed = dDifNodeIET.Values.Count(d => d.isAttListChanged);
            int moved   = dDifNodeIET.Values.Count(d => d.isMoved);
            int added   = dDifNodeIET.Values.Count(d => d.isNew);
            int parChg  = dDifNodeIET.Values.Count(d => d.isParChanged);

            Console.WriteLine($"\n=== CompareVersions Manual Review Summary ===");
            Console.WriteLine($"  Total IET nodes compared : {dDifNodeIET.Count}");
            Console.WriteLine($"  Attribute list changed   : {changed}");
            Console.WriteLine($"  Parent changed           : {parChg}");
            Console.WriteLine($"  Position moved           : {moved}");
            Console.WriteLine($"  New in V2                : {added}");

            foreach (var kv in dDifNodeIET.Where(d => d.Value.isAttListChanged))
            {
                Console.WriteLine($"\n  IET {kv.Key}:");
                foreach (var lkv in kv.Value.dlaiDif)
                    foreach (var dif in lkv.Value)
                        Console.WriteLine(
                            $"    subNode={dif.sGuidSubnode}  attr={dif.aiV2.Name}  " +
                            $"V1={dif.aiV1.Value}  V2={dif.aiV2.Value}");
            }

            Setup.TimerPrintSeconds("  seconds: ", $"\r\n<=={Setup.CallerName()} Complete");
            // No assertion on diff count — results are for manual review.
            // The test will fail automatically if any exception escapes above.
        }

        // ─── Local record types used only by this review class ───────────────────────

        /// <summary>Identifies a serialized attribute by subnode sGuid + AttributeInfo.</summary>
        public readonly record struct AttInfoDif(string sGuidSubnode, AttributeInfo aiV1, AttributeInfo aiV2);

        /// <summary>Captures per-IET-node diff metadata for inter-version comparison.</summary>
        public readonly record struct DifNodeIET(
            string sGuidIET,
            bool isParChanged,
            bool isMoved,
            bool isNew,
            bool isRemoved,
            bool isAttListChanged,
            Dictionary<string, List<AttInfoDif>> dlaiDif);
    }
}
