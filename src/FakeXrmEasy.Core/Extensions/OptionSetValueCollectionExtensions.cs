#if FAKE_XRM_EASY_9

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xrm.Sdk;

namespace FakeXrmEasy.Extensions
{
    /// <summary>
    /// Extension methods for OptionSetValue Collection
    /// </summary>
    public static class OptionSetValueCollectionExtensions
    {
        
        /// <summary>
        /// Converts current OptionSetValueCollection to a HashSet&lt;int&gt; values
        /// </summary>
        /// <param name="input"></param>
        /// <param name="isOptionSetValueCollectionAccepted"></param>
        /// <returns></returns>
        public static HashSet<int> ConvertToHashSetOfInt(this object input, bool isOptionSetValueCollectionAccepted)
        {
            if (input == null) return null;
            
            var set = new HashSet<int>();

            var faultReason = $"The formatter threw an exception while trying to deserialize the message: There was an error while trying to deserialize parameter" +
                $" http://schemas.microsoft.com/xrm/2011/Contracts/Services:query. The InnerException message was 'Error in line 1 position 8295. Element " +
                $"'http://schemas.microsoft.com/2003/10/Serialization/Arrays:anyType' contains data from a type that maps to the name " +
                $"'http://schemas.microsoft.com/xrm/2011/Contracts:{input.GetType()}'. The deserializer has no knowledge of any type that maps to this name. " +
                $"Consider changing the implementation of the ResolveName method on your DataContractResolver to return a non-null value for name " +
                $"'{input.GetType()}' and namespace 'http://schemas.microsoft.com/xrm/2011/Contracts'.'.  Please see InnerException for more details.";

            var type = input.GetType();
            
            if (input is int)
            {
                set.Add((int)input);
            }
            else if (input is string)
            {
                set.Add(int.Parse(input as string));
            }
            else if (input is int[])
            {
                set.UnionWith(input as int[]);
            }
            else if (input is string[])
            {
                set.UnionWith((input as string[]).Select(s => int.Parse(s)));
            }
            else if (input is DataCollection<object>)
            {
                var collection = input as DataCollection<object>;

                if (collection.All(o => o is int))
                {
                    set.UnionWith(collection.Cast<int>());
                }
                else if (collection.All(o => o is string))
                {
                    set.UnionWith(collection.Select(o => int.Parse(o as string)));
                }
                else if (collection.Count == 1 && collection[0] is int[])
                {
                    set.UnionWith(collection[0] as int[]);
                }
                else if (collection.Count == 1 && collection[0] is string[])
                {
                    set.UnionWith((collection[0] as string[]).Select(s => int.Parse(s)));
                }
                else
                {
                    throw FakeOrganizationServiceFaultFactory.New(faultReason);
                }
            }
            else if (isOptionSetValueCollectionAccepted && input is OptionSetValueCollection)
            {
                set.UnionWith((input as OptionSetValueCollection).Select(osv => osv.Value));
            }
            else if (typeof(IEnumerable).IsAssignableFrom(type))
            {
                if (type.IsGenericType)
                {
                    var genericTypeArguments = type.GenericTypeArguments;
                    if (genericTypeArguments.Length == 1 && genericTypeArguments[0].IsEnum)
                    {
                        var enumerator = (input as IEnumerable).GetEnumerator();
                        while (enumerator.MoveNext())
                        {
                            set.Add((int)enumerator.Current);
                        }
                    }
                }
            }
            else
            {
                throw FakeOrganizationServiceFaultFactory.New(faultReason);
            }

            return set;
        }
    }
}
#endif