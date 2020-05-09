using Microsoft.Xrm.Sdk;
using System;
using System.Linq;
using System.Reflection;

namespace FakeXrmEasy.Extensions
{
    public static class TypeExtensions
    {
        public static bool IsOptionSet(this Type t)
        {
            var nullableType = Nullable.GetUnderlyingType(t);
            return t == typeof(OptionSetValue)
                   || t.IsEnum
                   || nullableType != null && nullableType.IsEnum;
        }

#if FAKE_XRM_EASY_9
        public static bool IsOptionSetValueCollection(this Type t)
        {
            var nullableType = Nullable.GetUnderlyingType(t);
            return t == typeof(OptionSetValueCollection);
        }
#endif

        public static bool IsDateTime(this Type t)
        {
            var nullableType = Nullable.GetUnderlyingType(t);
            return t == typeof(DateTime)
                   || nullableType != null && nullableType == typeof(DateTime);
        }

        public static bool IsNullableEnum(this Type t)
        {
            return
                t.IsGenericType
                && t.GetGenericTypeDefinition() == typeof(Nullable<>)
                && t.GetGenericArguments()[0].IsEnum;
        }

        public static PropertyInfo GetEarlyBoundTypeAttribute(this Type earlyBoundType, string attributeName)
        {
            var attributeInfo = earlyBoundType.GetProperties()
                .Where(pi => pi.GetCustomAttributes(typeof(AttributeLogicalNameAttribute), true).Length > 0)
                .Where(pi => (pi.GetCustomAttributes(typeof(AttributeLogicalNameAttribute), true)[0] as AttributeLogicalNameAttribute).LogicalName.Equals(attributeName))
                .FirstOrDefault();

            return attributeInfo;
        }
    }
}