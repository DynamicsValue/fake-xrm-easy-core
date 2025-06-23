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

            SalesOrderDetail salesOrderDetail1 = new SalesOrderDetail() {Id = Guid.NewGuid(), SalesOrderId = _salesOrder.ToEntityReference(), LineItemNumber = 1 };

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


        [Fact]
        public void When_An_Aggregated_Query_Contains_Group_By_Attributes_These_Should_Be_Returned_In_The_Query_Results()
        {
            EntityCollection records = _service.RetrieveMultiple(new FetchExpression(fetchXML));
            
            // The 'name' column of the rist record should be the same as the orders name column
            Assert.Equal(_salesOrder.Name, (string)((AliasedValue)records.Entities[0]["name"]).Value);
        }

        [Fact]
        public void When_An_Aggregated_Query_Contains_Group_By_On_Base_Attributes_These_Should_Be_Returned_In_The_Query_Results()
        {
            EntityCollection records = _service.RetrieveMultiple(new FetchExpression(@"<fetch aggregate='true'>
                                  <entity name='salesorderdetail'>
                                    <attribute name='salesorderdetailid' alias='count' aggregate='count' />                                 
                                    <attribute name='lineitemnumber' alias='lineitemnumber' groupby='true' />                                     
                                  </entity>
                                </fetch>"));

            Assert.Equal(1, (int)((AliasedValue)records.Entities[0]["lineitemnumber"]).Value);
        }

        [Fact]
        public void When_An_Aggregated_Query_Contains_Group_By_Attributes_The_Right_Aggregate_Values_Should_Be_Returned_In_The_Query_Results()
        {
            EntityCollection records = _service.RetrieveMultiple(new FetchExpression(fetchXML));

            Assert.Equal(1, (int)((AliasedValue)records.Entities[0]["count"]).Value);
        }
    }
}