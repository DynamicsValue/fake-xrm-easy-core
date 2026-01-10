using System.Collections.Generic;
using System.Linq;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace FakeXrmEasy.Core.Query.Aggregations
{
    internal class EntityGroupKey
    {
        internal readonly Dictionary<string, object> _attributes;
        
        internal EntityGroupKey(Entity e, List<XrmAttributeExpression> attributeExpressions)
        {
            _attributes = new Dictionary<string, object>();
            foreach (var attrEx in attributeExpressions)
            {
                if (attrEx.HasGroupBy && e.Contains(attrEx.AttributeName))
                {
                    _attributes.Add(attrEx.Alias, e[attrEx.AttributeName]);
                }
                else
                {
                    _attributes.Add(attrEx.Alias, null);
                }
            }
        }

        /// <summary>
        /// Adds the current attribute keys in the current EntityGroupKey to the specified entity
        /// </summary>
        /// <param name="e"></param>
        internal void AddGroupKeyAttributes(Entity e)
        {
            foreach (var attr in _attributes)
            {
                e[attr.Key] = attr.Value;
            }
        }

        public override bool Equals(object other)
        {
            if (!(other is EntityGroupKey))
            {
                return false;
            }

            var otherEntityGroupKey = (EntityGroupKey)other;
            var keysLength = _attributes.Keys.Count;
            var otherKeysLength = otherEntityGroupKey._attributes.Keys.Count;

            if (keysLength != otherKeysLength)
            {
                return false;
            }

            foreach (var key in _attributes.Keys)
            {
                if (!otherEntityGroupKey._attributes.ContainsKey(key))
                {
                    return false;
                }
                
                var thisValue = _attributes[key];
                var otherValue = otherEntityGroupKey._attributes[key];

                if (thisValue == null && otherValue != null ||
                    thisValue != null && otherValue == null)
                {
                    return false;
                }

                if (thisValue == null)
                {
                    continue;
                }
                
                if (!thisValue.Equals(otherValue))
                {
                    return false;
                }
            }
            
            return true;
        }

        public override int GetHashCode()
        {
            int hashCode = 0;
            foreach (var attr in _attributes)
            {
                hashCode ^= attr.Value != null ? attr.Key.GetHashCode() ^ attr.Value.GetHashCode() : attr.Key.GetHashCode();
            }

            return hashCode;
        }
    }
}