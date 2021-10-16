using Crm;
using FakeXrmEasy.FakeMessageExecutors;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using FakeXrmEasy.Abstractions;

namespace FakeXrmEasy.Tests.FakeContextTests.AssociateRequestTests
{
    public class AssociateRequestTests : FakeXrmEasyTestsBase
    {

        public AssociateRequestTests()
        {
        }

        [Fact]
        public void When_can_execute_is_called_with_an_invalid_request_result_is_false()
        {
            var executor = new AssociateRequestExecutor();
            var anotherRequest = new RetrieveMultipleRequest();
            Assert.False(executor.CanExecute(anotherRequest));
        }

        [Fact]
        public void When_execute_is_called_with_a_null_request_exception_is_thrown()
        {
            
            var executor = new AssociateRequestExecutor();
            AssociateRequest req = null;
            Assert.Throws<Exception>(() => executor.Execute(req, _context));
        }

        [Fact]
        public void When_execute_is_called_with_a_null_target_exception_is_thrown()
        {
            
            var executor = new AssociateRequestExecutor();
            var req = new AssociateRequest() { Target = null, Relationship = new Relationship("fakeRelationship") };
            _context.AddRelationship("fakeRelationship", new XrmFakedRelationship());
            Assert.Throws<Exception>(() => executor.Execute(req, _context));
        }

        [Fact]
        public void When_execute_is_called_with_reversed_target_and_Related()
        {
            

            var userId = Guid.NewGuid();
            var teamId = Guid.NewGuid();
            var user2Id = Guid.NewGuid();
            _context.Initialize(new List<Entity> {
                new SystemUser
                {
                    Id = userId
                },
                new SystemUser
                {
                    Id = user2Id
                },
                new Team
                {
                    Id = teamId
                }
            });

            _context.AddRelationship("teammembership", new XrmFakedRelationship()
            {
                RelationshipType = XrmFakedRelationship.FakeRelationshipType.ManyToMany,
                IntersectEntity = "teammembership",
                Entity1Attribute = "systemuserid",
                Entity1LogicalName = "systemuser",
                Entity2Attribute = "teamid",
                Entity2LogicalName = "team"
            });

            var orgSvc = _context.GetOrganizationService();
            orgSvc.Associate("team", teamId, new Relationship("teammembership"),
                new EntityReferenceCollection(new List<EntityReference> { new EntityReference("systemuser", userId) }));

            orgSvc.Associate("systemuser", user2Id, new Relationship("teammembership"),
                new EntityReferenceCollection(new List<EntityReference> { new EntityReference("team", teamId) }));

            using (Crm.XrmServiceContext ctx = new XrmServiceContext(orgSvc))
            {
                var firstAssociation = (from tu in ctx.TeamMembershipSet
                                        where tu.TeamId == teamId
                                        && tu.SystemUserId == userId
                                        select tu).FirstOrDefault();
                Assert.NotNull(firstAssociation);

                var secondAssociation = (from tu in ctx.TeamMembershipSet
                                         where tu.TeamId == teamId
                                         && tu.SystemUserId == user2Id
                                         select tu).FirstOrDefault();
                Assert.NotNull(secondAssociation);
            }
        }

        [Fact]
        public void When_execute_is_called_with_a_non_existing_target_exception_is_thrown()
        {
            
            var executor = new AssociateRequestExecutor();

            _context.AddRelationship("fakeRelationship",
                new XrmFakedRelationship()
                {
                    IntersectEntity = "account_contact_intersect",
                    Entity1LogicalName = Contact.EntityLogicalName,
                    Entity1Attribute = "contactid",
                    Entity2LogicalName = Account.EntityLogicalName,
                    Entity2Attribute = "accountid"
                });

            var contact = new Entity("contact") { Id = Guid.NewGuid() };
            var account = new Entity("account") { Id = Guid.NewGuid() };
            _context.Initialize(new List<Entity>()
            {
                account
            });
            var req = new AssociateRequest()
            {
                Target = contact.ToEntityReference(),
                RelatedEntities = new EntityReferenceCollection()
                {
                    new EntityReference(Account.EntityLogicalName, account.Id),
                },
                Relationship = new Relationship("fakeRelationship")
            };
            Assert.Throws<Exception>(() => executor.Execute(req, _context));
        }

        [Fact]
        public void When_execute_is_called_with_a_non_existing_reference_exception_is_thrown()
        {
            
            var executor = new AssociateRequestExecutor();

            _context.AddRelationship("fakeRelationship",
                new XrmFakedRelationship()
                {
                    IntersectEntity = "account_contact_intersect",
                    Entity1LogicalName = Contact.EntityLogicalName,
                    Entity1Attribute = "contactid",
                    Entity2LogicalName = Account.EntityLogicalName,
                    Entity2Attribute = "accountid"
                });

            var contact = new Entity("contact") { Id = Guid.NewGuid() };
            var account = new Entity("account") { Id = Guid.NewGuid() };
            _context.Initialize(new List<Entity>()
            {
                contact
            });
            var req = new AssociateRequest()
            {
                Target = contact.ToEntityReference(),
                RelatedEntities = new EntityReferenceCollection()
                {
                    new EntityReference(Account.EntityLogicalName, account.Id),
                },
                Relationship = new Relationship("fakeRelationship")
            };
            Assert.Throws<Exception>(() => executor.Execute(req, _context));
        }
    }
}