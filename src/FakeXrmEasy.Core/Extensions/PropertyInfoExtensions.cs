using System.Reflection;

namespace FakeXrmEasy.Core.Extensions
{
    internal static class PropertyInfoExtensions
    {
        internal static bool IsEnumerable(this PropertyInfo propertyInfo)
        {
            return propertyInfo.PropertyType.Name == "IEnumerable`1";
        }
    }
}