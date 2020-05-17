using Crm;
using FakeXrmEasy.Abstractions;
using FakeXrmEasy.FakeMessageExecutors;
using FakeXrmEasy.Middleware;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using System;
using System.Linq;
using Xunit;

namespace FakeXrmEasy.Tests.FakeContextTests.AddToQueueRequestTests
{
    public class AddToQueueRequestTests
    {
        private readonly IXrmFakedContext _context;
        private readonly IOrganizationService _service;

        public AddToQueueRequestTests()
        {
            _context = XrmFakedContextFactory.New();
            _service = _context.GetOrganizationService();
        }

        [Fact]
        public void When_can_execute_is_called_with_an_invalid_request_result_is_false()
        {
            var executor = new AddToQueueRequestExecutor();
            var anotherRequest = new RetrieveMultipleRequest();
            Assert.False(executor.CanExecute(anotherRequest));
        }

        [Fact]
        public void When_a_request_is_called_New_Queueitem_Is_Created()
        {
            
            

            var email = new Entity
            {
                LogicalName = Crm.Email.EntityLogicalName,
                Id = Guid.NewGuid(),
            };

            var queue = new Entity
            {
                LogicalName = Crm.Queue.EntityLogicalName,
                Id = Guid.NewGuid(),
            };

            _context.Initialize(new[]
            {
                queue, email
            });

            var executor = new AddToQueueRequestExecutor();

            var req = new AddToQueueRequest
            {
                DestinationQueueId = queue.Id,
                Target = email.ToEntityReference(),
            };

            executor.Execute(req, _context);

            var queueItem = _context.CreateQuery(Crm.QueueItem.EntityLogicalName).Single();

            Assert.Equal(queue.ToEntityReference(), queueItem.GetAttributeValue<EntityReference>("queueid"));
            Assert.Equal(email.ToEntityReference(), queueItem.GetAttributeValue<EntityReference>("objectid"));
        }

        [Fact]
        public void When_Queue_Item_Properties_Are_Passed_They_Are_Set_On_Create()
        {
            
            
            var workedBy = new EntityReference(SystemUser.EntityLogicalName, Guid.NewGuid());

            var email = new Entity
            {
                LogicalName = Crm.Email.EntityLogicalName,
                Id = Guid.NewGuid(),
            };

            var queue = new Entity
            {
                LogicalName = Crm.Queue.EntityLogicalName,
                Id = Guid.NewGuid(),
            };

            _context.Initialize(new[]
            {
                queue, email
            });

            var executor = new AddToQueueRequestExecutor();

            var req = new AddToQueueRequest
            {
                DestinationQueueId = queue.Id,
                Target = email.ToEntityReference(),
                QueueItemProperties = new QueueItem
                {
                    WorkerId = workedBy
                }
            };

            executor.Execute(req, _context);

            var queueItem = _context.CreateQuery(Crm.QueueItem.EntityLogicalName).Single();

            Assert.Equal(queue.ToEntityReference(), queueItem.GetAttributeValue<EntityReference>("queueid"));
            Assert.Equal(email.ToEntityReference(), queueItem.GetAttributeValue<EntityReference>("objectid"));
            Assert.Equal(workedBy, queueItem.GetAttributeValue<EntityReference>("workerid"));
        }

        [Fact]
        public void When_A_Queue_Item_Already_Exists_Use_Existing()
        {
            
            
            var workedBy = new EntityReference(SystemUser.EntityLogicalName, Guid.NewGuid());

            var email = new Entity
            {
                LogicalName = Crm.Email.EntityLogicalName,
                Id = Guid.NewGuid(),
            };

            var queue = new Entity
            {
                LogicalName = Crm.Queue.EntityLogicalName,
                Id = Guid.NewGuid(),
            };

            var queueItem = new QueueItem
            {
                LogicalName = Crm.Queue.EntityLogicalName,
                Id = Guid.NewGuid(),
                ObjectId = email.ToEntityReference()
            };

            _context.Initialize(new[]
            {
                queue, email
            });

            var executor = new AddToQueueRequestExecutor();

            var req = new AddToQueueRequest
            {
                DestinationQueueId = queue.Id,
                Target = email.ToEntityReference(),
                QueueItemProperties = new QueueItem
                {
                    WorkerId = workedBy
                }
            };

            executor.Execute(req, _context);

            Assert.Equal(1, _context.CreateQuery(Crm.QueueItem.EntityLogicalName).Count());

            queueItem = _context.CreateQuery(Crm.QueueItem.EntityLogicalName).Single().ToEntity<QueueItem>();

            Assert.Equal(queue.ToEntityReference(), queueItem.GetAttributeValue<EntityReference>("queueid"));
            Assert.Equal(email.ToEntityReference(), queueItem.GetAttributeValue<EntityReference>("objectid"));
            Assert.Equal(workedBy, queueItem.GetAttributeValue<EntityReference>("workerid"));
        }
    }
}