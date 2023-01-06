using Crm;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using Xunit;

namespace FakeXrmEasy.Tests.Issues
{
    // https://github.com/DynamicsValue/fake-xrm-easy/issues/64

    public class Issue64 : FakeXrmEasyTestsBase
    {
        SalesOrderDetail salesOrderDetail;
        // setup
        public Issue64()
        {
            _context.EnableProxyTypes(typeof(Account).Assembly);

            salesOrderDetail = new SalesOrderDetail()
            {
                Id = Guid.NewGuid(),
                PricePerUnit = new Money(1.0m)
            };

            _context.Initialize(new List<Entity> { salesOrderDetail });
        }

        // This test currently fails
        [Fact]
        public void When_Querying_A_Money_Attribute_Using_An_Integer_Value_It_Should_Not_Fail()
        {
            var qe = new QueryExpression(SalesOrderDetail.EntityLogicalName);
            qe.Criteria.AddCondition("priceperunit", ConditionOperator.Equal, 1);

            var entities = _service.RetrieveMultiple(qe).Entities;

            Assert.Equal(entities[0].Id, salesOrderDetail.Id);
        }
        // This test currently passes
        [Fact]
        public void When_Querying_A_Money_Attribute_Using_A_Money_Value_It_Should_Not_Fail()
        {
            var qe = new QueryExpression(SalesOrderDetail.EntityLogicalName);
            qe.Criteria.AddCondition("priceperunit", ConditionOperator.Equal, new Money(1.0m));

            var entities = _service.RetrieveMultiple(qe).Entities;

            Assert.Single(entities);
            Assert.Equal(entities[0].Id, salesOrderDetail.Id);
        }

    }
}