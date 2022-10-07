using System.Linq;
using System.Reflection;
using System.Xml.Serialization;

namespace SDC.Type1.Extensions
{
    public static class TypeExtensions
    {
        public static IOrderedEnumerable<PropertyInfo> XmlPropertyInfo(this System.Type typeNode)
        {
            var applicableProperties = typeNode.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

            return applicableProperties
                .Where(p => p.IsDefined(typeof(XmlElementAttribute)))
                .OrderBy(p => p.GetCustomAttributes<XmlElementAttribute>().First().Order);
        }
    }
}