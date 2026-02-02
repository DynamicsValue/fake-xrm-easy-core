#if FAKE_XRM_EASY_9
using System;
using System.Collections.Generic;
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
                if (attrEx.HasGroupBy && e.Contains(attrEx.AttributeName) && e[attrEx.AttributeName] != null)
                {
                    if (attrEx.DateTimeGrouping == XrmDateTimeGrouping.None)
                    {
                        _attributes.Add(attrEx.Alias, new AliasedValue(e.LogicalName, attrEx.AttributeName, e[attrEx.AttributeName]));
                    }
                    else
                    {
                        object value = e[attrEx.AttributeName];
                        if (value == null)
                        {
                            _attributes.Add(attrEx.Alias, new AliasedValue(e.LogicalName, attrEx.AttributeName, e[attrEx.AttributeName]));
                        }
                        else
                        {
                            var dateTimeValue = (DateTime)value;
                            switch (attrEx.DateTimeGrouping)
                            {
                                case XrmDateTimeGrouping.Year:
                                    _attributes.Add(attrEx.Alias, new AliasedValue(e.LogicalName, attrEx.AttributeName, dateTimeValue.Year));
                                    break;
                                case XrmDateTimeGrouping.Month:
                                    _attributes.Add(attrEx.Alias, new AliasedValue(e.LogicalName, attrEx.AttributeName, dateTimeValue.Month));
                                    break;
                                case XrmDateTimeGrouping.Day:
                                    _attributes.Add(attrEx.Alias, new AliasedValue(e.LogicalName, attrEx.AttributeName, dateTimeValue.Day));
                                    break;
                                case XrmDateTimeGrouping.Week:
                                    _attributes.Add(attrEx.Alias, new AliasedValue(e.LogicalName, attrEx.AttributeName, Week.GetWeek(dateTimeValue)));
                                    break;
                                case XrmDateTimeGrouping.Quarter:
                                    _attributes.Add(attrEx.Alias, new AliasedValue(e.LogicalName, attrEx.AttributeName, Quarter.GetQuarter(dateTimeValue)));
                                    break;
                            }
                        }
                       
                    }
                    
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

        public override bool Equals(object obj)
        {
            if (!(obj is EntityGroupKey))
            {
                return false;
            }

            var otherEntityGroupKey = (EntityGroupKey)obj;
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
                
                var aliasedValue = thisValue as AliasedValue;
                var otherAliasedValue = otherValue as AliasedValue;
                
                if (!aliasedValue?.Value?.Equals(otherAliasedValue?.Value) == true)
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
                hashCode ^= attr.Value != null ? attr.Key.GetHashCode() ^ ((AliasedValue)attr.Value).Value.GetHashCode() : attr.Key.GetHashCode();
            }

            return hashCode;
        }
    }
}
#endif