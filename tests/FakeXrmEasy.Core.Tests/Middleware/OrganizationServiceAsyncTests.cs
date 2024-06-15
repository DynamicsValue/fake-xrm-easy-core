using System;
using System.Collections.Generic;
using System.Reflection;
using Crm;
using FakeItEasy;
using FakeXrmEasy.Abstractions;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using Xunit;

namespace FakeXrmEasy.Core.Tests.Middleware
{
    public class OrganizationServiceAsyncTests : FakeXrmEasyTestsBase
    {
        protected readonly Account _account;
        protected readonly Contact _contact;
        protected IOrganizationServiceAsync _serviceAsync;
        protected readonly SystemUser _user;
        protected readonly SystemUser _user2;
        protected readonly Team _team;

        public OrganizationServiceAsyncTests() : base()
        {
            InitServiceAsync();

            _contact = new Contact() { Id = Guid.NewGuid() };
            _account = new Account() { Id = Guid.NewGuid() };

            _context.EnableProxyTypes(Assembly.GetAssembly(typeof(Contact)));

            _context.AddRelationship("teammembership", new XrmFakedRelationship()
            {
                RelationshipType = XrmFakedRelationship.FakeRelationshipType.ManyToMany,
                IntersectEntity = "teammembership",
                Entity1Attribute = "systemuserid",
                Entity1LogicalName = "systemuser",
                Entity2Attribute = "teamid",
                Entity2LogicalName = "team"
            });

            _user = new SystemUser() { Id = Guid.NewGuid() };
            _user2 = new SystemUser() { Id = Guid.NewGuid() };
            _team = new Team() { Id = Guid.NewGuid() };

        }

        protected virtual void InitServiceAsync()
        {
            _serviceAsync = _context.GetAsyncOrganizationService();
        }

        [Fact]
        public async void Should_call_create_when_calling_async_create() 
        {
            var asyncResult = await _serviceAsync.CreateAsync(_contact);

            A.CallTo(() => _service.Create(_contact)).MustHaveHappened(); 
        }

        [Fact]
        public void Should_call_create_when_calling_sync_create() 
        {
            var asyncResult = _serviceAsync.Create(_contact);

            A.CallTo(() => _service.Create(_contact)).MustHaveHappened(); 
        }

        [Fact]
        public async void Should_call_update_when_calling_async_update() 
        {
            _context.Initialize(_contact);

            var entity = new Contact() { Id = _contact.Id, FirstName = "New Name" };
            await _serviceAsync.UpdateAsync(entity);

            A.CallTo(() => _service.Update(entity)).MustHaveHappened(); 
        }

        [Fact]
        public void Should_call_update_when_calling_sync_update() 
        {
            _context.Initialize(_contact);

            var entity = new Contact() { Id = _contact.Id, FirstName = "New Name" };
            _serviceAsync.Update(entity);

            A.CallTo(() => _service.Update(entity)).MustHaveHappened(); 
        }
        
        [Fact]
        public async void Should_call_delete_when_calling_async_delete() 
        {
            _context.Initialize(_contact);

            await _serviceAsync.DeleteAsync(Contact.EntityLogicalName, _contact.Id);

            A.CallTo(() => _service.Delete(Contact.EntityLogicalName, _contact.Id)).MustHaveHappened(); 
        }

        [Fact]
        public void Should_call_delete_when_calling_sync_delete() 
        {
            _context.Initialize(_contact);

            _serviceAsync.Delete(Contact.EntityLogicalName, _contact.Id);

            A.CallTo(() => _service.Delete(Contact.EntityLogicalName, _contact.Id)).MustHaveHappened(); 
        }

        [Fact]
        public async void Should_call_retrieve_when_calling_async_retrieve() 
        {
            _context.Initialize(_contact);

            var allColumns = new ColumnSet(true);
            var entity = await _serviceAsync.RetrieveAsync(Contact.EntityLogicalName, _contact.Id, allColumns);

            A.CallTo(() => _service.Retrieve(Contact.EntityLogicalName, _contact.Id, allColumns)).MustHaveHappened(); 
        }

        [Fact]
        public void Should_call_retrieve_when_calling_sync_retrieve() 
        {
            _context.Initialize(_contact);

            var allColumns = new ColumnSet(true);
            var entity = _serviceAsync.Retrieve(Contact.EntityLogicalName, _contact.Id, allColumns);

            A.CallTo(() => _service.Retrieve(Contact.EntityLogicalName, _contact.Id, allColumns)).MustHaveHappened(); 
        }

        [Fact]
        public async void Should_call_retrieve_multiple_when_calling_async_retrieve_multiple() 
        {
            _context.Initialize(_contact);

            var queryByAttribute = new QueryByAttribute(Contact.EntityLogicalName);
            queryByAttribute.ColumnSet = new ColumnSet(true);
            queryByAttribute.Attributes.AddRange("firstname");
            queryByAttribute.Values.AddRange("Lionel");

            var entityCollection = await _serviceAsync.RetrieveMultipleAsync(queryByAttribute);

            A.CallTo(() => _service.RetrieveMultiple(queryByAttribute)).MustHaveHappened(); 
        }

        [Fact]
        public void Should_call_retrieve_multiple_when_calling_sync_retrieve_multiple() 
        {
            _context.Initialize(_contact);

            var queryByAttribute = new QueryByAttribute(Contact.EntityLogicalName);
            queryByAttribute.ColumnSet = new ColumnSet(true);
            queryByAttribute.Attributes.AddRange("firstname");
            queryByAttribute.Values.AddRange("Lionel");

            var entity = _serviceAsync.RetrieveMultiple(queryByAttribute);

            A.CallTo(() => _service.RetrieveMultiple(queryByAttribute)).MustHaveHappened(); 
        }

        [Fact]
        public async void Should_call_execute_when_calling_async_execute() 
        {
            var request = new CreateRequest() { Target = _contact };

            var asyncResult = await _serviceAsync.ExecuteAsync(request);

            A.CallTo(() => _service.Execute(request)).MustHaveHappened(); 
        }

        [Fact]
        public void Should_call_execute_when_calling_sync_execute() 
        {
            var request = new CreateRequest() { Target = _contact };

            var asyncResult = _serviceAsync.Execute(request);

            A.CallTo(() => _service.Execute(request)).MustHaveHappened(); 
        }

        [Fact]
        public async void Should_call_associate_when_calling_async_associate() 
        {
            _context.Initialize(new List<Entity>() { _team, _user });

            var relationShip = new Relationship("teammembership");
            var relatedEntities = new EntityReferenceCollection()
                                {
                                    new EntityReference(SystemUser.EntityLogicalName, _user.Id),
                                };

            await _serviceAsync.AssociateAsync(Team.EntityLogicalName, _team.Id, relationShip, relatedEntities);

            A.CallTo(() => _service.Associate(Team.EntityLogicalName, _team.Id, relationShip, relatedEntities)).MustHaveHappened(); 
        }

        [Fact]
        public void Should_associate_execute_when_calling_sync_associate() 
        {
            _context.Initialize(new List<Entity>() { _team, _user });

            var relationShip = new Relationship("teammembership");
            var relatedEntities = new EntityReferenceCollection()
                                {
                                    new EntityReference(SystemUser.EntityLogicalName, _user.Id),
                                };

            _serviceAsync.Associate(Team.EntityLogicalName, _team.Id, relationShip, relatedEntities);

            A.CallTo(() => _service.Associate(Team.EntityLogicalName, _team.Id, relationShip, relatedEntities)).MustHaveHappened();
        }

        [Fact]
        public async void Should_call_disassociate_when_calling_async_disassociate() 
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

            await _serviceAsync.DisassociateAsync(Team.EntityLogicalName, _team.Id, relationShip, relatedEntities);

            A.CallTo(() => _service.Disassociate(Team.EntityLogicalName, _team.Id, relationShip, relatedEntities)).MustHaveHappened(); 
        }

        [Fact]
        public void Should_disassociate_execute_when_calling_sync_disassociate() 
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

            _serviceAsync.Disassociate(Team.EntityLogicalName, _team.Id, relationShip, relatedEntities);

            A.CallTo(() => _service.Disassociate(Team.EntityLogicalName, _team.Id, relationShip, relatedEntities)).MustHaveHappened(); 
        }
    }


    
    
}
