namespace SDC.Schema
{
    using Newtonsoft.Json.Serialization;
    using Newtonsoft.Json;
    using System.Reflection;

    // Shared contract resolver that deserializes by setting backing fields directly
    // to avoid invoking property setters (which perform validation and tree-mutation
    // logic) during deserialization.
    internal class SdcNoSetterContractResolver : DefaultContractResolver
    {
        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            var prop = base.CreateProperty(member, memberSerialization);
            if (prop.Writable)
            {
                var declaring = member.DeclaringType;
                if (declaring != null)
                {
                    string name = member.Name;
                    string[] candidates = new[] { "_" + name, "_" + (char.ToLowerInvariant(name[0]) + name.Substring(1)) };
                    FieldInfo? field = null;
                    foreach (var c in candidates)
                    {
                        field = declaring.GetField(c, BindingFlags.Instance | BindingFlags.NonPublic);
                        if (field != null) break;
                    }
                    if (field != null)
                    {
                        prop.ValueProvider = new ReflectionValueProvider(field);
                    }
                }
            }
            return prop;
        }
    }
}
