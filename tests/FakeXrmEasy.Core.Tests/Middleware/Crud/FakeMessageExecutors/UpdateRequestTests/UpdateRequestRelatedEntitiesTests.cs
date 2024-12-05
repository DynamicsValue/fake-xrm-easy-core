using System;
using System.Collections.Generic;
using System.Linq;
using Crm;
using FakeXrmEasy.Abstractions;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Xunit;

namespace FakeXrmEasy.Core.Tests.Middleware.Crud.FakeMessageExecutors.UpdateRequestTests
{
    public class UpdateRequestRelatedEntitiesTests: FakeXrmEasyTestsBase
    {
        private readonly SalesOrder _salesOrder;
        private readonly SalesOrderDetail _salesOrderDetail1;
        private readonly SalesOrderDetail _salesOrderDetail2;

        public UpdateRequestRelatedEntitiesTests()
        {
            _salesOrder = new SalesOrder()
            {
                Id = Guid.NewGuid(),
                OrderNumber = "Order 1"
            };

            _salesOrderDetail1 = new SalesOrderDetail()
            {
                Id = Guid.NewGuid(),
                Quantity = 1,
                SalesOrderId = _salesOrder.ToEntityReference()
            };
            
            _salesOrderDetail2 = new SalesOrderDetail()
            {
                Id = Guid.NewGuid(),
                Quantity = 2,
                SalesOrderId = _salesOrder.ToEntityReference()
            };
            
        }
        
        [Fact]
        public void Should_raise_exception_when_related_entities_are_used_without_a_relationship()
        {
            _context.Initialize(new List<Entity>()
            {
                _salesOrder, _salesOrderDetail1, _salesOrderDetail2
            });

            var order = new SalesOrder()
            {
                Id = _salesOrder.Id,
                OrderNumber = "Order 1 Updated"
            };
            
            var orderItems = new EntityCollection(new List<Entity>()
            {
                new SalesOrderDetail() { Id = _salesOrderDetail1.Id },
                new SalesOrderDetail() { Id = _salesOrderDetail2.Id }
            });

            // Add related order items so it can be updated in one request
            order.RelatedEntities.Add(new Relationship
            {
                PrimaryEntityRole = EntityRole.Referenced,
                SchemaName = "order_details"
            }, orderItems);

            var request = new UpdateRequest()
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
            _context.Initialize(new List<Entity>()
            {
                _salesOrder, _salesOrderDetail1, _salesOrderDetail2
            });
            
            _context.AddRelationship("order_details",
                new XrmFakedRelationship()
                {
                    Entity1LogicalName = SalesOrder.EntityLogicalName,  //Referenced
                    Entity1Attribute = "salesorderid",              //Pk
                    Entity2LogicalName = SalesOrderDetail.EntityLogicalName,
                    Entity2Attribute = "salesorderid",              //Lookup attribute
                    RelationshipType = XrmFakedRelationship.FakeRelationshipType.OneToMany
                });

            var order = new SalesOrder()
            {
                Id = _salesOrder.Id,
                OrderNumber = "Order 1 Updated"
            };
            
            var orderItems = new EntityCollection(new List<Entity>()
            {
                new SalesOrderDetail()
                {
                    Id = _salesOrderDetail1.Id,
                    Quantity = 11
                },
                new SalesOrderDetail()
                {
                    Id = _salesOrderDetail2.Id,
                    Quantity = 21
                }
            });

            // Add related order items so it can be created in one request
            order.RelatedEntities.Add(new Relationship
            {
                PrimaryEntityRole = EntityRole.Referenced,
                SchemaName = "order_details"
            }, orderItems);

            var request = new UpdateRequest()
            {
                Target = order
            };

            _service.Execute(request);
                
            var updatedOrder = _context.CreateQuery<SalesOrder>().FirstOrDefault();
            var updatedOrderDetails = _context.CreateQuery<SalesOrderDetail>().ToList();

            Assert.Equal(_salesOrder.Id, updatedOrder.Id);
            Assert.Equal("Order 1 Updated", updatedOrder.OrderNumber);
            
            Assert.Equal(2, updatedOrderDetails.Count);
            Assert.Equal(_salesOrder.Id, updatedOrderDetails[0].SalesOrderId.Id);
            Assert.Equal(_salesOrder.Id, updatedOrderDetails[1].SalesOrderId.Id);
            
            Assert.Equal(11, updatedOrderDetails[0].Quantity);
            Assert.Equal(21, updatedOrderDetails[1].Quantity);
        }
    }
}