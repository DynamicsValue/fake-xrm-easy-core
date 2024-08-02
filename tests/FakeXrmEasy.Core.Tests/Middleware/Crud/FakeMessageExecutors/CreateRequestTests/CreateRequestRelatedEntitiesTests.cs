using System;
using System.Collections.Generic;
using System.Linq;
using Crm;
using FakeXrmEasy.Abstractions;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Xunit;

namespace FakeXrmEasy.Core.Tests.Middleware.Crud.FakeMessageExecutors.CreateRequestTests
{
    public class CreateRequestRelatedEntitiesTests : FakeXrmEasyTestsBase
    {
        private readonly SalesOrder _salesOrder;
        private readonly SalesOrderDetail _salesOrderDetail1;
        private readonly SalesOrderDetail _salesOrderDetail2;
        
        public CreateRequestRelatedEntitiesTests()
        {
            
        }
        
        [Fact]
        public void When_related_entities_are_used_without_relationship_info_exception_is_raised()
        {
            var order = new SalesOrder();

            var orderItems = new EntityCollection(new List<Entity>()
            {
                new SalesOrderDetail(),
                new SalesOrderDetail()
            });

            // Add related order items so it can be created in one request
            order.RelatedEntities.Add(new Relationship
            {
                PrimaryEntityRole = EntityRole.Referenced,
                SchemaName = "order_details"
            }, orderItems);

            var request = new CreateRequest
            {
                Target = order
            };

            var exception = Record.Exception(() => _service.Execute(request));

            Assert.IsType<Exception>(exception);
            Assert.Equal("Relationship order_details does not exist in the metadata cache", exception.Message);
        }

        [Fact]
        public void When_related_entities_and_relationship_are_used_child_entities_are_created()
        {
            _context.AddRelationship("order_details",
                new XrmFakedRelationship()
                {
                    Entity1LogicalName = SalesOrder.EntityLogicalName,  //Referenced
                    Entity1Attribute = "salesorderid",              //Pk
                    Entity2LogicalName = SalesOrderDetail.EntityLogicalName,
                    Entity2Attribute = "salesorderid",              //Lookup attribute
                    RelationshipType = XrmFakedRelationship.FakeRelationshipType.OneToMany
                });

            var order = new SalesOrder();

            var orderItems = new EntityCollection(new List<Entity>()
            {
                new SalesOrderDetail(),
                new SalesOrderDetail()
            });

            // Add related order items so it can be created in one request
            order.RelatedEntities.Add(new Relationship
            {
                PrimaryEntityRole = EntityRole.Referenced,
                SchemaName = "order_details"
            }, orderItems);

            var request = new CreateRequest
            {
                Target = order
            };

            var id = (_service.Execute(request) as CreateResponse).id;
            var createdOrderDetails = _context.CreateQuery<SalesOrderDetail>().ToList();

            Assert.Equal(2, createdOrderDetails.Count);
            Assert.Equal(createdOrderDetails[0].SalesOrderId.Id, id);
            Assert.Equal(createdOrderDetails[1].SalesOrderId.Id, id);
        }
    }
}