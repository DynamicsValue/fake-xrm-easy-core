using System;
using System.Collections.Generic;
using System.Threading;
using Crm;
using FakeItEasy;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using Xunit;

namespace FakeXrmEasy.Core.Tests.Middleware
{
    public class OrganizationServiceAsync2Tests : OrganizationServiceAsyncTests
    {
        protected IOrganizationServiceAsync2 _serviceAsync2;

        public OrganizationServiceAsync2Tests() : base() 
        {
            _serviceAsync2 = _context.GetAsyncOrganizationService2();
        }

        protected override void InitServiceAsync()
        {
            _serviceAsync = _context.GetAsyncOrganizationService2();
        }

        [Fact]
        public async void Should_call_create_when_calling_async_create_with_cancellation() 
        {
            var asyncResult = await _serviceAsync2.CreateAsync(_contact, new CancellationToken());

            A.CallTo(() => _service.Create(_contact)).MustHaveHappened(); 
        }

        [Fact]
        public async void Should_call_update_when_calling_async_update_with_cancellation() 
        {
            _context.Initialize(_contact);

            var entity = new Contact() { Id = _contact.Id, FirstName = "New Name" };
            await _serviceAsync2.UpdateAsync(entity, new CancellationToken());

            A.CallTo(() => _service.Update(entity)).MustHaveHappened(); 
        }

        [Fact]
        public async void Should_call_delete_when_calling_async_delete_with_cancellation() 
        {
            _context.Initialize(_contact);

            await _serviceAsync2.DeleteAsync(Contact.EntityLogicalName, _contact.Id, new CancellationToken());

            A.CallTo(() => _service.Delete(Contact.EntityLogicalName, _contact.Id)).MustHaveHappened(); 
        }

        [Fact]
        public async void Should_call_retrieve_when_calling_async_retrieve_with_cancellation() 
        {
            _context.Initialize(_contact);

            var allColumns = new ColumnSet(true);
            var entity = await _serviceAsync2.RetrieveAsync(Contact.EntityLogicalName, _contact.Id, allColumns, new CancellationToken());

            A.CallTo(() => _service.Retrieve(Contact.EntityLogicalName, _contact.Id, allColumns)).MustHaveHappened(); 
        }

        [Fact]
        public async void Should_call_retrieve_multiple_when_calling_async_retrieve_multiple_with_cancellation() 
        {
            _context.Initialize(_contact);

            var queryByAttribute = new QueryByAttribute(Contact.EntityLogicalName);
            queryByAttribute.ColumnSet = new ColumnSet(true);
            queryByAttribute.Attributes.AddRange("firstname");
            queryByAttribute.Values.AddRange("Lionel");

            var entityCollection = await _serviceAsync2.RetrieveMultipleAsync(queryByAttribute, new CancellationToken());

            A.CallTo(() => _service.RetrieveMultiple(queryByAttribute)).MustHaveHappened(); 
        }

        [Fact]
        public async void Should_call_execute_when_calling_async_execute_with_cancellation() 
        {
            var request = new CreateRequest() { Target = _contact };

            var asyncResult = await _serviceAsync2.ExecuteAsync(request, new CancellationToken());

            A.CallTo(() => _service.Execute(request)).MustHaveHappened(); 
        }

        [Fact]
        public async void Should_call_associate_when_calling_async_associate_with_cancellation() 
        {
            _context.Initialize(new List<Entity>() { _team, _user });

            var relationShip = new Relationship("teammembership");
            var relatedEntities = new EntityReferenceCollection()
                                {
                                    new EntityReference(SystemUser.EntityLogicalName, _user.Id),
                                };

            await _serviceAsync2.AssociateAsync(Team.EntityLogicalName, _team.Id, relationShip, relatedEntities, new CancellationToken());

            A.CallTo(() => _service.Associate(Team.EntityLogicalName, _team.Id, relationShip, relatedEntities)).MustHaveHappened(); 
        }

        [Fact]
        public async void Should_call_disassociate_when_calling_async_disassociate_with_cancellation() 
        {
            var intersect = new TeamMembership()
            {
                Id = Guid.NewGuid(),
                ["teamid"] = _team.Id,
                ["systemuserid"] = _user.Id                
            };
            _context.Initialize(new List<Entity>() { _team, _user, intersect });

            var relationShip = new Relationship("teammembership");
            var relatedEntities = new EntityReferenceCollection()
                                {
                                    new EntityReference(SystemUser.EntityLogicalName, _user.Id),
                                };

            await _serviceAsync2.DisassociateAsync(Team.EntityLogicalName, _team.Id, relationShip, relatedEntities, new CancellationToken());

            A.CallTo(() => _service.Disassociate(Team.EntityLogicalName, _team.Id, relationShip, relatedEntities)).MustHaveHappened(); 
        }
    }
}