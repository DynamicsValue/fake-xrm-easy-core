
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
    public static partial class FetchXmlExtensions
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
            if(hasPageInfoAttributes)
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
    }
    
}