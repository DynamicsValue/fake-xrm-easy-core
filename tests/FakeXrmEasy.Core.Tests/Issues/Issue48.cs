using Crm;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using Xunit;

namespace FakeXrmEasy.Tests.Issues
{
    // https://github.com/DynamicsValue/fake-xrm-easy/issues/48

    public class Issue48 : FakeXrmEasyTestsBase
    {
        SalesOrder _salesOrder;
        String fetchXML;
        // setup
        public Issue48()
        {
            _context.EnableProxyTypes(typeof(SalesOrder).Assembly);

            _salesOrder = new SalesOrder() {Id = Guid.NewGuid(), Name = "O-12345" };

            SalesOrderDetail salesOrderDetail1 = new SalesOrderDetail() {Id = Guid.NewGuid(), SalesOrderId = _salesOrder.ToEntityReference() };

            _context.Initialize(new List<Entity> { _salesOrder, salesOrderDetail1 });

            fetchXML = @"<fetch aggregate='true'>
                                  <entity name='salesorderdetail'>
                                    <attribute name='salesorderdetailid' alias='count' aggregate='count' />
                                    <link-entity name='salesorder' to='salesorderid' from='salesorderid' alias='so' link-type='inner'>
                                      <attribute name='name' alias='name' groupby='true' />
                                      <order alias='name' />
                                    </link-entity>
                                  </entity>
                                </fetch>";            
        }


        // This test currently fails - The given key was not present in the dictionary - the grouped by 'name' attribute is not in the resultset
        [Fact]
        public void When_An_Aggregated_Query_Contains_Group_By_Attributes_These_Should_Be_Returned_In_The_Query_Results()
        {
            EntityCollection records = _service.RetrieveMultiple(new FetchExpression(fetchXML));
            
            // The 'name' column of the rist record should be the same as the orders name column
            Assert.Equal(_salesOrder.Name, (string)((AliasedValue)records.Entities[0]["name"]).Value);
        }

        // This test currently passes - the aggregated value is returned ok
        [Fact]
        public void When_An_Aggregated_Query_Contains_Group_By_Attributes_The_Right_Aggregate_Values_Should_Be_Returned_In_The_Query_Results()
        {
            EntityCollection records = _service.RetrieveMultiple(new FetchExpression(fetchXML));

            Assert.Equal(1, (int)((AliasedValue)records.Entities[0]["count"]).Value);

        }
    }
}