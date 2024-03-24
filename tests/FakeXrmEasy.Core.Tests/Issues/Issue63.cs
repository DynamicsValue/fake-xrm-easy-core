using Crm;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using FakeXrmEasy.Query;
using Xunit;

namespace FakeXrmEasy.Core.Tests.Issues
{
    // https://github.com/DynamicsValue/fake-xrm-easy/issues/63

    public class Issue63 : FakeXrmEasyTestsBase
    {
        // setup
        public Issue63()
        {
            _context.EnableProxyTypes(typeof(Account).Assembly);

            var order = new SalesOrder() { Id = Guid.NewGuid() };
            
            var uom = new UoM()
            {
                Id = Guid.NewGuid(),
                Name = "KG"
            };
            var detail1 = new SalesOrderDetail()
            {
                Id = Guid.NewGuid(),
                SalesOrderId = order.ToEntityReference(),
                Quantity = 1,
                ProductDescription = "A",
                Description = "B",
                UoMId = uom.ToEntityReference()
            };
            var detail2 = new SalesOrderDetail()
            {
                Id = Guid.NewGuid(),
                SalesOrderId = order.ToEntityReference(),
                Quantity = 1,
                ProductDescription = "C",
                Description = "D",
                UoMId = uom.ToEntityReference()
            };
            var detail3 = new SalesOrderDetail()
            {
                Id = Guid.NewGuid(),
                SalesOrderId = order.ToEntityReference(),
                Quantity = 1,
                ProductDescription = "E",
                Description = "F",
                UoMId = uom.ToEntityReference()
            };

            _context.Initialize(new Entity[] { order, detail1, detail2, detail3, uom });
        }

        // This test currently fails
        [Fact]
        public void When_A_QueryExpression_Contains_A_Complex_subquery_On_A_Link_Entity_It_Should_Return_The_Right_Records()
        {
            var query = new QueryExpression("salesorder");
            // link to salesorderdetail
            var query_salesorderdetail = query.AddLink("salesorderdetail", "salesorderid", "salesorderid");
            // Quantity not null
            query_salesorderdetail.LinkCriteria.AddCondition("quantity", ConditionOperator.NotNull);
            var query_currency =
                query_salesorderdetail.AddLink("uom", "uomid", "uomid");
            query_currency.LinkCriteria.AddCondition("name", ConditionOperator.Equal, "KG");

            // Add an 'Or' filter
            var orFilter = new FilterExpression();
            query_salesorderdetail.LinkCriteria.AddFilter(orFilter);
            orFilter.FilterOperator = LogicalOperator.Or;

            // Filter with two Ands - A and B - should find detail1
            var aAndBFilter = new FilterExpression();
            aAndBFilter.AddCondition("productdescription", ConditionOperator.Equal, "A");
            aAndBFilter.AddCondition("description", ConditionOperator.Equal, "B");

            // Filter with two Ands - C and D - should find detail2
            var cAndDFilter = new FilterExpression();
            cAndDFilter.AddCondition("productdescription", ConditionOperator.Equal, "C");
            cAndDFilter.AddCondition("description", ConditionOperator.Equal, "D");

            // Add the two and filters to the Or Filter
            orFilter.AddFilter(aAndBFilter);
            orFilter.AddFilter(cAndDFilter);

            var records = _service.RetrieveMultiple(query);

            Assert.Equal(2, records.Entities.Count);
        }
    }
}