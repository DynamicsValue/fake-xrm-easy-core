
using System;
using System.Linq;
using System.Xml.Linq;
using FakeXrmEasy.Abstractions;
using FakeXrmEasy.Extensions.FetchXml;
using Microsoft.Xrm.Sdk.Query;

namespace FakeXrmEasy.Query
{
    public static class FetchXmlExtensions
    {
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

            query.PageInfo.Count = xlDoc.ToCount() ?? 0;
            query.PageInfo.PageNumber = xlDoc.ToPageNumber() ?? 1;
            query.PageInfo.ReturnTotalRecordCount = xlDoc.ToReturnTotalRecordCount();

            var linkedEntities = xlDoc.ToLinkEntities(context);
            foreach (var le in linkedEntities)
            {
                query.LinkEntities.Add(le);
            }

            return query;
        }

        public static XDocument ToXmlDocument(this string fetchXml)
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