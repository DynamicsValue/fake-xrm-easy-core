
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using FakeXrmEasy.Abstractions;
using FakeXrmEasy.Core.Exceptions.Query.FetchXml.Aggregations;
using FakeXrmEasy.Extensions.FetchXml;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace FakeXrmEasy.Query
{
    /// <summary>
    /// Extensions for FetchXml manipulation
    /// </summary>
    public static class FetchXmlExtensions
    {
        /// <summary>
        /// Converts a string fetchXml into a query expression
        /// </summary>
        /// <param name="fetchXml"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public static QueryExpression ToQueryExpression(this string fetchXml, IXrmFakedContext context)
        {
            var xlDoc = fetchXml.ToXmlDocument();
            return xlDoc.ToQueryExpression(context);
        }

        private static QueryExpression ToQueryExpression(this XDocument xlDoc, IXrmFakedContext context)
        {
            //Validate nodes
            if (!xlDoc.Descendants().All(el => el.IsFetchXmlNodeValid()))
                throw new Exception("At least some node is not valid");

            //Root node
            if (!xlDoc.Root.Name.LocalName.Equals("fetch"))
            {
                throw new Exception("Root node must be fetch");
            }

            var entityNode = xlDoc.RetrieveFetchXmlNode("entity");
            var query = new QueryExpression(entityNode.GetAttribute("name").Value);

            query.ColumnSet = xlDoc.ToColumnSet();

            // Ordering is done after grouping/aggregation
            if (!xlDoc.HasAggregations())
            {
                var orders = xlDoc.ToOrderExpressionList();
                foreach (var order in orders)
                {
                    query.AddOrder(order.AttributeName, order.OrderType);
                }
            }

            query.Distinct = xlDoc.IsDistincFetchXml();

            query.Criteria = xlDoc.ToCriteria(context);

            query.TopCount = xlDoc.ToTopCount();

            int? count = xlDoc.ToCount();
            int? pageNumber = xlDoc.ToPageNumber();
            bool? returnTotalRecordCount = xlDoc.ToReturnTotalRecordCount();

            bool hasPageInfoAttributes = count != null
                                            || pageNumber != null
                                            || returnTotalRecordCount != null;

            query.PageInfo = null;
            if (hasPageInfoAttributes)
            {
                query.PageInfo = new PagingInfo();
                query.PageInfo.Count = count ?? 0;
                query.PageInfo.PageNumber = pageNumber ?? 1;
                query.PageInfo.ReturnTotalRecordCount = returnTotalRecordCount ?? false;
            }

            var linkedEntities = xlDoc.ToLinkEntities(context);
            foreach (var le in linkedEntities)
            {
                query.LinkEntities.Add(le);
            }

            return query;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fetchXml"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        internal static XDocument ToXmlDocument(this string fetchXml)
        {
            try
            {
                return XDocument.Parse(fetchXml);
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("FetchXml must be a valid XML document: {0}", ex.ToString()));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="resultOfQuery"></param>
        /// <param name="ctx"></param>
        /// <param name="xmlDoc"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        /// <exception cref="UnknownAggregateFunctionException"></exception>
        internal static List<Entity> Aggregate(this List<Entity> resultOfQuery, XrmFakedContext ctx, XDocument xmlDoc)
        {
            // Validate that <all-attributes> is not present,
            // that all attributes have groupby or aggregate, and an alias,
            // and that there is exactly 1 groupby.
            if (xmlDoc.RetrieveFetchXmlNode("all-attributes") != null)
            {
                throw new Exception("Can't have <all-attributes /> present when using aggregate");
            }

            var ns = xmlDoc.Root.Name.Namespace;

            var entityName = xmlDoc.RetrieveFetchXmlNode("entity")?.GetAttribute("name")?.Value;
            if (string.IsNullOrEmpty(entityName))
            {
                throw new Exception("Can't find entity name for aggregate query");
            }

            var aggregates = new List<FetchAggregate>();
            var groups = new List<FetchGrouping>();

            foreach (var attr in xmlDoc.Descendants(ns + "attribute"))
            {
                //TODO: Find entity alias. Handle aliasedvalue in the query result.
                var namespacedAlias = attr.Ancestors(ns + "link-entity").Select(x => x.GetAttribute("alias")?.Value != null ? x.GetAttribute("alias").Value : x.GetAttribute("name").Value).ToList();
                var alias = attr.GetAttribute("alias")?.Value;
                namespacedAlias.Add(attr.GetAttribute("name")?.Value);
                var logicalName = string.Join(".", namespacedAlias);

                if (string.IsNullOrEmpty(alias))
                {
                    throw new Exception("Missing alias for attribute in aggregate fetch xml");
                }
                if (string.IsNullOrEmpty(logicalName))
                {
                    throw new Exception("Missing name for attribute in aggregate fetch xml");
                }

                if (attr.IsAttributeTrue("groupby"))
                {
                    var dategrouping = attr.GetAttribute("dategrouping")?.Value;
                    if (dategrouping != null)
                    {
                        DateGroupType t;
                        if (!Enum.TryParse(dategrouping, true, out t))
                        {
                            throw new Exception("Unknown dategrouping value '" + dategrouping + "'");
                        }
                        groups.Add(new DateTimeGroup()
                        {
                            Type = t,
                            OutputAlias = alias,
                            Attribute = logicalName
                        });
                    }
                    else
                    {
                        groups.Add(new SimpleValueGroup()
                        {
                            OutputAlias = alias,
                            Attribute = logicalName
                        });
                    }
                }
                else
                {
                    var agrFn = attr.GetAttribute("aggregate")?.Value;
                    if (string.IsNullOrEmpty(agrFn))
                    {
                        throw new Exception("Attributes must have be aggregated or grouped by when using aggregation");
                    }

                    FetchAggregate newAgr = null;
                    switch (agrFn?.ToLower())
                    {
                        case "count":
                            newAgr = new CountAggregate();
                            break;

                        case "countcolumn":
                            if (attr.IsAttributeTrue("distinct"))
                            {
                                newAgr = new CountDistinctAggregate();
                            }
                            else
                            {
                                newAgr = new CountColumnAggregate();
                            }
                            break;

                        case "min":
                            newAgr = new MinAggregate();
                            break;

                        case "max":
                            newAgr = new MaxAggregate();
                            break;

                        case "avg":
                            newAgr = new AvgAggregate();
                            break;

                        case "sum":
                            newAgr = new SumAggregate();
                            break;

                        default:
                            throw new UnknownAggregateFunctionException(agrFn);
                    }

                    newAgr.OutputAlias = alias;
                    newAgr.Attribute = logicalName;
                    aggregates.Add(newAgr);
                }
            }

            List<Entity> aggregateResult;

            if (groups.Any())
            {
                aggregateResult = ProcessGroupedAggregate(entityName, resultOfQuery, aggregates, groups);
            }
            else
            {
                aggregateResult = new List<Entity>();
                var ent = ProcessAggregatesForSingleGroup(entityName, resultOfQuery, aggregates);
                aggregateResult.Add(ent);
            }

            return OrderAggregateResult(xmlDoc, aggregateResult.AsQueryable());
        }

        private static List<Entity> OrderAggregateResult(XDocument xmlDoc, IQueryable<Entity> result)
        {
            var ns = xmlDoc.Root.Name.Namespace;
            var orderByElements = xmlDoc.Root.Element(ns + "entity").Elements(ns + "order");

            foreach (var order in orderByElements)
            {
                var alias = order.GetAttribute("alias")?.Value;

                // These error is also thrown by CRM
                if (order.GetAttribute("attribute") != null)
                {
                    throw new Exception("An attribute cannot be specified for an order clause for an aggregate Query. Use an alias");
                }
                if (string.IsNullOrEmpty(alias))
                {
                    throw new Exception("An alias is required for an order clause for an aggregate Query.");
                }

                if (order.IsAttributeTrue("descending"))
                    result = result.OrderByDescending(e => e.Attributes.ContainsKey(alias) ? e.Attributes[alias] : null, new XrmOrderByAttributeComparer());
                else
                    result = result.OrderBy(e => e.Attributes.ContainsKey(alias) ? e.Attributes[alias] : null, new XrmOrderByAttributeComparer());
            }

            return result.ToList();
        }

        private static Entity ProcessAggregatesForSingleGroup(string entityName, IEnumerable<Entity> entities, IList<FetchAggregate> aggregates)
        {
            var ent = new Entity(entityName);

            foreach (var agg in aggregates)
            {
                var val = agg.Process(entities);
                if (val != null)
                {
                    ent[agg.OutputAlias] = new AliasedValue(null, agg.Attribute, val);
                }
                else
                {
                    //if the aggregate value cannot be calculated
                    //CRM still returns an alias
                    ent[agg.OutputAlias] = new AliasedValue(null, agg.Attribute, null);
                }
            }

            return ent;
        }

        private static List<Entity> ProcessGroupedAggregate(string entityName, IList<Entity> resultOfQuery, IList<FetchAggregate> aggregates, IList<FetchGrouping> groups)
        {
            // Group by the groupBy-attribute
            var grouped = resultOfQuery.GroupBy(e =>
            {
                return groups
                    .Select(g => g.Process(e))
                    .ToArray();
            }, new ArrayComparer());

            // Perform aggregates in each group
            var result = new List<Entity>();
            foreach (var g in grouped)
            {
                var firstInGroup = g.First();

                // Find the aggregates values in the group
                var ent = ProcessAggregatesForSingleGroup(entityName, g, aggregates);

                // Find the group values
                for (var rule = 0; rule < groups.Count; ++rule)
                {
                    if (g.Key[rule] != null)
                    {
                        object value = g.Key[rule];
                        ent[groups[rule].OutputAlias] = new AliasedValue(null, groups[rule].Attribute, value is ComparableEntityReference ? (value as ComparableEntityReference).entityReference : value);
                    }
                }

                result.Add(ent);
            }

            return result;
        }

        private abstract class FetchAggregate
        {
            public string Attribute { get; set; }
            public string OutputAlias { get; set; }

            public object Process(IEnumerable<Entity> entities)
            {
                return AggregateValues(entities.Select(e =>
                    e.Contains(Attribute) ? e[Attribute] : null
                ));
            }

            protected abstract object AggregateValues(IEnumerable<object> values);
        }

        private abstract class AliasedAggregate : FetchAggregate
        {
            protected override object AggregateValues(IEnumerable<object> values)
            {
                var lst = values.Where(x => x != null);
                bool alisedValue = lst.FirstOrDefault() is AliasedValue;
                if (alisedValue)
                {
                    lst = lst.Select(x => (x as AliasedValue)?.Value);
                }

                return AggregateAliasedValues(lst);
            }

            protected abstract object AggregateAliasedValues(IEnumerable<object> values);
        }

        private class CountAggregate : FetchAggregate
        {
            protected override object AggregateValues(IEnumerable<object> values)
            {
                return values.Count();
            }
        }

        private class CountColumnAggregate : AliasedAggregate
        {
            protected override object AggregateAliasedValues(IEnumerable<object> values)
            {
                return values.Where(x => x != null).Count();
            }
        }

        private class CountDistinctAggregate : AliasedAggregate
        {
            protected override object AggregateAliasedValues(IEnumerable<object> values)
            {
                return values.Where(x => x != null).Distinct().Count();
            }
        }

        private class MinAggregate : AliasedAggregate
        {
            protected override object AggregateAliasedValues(IEnumerable<object> values)
            {
                var lst = values.Where(x => x != null);
                if (!lst.Any()) return null;

                var firstValue = lst.Where(x => x != null).First();
                var valType = firstValue.GetType();

                if (valType == typeof(decimal) || valType == typeof(decimal?))
                {
                    return lst.Min(x => (decimal)x);
                }

                if (valType == typeof(Money))
                {
                    return new Money(lst.Min(x => (x as Money).Value));
                }

                if (valType == typeof(int) || valType == typeof(int?))
                {
                    return lst.Min(x => (int)x);
                }

                if (valType == typeof(float) || valType == typeof(float?))
                {
                    return lst.Min(x => (float)x);
                }

                if (valType == typeof(double) || valType == typeof(double?))
                {
                    return lst.Min(x => (double)x);
                }

                if (valType == typeof(DateTime) || valType == typeof(DateTime?))
                {
                    return lst.Min(x => (DateTime)x);
                }

                throw new UnhandledPropertyTypeException(valType.FullName, "min");
            }
        }

        private class MaxAggregate : AliasedAggregate
        {
            protected override object AggregateAliasedValues(IEnumerable<object> values)
            {
                var lst = values.Where(x => x != null);
                if (!lst.Any()) return null;

                var firstValue = lst.First();
                var valType = firstValue.GetType();

                if (valType == typeof(decimal) || valType == typeof(decimal?))
                {
                    return lst.Max(x => (decimal)x);
                }

                if (valType == typeof(Money))
                {
                    return new Money(lst.Max(x => (x as Money).Value));
                }

                if (valType == typeof(int) || valType == typeof(int?))
                {
                    return lst.Max(x => (int)x);
                }

                if (valType == typeof(float) || valType == typeof(float?))
                {
                    return lst.Max(x => (float)x);
                }

                if (valType == typeof(double) || valType == typeof(double?))
                {
                    return lst.Max(x => (double)x);
                }

                if (valType == typeof(DateTime) || valType == typeof(DateTime?))
                {
                    return lst.Max(x => (DateTime)x);
                }

                throw new UnhandledPropertyTypeException(valType.FullName, "max");
            }
        }

        private class AvgAggregate : AliasedAggregate
        {
            protected override object AggregateAliasedValues(IEnumerable<object> values)
            {
                var lst = values.Where(x => x != null);
                if (!lst.Any()) return null;

                var firstValue = lst.First();
                var valType = firstValue.GetType();

                if (valType == typeof(decimal) || valType == typeof(decimal?))
                {
                    return lst.Average(x => (decimal)x);
                }

                if (valType == typeof(Money))
                {
                    return new Money(lst.Average(x => (x as Money).Value));
                }

                if (valType == typeof(int) || valType == typeof(int?))
                {
                    return lst.Average(x => (int)x);
                }

                if (valType == typeof(float) || valType == typeof(float?))
                {
                    return lst.Average(x => (float)x);
                }

                if (valType == typeof(double) || valType == typeof(double?))
                {
                    return lst.Average(x => (double)x);
                }

                throw new UnhandledPropertyTypeException(valType.FullName, "avg");
            }
        }

        private class SumAggregate : AliasedAggregate
        {
            protected override object AggregateAliasedValues(IEnumerable<object> values)
            {
                var lst = values.ToList().Where(x => x != null);
                // TODO: Check these cases in CRM proper
                if (!lst.Any()) return null;

                var valType = lst.First().GetType();

                if (valType == typeof(decimal) || valType == typeof(decimal?))
                {
                    return lst.Sum(x => x as decimal? ?? 0m);
                }
                if (valType == typeof(Money))
                {
                    return new Money(lst.Sum(x => (x as Money)?.Value ?? 0m));
                }

                if (valType == typeof(int) || valType == typeof(int?))
                {
                    return lst.Sum(x => x as int? ?? 0);
                }

                if (valType == typeof(float) || valType == typeof(float?))
                {
                    return lst.Sum(x => x as float? ?? 0f);
                }

                if (valType == typeof(double) || valType == typeof(double?))
                {
                    return lst.Sum(x => x as double? ?? 0d);
                }

                throw new Exception("Unhndled property type '" + valType.FullName + "' in 'sum' aggregate");
            }
        }

        private abstract class FetchGrouping
        {
            public string Attribute { get; set; }
            public string OutputAlias { get; set; }

            public IComparable Process(Entity entity)
            {
                var attr = entity.Contains(Attribute) ? entity[Attribute] : null;
                return FindGroupValue(attr);
            }

            public abstract IComparable FindGroupValue(object attributeValue);
        }

        /// <summary>
        /// Used to compare array of objects, in order to group by a variable number of conditions.
        /// </summary>
        private class ArrayComparer : IEqualityComparer<IComparable[]>
        {
            public bool Equals(IComparable[] x, IComparable[] y)
            {
                return x.SequenceEqual(y);
            }

            public int GetHashCode(IComparable[] obj)
            {
                int result = 0;
                foreach (IComparable x in obj)
                {
                    result ^= x == null ? 0 : x.GetHashCode();
                }
                return result;
            }
        }

        private class ComparableEntityReference : IComparable
        {
            public EntityReference entityReference { get; private set; }

            public ComparableEntityReference(EntityReference entityReference)
            {
                this.entityReference = entityReference;
            }

            int IComparable.CompareTo(object obj)
            {
                return Equals(obj) ? 0 : 1;
            }

            public override bool Equals(object obj)
            {
                EntityReference other;
                if (obj is EntityReference)
                {
                    other = obj as EntityReference;
                }
                else if (obj is ComparableEntityReference)
                {
                    other = (obj as ComparableEntityReference).entityReference;
                }
                else
                {
                    return false;
                }
                return entityReference.Id == other.Id && entityReference.LogicalName == other.LogicalName;
            }

            public override int GetHashCode()
            {
                return (entityReference.LogicalName == null ? 0 : entityReference.LogicalName.GetHashCode()) ^ entityReference.Id.GetHashCode();
            }
        }

        private class SimpleValueGroup : FetchGrouping
        {
            public override IComparable FindGroupValue(object attributeValue)
            {
                if (attributeValue is AliasedValue aliasedValue)
                {
                    attributeValue = aliasedValue.Value;
                }

                if (attributeValue is EntityReference)
                {
                    return new ComparableEntityReference(attributeValue as EntityReference) as IComparable;
                }
                else if (attributeValue is OptionSetValue)
                {
                    return ((OptionSetValue)attributeValue).Value as IComparable;
                }
                else
                {
                    return attributeValue as IComparable;
                }
            }
        }

        private enum DateGroupType
        {
            DateTime,
            Day,
            Week,
            Month,
            Quarter,
            Year
        }

        private class DateTimeGroup : FetchGrouping
        {
            public DateGroupType Type { get; set; }

            public override IComparable FindGroupValue(object attributeValue)
            {
                if (attributeValue == null) return null;

                if (!(attributeValue is DateTime || attributeValue is DateTime?))
                {
                    throw new Exception("Can only do date grouping of DateTime values");
                }

                var d = attributeValue as DateTime?;

                switch (Type)
                {
                    case DateGroupType.DateTime:
                        return d;

                    case DateGroupType.Day:
                        return d?.Day;

                    case DateGroupType.Week:
                        var cal = System.Globalization.DateTimeFormatInfo.InvariantInfo;
                        return cal.Calendar.GetWeekOfYear(d.Value, cal.CalendarWeekRule, cal.FirstDayOfWeek);

                    case DateGroupType.Month:
                        return d?.Month;

                    case DateGroupType.Quarter:
                        return (d?.Month + 2) / 3;

                    case DateGroupType.Year:
                        return d?.Year;

                    default:
                        throw new Exception("Unhandled date group type");
                }
            }
        }
    }

}