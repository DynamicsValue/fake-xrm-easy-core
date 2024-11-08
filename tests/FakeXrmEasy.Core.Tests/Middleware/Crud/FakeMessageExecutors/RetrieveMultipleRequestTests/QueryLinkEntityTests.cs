﻿using Crm;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xunit;
using FakeXrmEasy.Abstractions;

namespace FakeXrmEasy.Core.Tests.Middleware.Crud.FakeMessageExecutors.RetrieveMultipleRequestTests
{
    public class QueryLinkEntityTests : FakeXrmEasyTestsBase
    {       
        [Fact]
        public void Should_Find_Faked_N_To_N_Records()
        {
            var userId = new Guid("11111111-7982-4276-A8FE-7CE05FABEAB4");
            var businessId = Guid.NewGuid();

            var testUser = new SystemUser
            {
                Id = userId
            };

            var testRole = new Role
            {
                Id = new Guid("22222222-7982-4276-A8FE-7CE05FABEAB4"),
                Name = "Test Role",
                BusinessUnitId = new EntityReference(BusinessUnit.EntityLogicalName, businessId)
            };

            _context.Initialize(new Entity[] { testUser, testRole });

            _context.AddRelationship("systemuserroles_association", new XrmFakedRelationship
            {
                IntersectEntity = "systemuserroles",
                Entity1LogicalName = SystemUser.EntityLogicalName,
                Entity1Attribute = "systemuserid",
                Entity2LogicalName = Role.EntityLogicalName,
                Entity2Attribute = "roleid"
            });

            var request = new AssociateRequest()
            {
                Target = testUser.ToEntityReference(),
                RelatedEntities = new EntityReferenceCollection()
                {
                    new EntityReference(Role.EntityLogicalName, testRole.Id),
                },
                Relationship = new Relationship("systemuserroles_association")
            };

            _service.Execute(request);

            var query = new QueryExpression()
            {
                EntityName = "role",
                ColumnSet = new ColumnSet("name"),
                LinkEntities =
                {
                    new LinkEntity
                    {
                        LinkFromEntityName = Role.EntityLogicalName,
                        LinkFromAttributeName = "roleid",
                        LinkToEntityName = SystemUserRoles.EntityLogicalName,
                        LinkToAttributeName = "roleid",
                        LinkCriteria = new FilterExpression
                        {
                            FilterOperator = LogicalOperator.And,
                            Conditions =
                            {
                                new ConditionExpression
                                {
                                    AttributeName = "systemuserid",
                                    Operator = ConditionOperator.Equal,
                                    Values = {userId}
                                }
                            }
                        }
                    }
                }
            };

            var result = _service.RetrieveMultiple(query);
            Assert.NotEmpty(result.Entities);
            Assert.Single(result.Entities);
        }

        [Fact]
        public void Should_Only_Find_Correct_Faked_N_To_N_Records()
        {
            var userId = new Guid("11111111-7982-4276-A8FE-7CE05FABEAB4");
            var businessId = Guid.NewGuid();

            var testUser = new SystemUser
            {
                Id = userId
            };

            var testRole = new Role
            {
                Id = new Guid("22222222-7982-4276-A8FE-7CE05FABEAB4"),
                Name = "Test Role",
                BusinessUnitId = new EntityReference(BusinessUnit.EntityLogicalName, businessId)
            };

            var testUser2 = new SystemUser
            {
                Id = Guid.NewGuid()
            };

            var testRole2 = new Role
            {
                Id = Guid.NewGuid(),
                Name = "Test Role",
                BusinessUnitId = new EntityReference(BusinessUnit.EntityLogicalName, businessId)
            };

            _context.Initialize(new Entity[] { testUser, testRole, testUser2, testRole2 });

            _context.AddRelationship("systemuserroles_association", new XrmFakedRelationship
            {
                IntersectEntity = "systemuserroles",
                Entity1LogicalName = SystemUser.EntityLogicalName,
                Entity1Attribute = "systemuserid",
                Entity2LogicalName = Role.EntityLogicalName,
                Entity2Attribute = "roleid"
            });

            var request = new AssociateRequest()
            {
                Target = testUser.ToEntityReference(),
                RelatedEntities = new EntityReferenceCollection()
                {
                    new EntityReference(Role.EntityLogicalName, testRole.Id),
                },
                Relationship = new Relationship("systemuserroles_association")
            };

            _service.Execute(request);

            var request2 = new AssociateRequest()
            {
                Target = testUser2.ToEntityReference(),
                RelatedEntities = new EntityReferenceCollection()
                {
                    new EntityReference(Role.EntityLogicalName, testRole2.Id),
                },
                Relationship = new Relationship("systemuserroles_association")
            };

            _service.Execute(request2);

            var query = new QueryExpression()
            {
                EntityName = "role",
                ColumnSet = new ColumnSet("name"),
                LinkEntities =
                {
                    new LinkEntity
                    {
                        LinkFromEntityName = Role.EntityLogicalName,
                        LinkFromAttributeName = "roleid",
                        LinkToEntityName = SystemUserRoles.EntityLogicalName,
                        LinkToAttributeName = "roleid",
                        LinkCriteria = new FilterExpression
                        {
                            FilterOperator = LogicalOperator.And,
                            Conditions =
                            {
                                new ConditionExpression
                                {
                                    AttributeName = "systemuserid",
                                    Operator = ConditionOperator.Equal,
                                    Values = {userId}
                                }
                            }
                        }
                    }
                }
            };

            var result = _service.RetrieveMultiple(query);
            Assert.NotEmpty(result.Entities);
            Assert.Single(result.Entities);
        }

        [Fact]
        public void Should_Not_Find_Faked_N_To_N_Records_If_Disassociated_Again()
        {

            var userId = new Guid("11111111-7982-4276-A8FE-7CE05FABEAB4");
            var businessId = Guid.NewGuid();

            var testUser = new SystemUser
            {
                Id = userId
            };

            var testRole = new Role
            {
                Id = new Guid("22222222-7982-4276-A8FE-7CE05FABEAB4"),
                Name = "Test Role",
                BusinessUnitId = new EntityReference(BusinessUnit.EntityLogicalName, businessId)
            };

            _context.Initialize(new Entity[] { testUser, testRole });

            _context.AddRelationship("systemuserroles_association", new XrmFakedRelationship
            {
                IntersectEntity = "systemuserroles",
                Entity1LogicalName = SystemUser.EntityLogicalName,
                Entity1Attribute = "systemuserid",
                Entity2LogicalName = Role.EntityLogicalName,
                Entity2Attribute = "roleid"
            });

            var request = new AssociateRequest()
            {
                Target = testUser.ToEntityReference(),
                RelatedEntities = new EntityReferenceCollection()
                {
                    new EntityReference(Role.EntityLogicalName, testRole.Id),
                },
                Relationship = new Relationship("systemuserroles_association")
            };

            _service.Execute(request);

            var disassociate = new DisassociateRequest
            {
                Target = testUser.ToEntityReference(),
                RelatedEntities = new EntityReferenceCollection()
                {
                    new EntityReference(Role.EntityLogicalName, testRole.Id),
                },
                Relationship = new Relationship("systemuserroles_association")
            };

            _service.Execute(disassociate);

            var query = new QueryExpression()
            {
                EntityName = "role",
                ColumnSet = new ColumnSet("name"),
                LinkEntities =
                {
                    new LinkEntity
                    {
                        LinkFromEntityName = Role.EntityLogicalName,
                        LinkFromAttributeName = "roleid",
                        LinkToEntityName = SystemUserRoles.EntityLogicalName,
                        LinkToAttributeName = "roleid",
                        LinkCriteria = new FilterExpression
                        {
                            FilterOperator = LogicalOperator.And,
                            Conditions =
                            {
                                new ConditionExpression
                                {
                                    AttributeName = "systemuserid",
                                    Operator = ConditionOperator.Equal,
                                    Values = {userId}
                                }
                            }
                        }
                    }
                }
            };

            var result = _service.RetrieveMultiple(query);
            Assert.Empty(result.Entities);
        }

        [Fact]
        public void Should_Find_Faked_N_To_N_Records_Using_Associate_Method()
        {
            
            var userId = new Guid("11111111-7982-4276-A8FE-7CE05FABEAB4");
            var businessId = Guid.NewGuid();

            var testUser = new SystemUser
            {
                Id = userId
            };

            var testRole = new Role
            {
                Id = new Guid("22222222-7982-4276-A8FE-7CE05FABEAB4"),
                Name = "Test Role",
                BusinessUnitId = new EntityReference(BusinessUnit.EntityLogicalName, businessId)
            };

            _context.Initialize(new Entity[] { testUser, testRole });

            _context.AddRelationship("systemuserroles", new XrmFakedRelationship
            {
                IntersectEntity = "systemuserroles",
                Entity1LogicalName = SystemUser.EntityLogicalName,
                Entity1Attribute = "systemuserid",
                Entity2LogicalName = Role.EntityLogicalName,
                Entity2Attribute = "roleid"
            });

            _service.Associate("systemuserroles",
                testUser.Id,
                new Relationship("systemuserroles"),
                new EntityReferenceCollection()
                {
                    new EntityReference(Role.EntityLogicalName, testRole.Id),
                });

            var query = new QueryExpression()
            {
                EntityName = "role",
                ColumnSet = new ColumnSet("name"),
                LinkEntities =
                {
                    new LinkEntity
                    {
                        LinkFromEntityName = Role.EntityLogicalName,
                        LinkFromAttributeName = "roleid",
                        LinkToEntityName = SystemUserRoles.EntityLogicalName,
                        LinkToAttributeName = "roleid",
                        LinkCriteria = new FilterExpression
                        {
                            FilterOperator = LogicalOperator.And,
                            Conditions =
                            {
                                new ConditionExpression
                                {
                                    AttributeName = "systemuserid",
                                    Operator = ConditionOperator.Equal,
                                    Values = {userId}
                                }
                            }
                        }
                    }
                }
            };

            var result = _service.RetrieveMultiple(query);
            Assert.NotEmpty(result.Entities);
            Assert.Single(result.Entities);
        }

        [Fact]
        public void Should_Not_Find_Faked_N_To_N_Records_If_Disassociated_Again_Using_Disassociate_Method()
        {
            
            

            var userId = new Guid("11111111-7982-4276-A8FE-7CE05FABEAB4");
            var businessId = Guid.NewGuid();

            var testUser = new SystemUser
            {
                Id = userId
            };

            var testRole = new Role
            {
                Id = new Guid("22222222-7982-4276-A8FE-7CE05FABEAB4"),
                Name = "Test Role",
                BusinessUnitId = new EntityReference(BusinessUnit.EntityLogicalName, businessId)
            };

            _context.Initialize(new Entity[] { testUser, testRole });

            _context.AddRelationship("systemuserroles", new XrmFakedRelationship
            {
                IntersectEntity = "systemuserroles",
                Entity1LogicalName = SystemUser.EntityLogicalName,
                Entity1Attribute = "systemuserid",
                Entity2LogicalName = Role.EntityLogicalName,
                Entity2Attribute = "roleid"
            });

            _service.Associate("systemuserroles",
                testUser.Id,
                new Relationship("systemuserroles"),
                new EntityReferenceCollection()
                {
                    new EntityReference(Role.EntityLogicalName, testRole.Id),
                });

            _service.Disassociate("systemuserroles",
                testUser.Id,
                new Relationship("systemuserroles"),
                new EntityReferenceCollection()
                {
                    new EntityReference(Role.EntityLogicalName, testRole.Id),
                });

            var query = new QueryExpression()
            {
                EntityName = "role",
                ColumnSet = new ColumnSet("name"),
                LinkEntities =
                {
                    new LinkEntity
                    {
                        LinkFromEntityName = Role.EntityLogicalName,
                        LinkFromAttributeName = "roleid",
                        LinkToEntityName = SystemUserRoles.EntityLogicalName,
                        LinkToAttributeName = "roleid",
                        LinkCriteria = new FilterExpression
                        {
                            FilterOperator = LogicalOperator.And,
                            Conditions =
                            {
                                new ConditionExpression
                                {
                                    AttributeName = "systemuserid",
                                    Operator = ConditionOperator.Equal,
                                    Values = {userId}
                                }
                            }
                        }
                    }
                }
            };

            var result = _service.RetrieveMultiple(query);
            Assert.Empty(result.Entities);
        }

        [Fact]
        public void Should_Not_Fail_On_Conditions_In_Link_Entities()
        {
            var testEntity1 = new Entity("entity1")
            {
                Attributes = new AttributeCollection
                {
                    {"entity1attr", "test2"}
                }
            };
            var testEntity2 = new Entity("entity2")
            {
                Attributes = new AttributeCollection
                {
                    {"entity2attr", "test2"}
                }
            };

            testEntity1.Id = _service.Create(testEntity1);
            testEntity2.Id = _service.Create(testEntity2);

            var testRelation = new XrmFakedRelationship
            {
                IntersectEntity = "TestIntersectEntity",
                Entity1LogicalName = "entity1",
                Entity1Attribute = "entity1attr",
                Entity2LogicalName = "entity2",
                Entity2Attribute = "entity2attr"
            };
            _context.AddRelationship(testRelation.Entity2LogicalName, testRelation);
            _service.Associate(testEntity1.LogicalName, testEntity1.Id,
                new Relationship(testRelation.Entity2LogicalName),
                new EntityReferenceCollection { testEntity2.ToEntityReference() });

            var query = new QueryExpression
            {
                EntityName = "entity1",
                Criteria = new FilterExpression { FilterOperator = LogicalOperator.And },
                ColumnSet = new ColumnSet(true)
            };

            var link = new LinkEntity
            {
                JoinOperator = JoinOperator.Natural,
                LinkFromEntityName = "entity1",
                LinkFromAttributeName = "entity1attr",
                LinkToEntityName = "entity2",
                LinkToAttributeName = "entity2attr",
                LinkCriteria = new FilterExpression
                {
                    FilterOperator = LogicalOperator.And,
                    Conditions =
                    {
                        new ConditionExpression
                        {
                            AttributeName = "entity2attr",
                            Operator = ConditionOperator.Equal,
                            Values = {"test2"}
                        }
                    }
                }
            };
            query.LinkEntities.Add(link);

            var result = _service.RetrieveMultiple(query);
            Assert.NotEmpty(result.Entities);
            Assert.Single(result.Entities);
        }

        [Fact]
        public void Entities_Can_Be_Linked_On_String_Attribute()
        {
            
            
            var entity = new Entity("entity") { Id = Guid.NewGuid(), ["name"] = "test" };
            _context.Initialize(entity);
            var query = new QueryExpression("entity");
            query.ColumnSet = new ColumnSet(true);
            query.AddLink("entity", "name", "name");

            var queryResult = _service.RetrieveMultiple(query);

            Assert.Single(queryResult.Entities);
        }

        [Fact]
        public void Should_evaluate_all_LinkEntity_conditions()
        {
            
            

            var entity1 = new Entity("entity1")
            {
                Id = Guid.NewGuid()
            };

            var entity2 = new Entity("entity2")
            {
                Id = Guid.NewGuid(),
                ["associated1"] = entity1.ToEntityReference()
            };

            var entity3 = new Entity("entity3")
            {
                Id = Guid.NewGuid(),
                ["associated2"] = entity2.ToEntityReference(),
                ["testFilter"] = "testValue"
            };

            var query = new QueryExpression
            {
                EntityName = "entity1",
                ColumnSet = new ColumnSet(true),
                LinkEntities =
                {
                    new LinkEntity("entity1", "entity2", "entity1id", "associated1", JoinOperator.Inner)
                    {
                        LinkEntities =
                        {
                            new LinkEntity("entity2", "entity3", "entity2id", "associated2", JoinOperator.Inner)
                            {
                                LinkCriteria =
                                {
                                    Conditions =
                                    {
                                        new ConditionExpression("testFilter", ConditionOperator.Equal, "doesNotMatch")
                                    }
                                }
                            }
                        }
                    }
                }
            };

            _context.Initialize(new[] { entity1, entity2, entity3 });

            var result = _service.RetrieveMultiple(query);

            Assert.Empty(result.Entities);
        }

        [Fact]
        public void When_querying_by_an_attribute_which_wasnt_initialised_null_value_is_returned_for_early_bound_and_not_an_exception()
        {
            
            _context.EnableProxyTypes(Assembly.GetAssembly(typeof(Contact)));

            
            _context.Initialize(new List<Entity>()
            {
                new Contact()
                {
                    Id = Guid.NewGuid(),
                    LastName = "Mcdonald"
                }
            });

            var name = "Mcdonald";

            using (var _context = new XrmServiceContext(_service))
            {
                var contacts = (from c in _context.ContactSet
                                where c.FirstName == name || c.LastName == name
                                select new Contact { Id = c.Id, FirstName = c.FirstName, LastName = c.LastName }).ToList();

                Assert.Single(contacts);
                Assert.Null(contacts[0].FirstName);
            }
        }

        [Fact]
        public void When_sorting_by_an_attribute_which_wasnt_initialised_an_exception_is_not_thrown()
        {
            
            _context.EnableProxyTypes(Assembly.GetAssembly(typeof(Contact)));

            
            _context.Initialize(new List<Entity>()
            {
                new Contact() {Id = Guid.NewGuid(), FirstName = "Ronald", LastName = "Mcdonald"},
                new Contact() {Id = Guid.NewGuid(), LastName = "Jordan"}
            });

            using (var _context = new XrmServiceContext(_service))
            {
                var contacts = (from c in _context.ContactSet
                                orderby c.FirstName
                                select new Contact { Id = c.Id, FirstName = c.FirstName, LastName = c.LastName }).ToList();

                Assert.Equal(2, contacts.Count);
                Assert.Null(contacts[0].FirstName);
            }
        }

        [Fact]
        public void Should_Not_Throw_Unable_To_Cast_AliasedValue_Exception()
        {
            var account1 = new Account() { Id = Guid.NewGuid(), Name = "1 Test", Address1_City = "1 City", Address1_StateOrProvince = "a2 State" };
            var account2 = new Account() { Id = Guid.NewGuid(), Name = "2 Test", Address1_City = "2 City", Address1_StateOrProvince = "b2 State" };
            var account3 = new Account() { Id = Guid.NewGuid(), Name = "3 Test", Address1_City = "2 City", Address1_StateOrProvince = "b1 State" };

            var contact1 = new Contact() { Id = Guid.NewGuid(), FirstName = "1 Cont", LastName = "Cont 1", Address1_City = "1 City", ParentCustomerId = account1.ToEntityReference() };
            var contact2 = new Contact() { Id = Guid.NewGuid(), FirstName = "2 Cont", LastName = "Cont 2", Address1_City = "1 City", ParentCustomerId = account2.ToEntityReference() };
            var contact3 = new Contact() { Id = Guid.NewGuid(), FirstName = "3 Cont", LastName = "Cont 3", Address1_City = "1 City", ParentCustomerId = account3.ToEntityReference() };
            var contact4 = new Contact() { Id = Guid.NewGuid(), FirstName = "4 Cont", LastName = "Cont 4", Address1_City = "2 City", ParentCustomerId = account1.ToEntityReference() };
            var contact5 = new Contact() { Id = Guid.NewGuid(), FirstName = "5 Cont", LastName = "Cont 5", Address1_City = "2 City", ParentCustomerId = account2.ToEntityReference() };
            var contact6 = new Contact() { Id = Guid.NewGuid(), FirstName = "6 Cont", LastName = "Cont 6", Address1_City = "2 City", ParentCustomerId = account3.ToEntityReference() };

            
            _context.EnableProxyTypes(Assembly.GetAssembly(typeof(Contact)));
            
            _context.Initialize(new List<Entity>() {
                account1, account2, account3, contact1, contact2, contact3, contact4, contact5, contact6
            });

            QueryExpression query = new QueryExpression()
            {
                EntityName = "contact",
                ColumnSet = new ColumnSet(true),
                LinkEntities =
                {
                    new LinkEntity(Contact.EntityLogicalName, Account.EntityLogicalName, "parentcustomerid", "accountid", JoinOperator.LeftOuter)
                    {
                        LinkCriteria = new FilterExpression()
                        {
                            FilterOperator = LogicalOperator.And,
                            Conditions =
                            {
                                new ConditionExpression("address1_city", ConditionOperator.Like, "2%")
                            }
                        }
                    },
                    new LinkEntity(Contact.EntityLogicalName, Contact.EntityLogicalName, "parentcustomerid", "contactid", JoinOperator.LeftOuter)
                    {
                        LinkCriteria = new FilterExpression()
                        {
                            FilterOperator = LogicalOperator.And,
                            Conditions =
                            {
                                new ConditionExpression("address1_city", ConditionOperator.Like, "2%")
                            }
                        }
                    },
                }
            };

            EntityCollection entities = _service.RetrieveMultiple(query);
            Assert.Equal(6, entities.Entities.Count);
        }

#if FAKE_XRM_EASY_2016 || FAKE_XRM_EASY_2015 || FAKE_XRM_EASY_2013 || FAKE_XRM_EASY_365 || FAKE_XRM_EASY_9
        [Fact]
        public void Should_Not_Apply_Left_Outer_Join_Filters_When_The_Right_hand_side_of_the_expression_wasnt_found()
        {
            
            

            // Date for filtering, we only want "expired" records, i.e. those that weren't set as regarding in any emails for this period and logically even exist this long
            var days = 5;

            var incident = new Incident
            {
                Id = Guid.NewGuid(),
                Title = "Test case",
                StatusCode = new OptionSetValue((int)IncidentState.Active)
            };
            incident["createdon"] = DateTime.UtcNow.AddDays(-6);

            _context.Initialize(new[] { incident });

            // Remove either incident createdon conditionexpression, or LinkEntities and the e-mail conditionexpression and it will pass
            // What this query expresses: Get all incidents, that are older than given number of days and that also didn't receive emails for this number of days
            var query = new QueryExpression
            {
                ColumnSet = new ColumnSet(true),
                EntityName = Incident.EntityLogicalName,
                Criteria =
                {
                    FilterOperator = LogicalOperator.And,
                    Filters =
                    {
                        new FilterExpression
                        {
                            FilterOperator = LogicalOperator.And,
                            Conditions =
                            {
                                new ConditionExpression("statuscode", ConditionOperator.Equal, new OptionSetValue((int) IncidentState.Active)),
                                new ConditionExpression("createdon", ConditionOperator.LessEqual, DateTime.UtcNow.AddDays(-1 * days))
                            }
                        }
                    }
                },
                LinkEntities =
                {
                    new LinkEntity
                    {
                        LinkFromEntityName = "incident",
                        LinkToEntityName = "email",
                        LinkFromAttributeName = "incidentid",
                        LinkToAttributeName = "regardingobjectid",
                        JoinOperator = JoinOperator.LeftOuter,
                        LinkCriteria = new FilterExpression
                        {
                            Filters =
                            {
                                new FilterExpression
                                {
                                    FilterOperator = LogicalOperator.And,
                                    Conditions =
                                    {
                                        new ConditionExpression("createdon", ConditionOperator.GreaterEqual, DateTime.UtcNow.AddDays(-1*days))
                                    }
                                }
                            }
                        }
                    }
                }
            };

            var incidents = _service.RetrieveMultiple(query).Entities;
            Assert.Single(incidents);
        }

        [Fact]
        public void Should_Apply_Left_Outer_Join_Filters_When_The_Right_hand_side_of_the_expression_was_found()
        {
            
            

            // Date for filtering, we only want "expired" records, i.e. those that weren't set as regarding in any emails for this period and logically even exist this long
            var days = 5;

            var incident = new Incident
            {
                Id = Guid.NewGuid(),
                Title = "Test case",
                StatusCode = new OptionSetValue((int)IncidentState.Active)
            };

            var email = new Email
            {
                Id = Guid.NewGuid(),
                RegardingObjectId = incident.ToEntityReference(),
            };

            incident["createdon"] = DateTime.UtcNow.AddDays(-6);
            email["createdon"] = DateTime.UtcNow.AddDays(10);

            _context.Initialize(new List<Entity>() { incident, email });

            // Remove either incident createdon conditionexpression, or LinkEntities and the e-mail conditionexpression and it will pass
            // What this query expresses: Get all incidents, that are older than given number of days and that also didn't receive emails for this number of days
            var query = new QueryExpression
            {
                ColumnSet = new ColumnSet(true),
                EntityName = Incident.EntityLogicalName,
                Criteria =
                {
                    FilterOperator = LogicalOperator.And,
                    Filters =
                    {
                        new FilterExpression
                        {
                            FilterOperator = LogicalOperator.And,
                            Conditions =
                            {
                                new ConditionExpression("statuscode", ConditionOperator.Equal, new OptionSetValue((int) IncidentState.Active)),
                                new ConditionExpression("createdon", ConditionOperator.LessEqual, DateTime.UtcNow.AddDays(-1 * days))
                            }
                        }
                    }
                },
                LinkEntities =
                {
                    new LinkEntity
                    {
                        LinkFromEntityName = "incident",
                        LinkToEntityName = "email",
                        LinkFromAttributeName = "incidentid",
                        LinkToAttributeName = "regardingobjectid",
                        JoinOperator = JoinOperator.LeftOuter,
                        LinkCriteria = new FilterExpression
                        {
                            Filters =
                            {
                                new FilterExpression
                                {
                                    FilterOperator = LogicalOperator.And,
                                    Conditions =
                                    {
                                        new ConditionExpression("createdon", ConditionOperator.GreaterEqual, DateTime.UtcNow.AddDays(-1*days))
                                    }
                                }
                            }
                        }
                    }
                }
            };

            var incidents = _service.RetrieveMultiple(query).Entities;
            Assert.Single(incidents);
        }
#endif

        [Fact]
        public void When_There_Is_A_LinkedEntity_The_Output_EntityAlias_Should_Be_Suffixed_With_1()
        {
            
            

            var entities = new List<Entity>();

            var user1 = new SystemUser
            {
                Id = Guid.NewGuid(),
                ["fullname"] = "User1"
            };
            entities.Add(user1);

            var user2 = new SystemUser
            {
                Id = Guid.NewGuid(),
                ["fullname"] = "User2",
                ["modifiedby"] = user1.ToEntityReference()
            };
            entities.Add(user2);

            _context.Initialize(entities);

            var query = new QueryExpression(SystemUser.EntityLogicalName)
            {
                LinkEntities =
                {
                    new LinkEntity(SystemUser.EntityLogicalName, SystemUser.EntityLogicalName, "modifiedby", "systemuserid", JoinOperator.Inner)
                    {
                        Columns = new ColumnSet("fullname"),
                    }
                }
            };

            var result = _service.RetrieveMultiple(query);
            var resultingEntity = result.Entities[0];
            Assert.Equal(2, resultingEntity.Attributes.Count);
            Assert.Equal("User1", ((AliasedValue)resultingEntity["systemuser1.fullname"]).Value);
        }

        [Fact]
        public void When_There_Are_Multiple_LinkedEntities_The_Output_EntityAlias_Should_All_Be_Suffixed_With_1()

        {
            
            

            var entities = new List<Entity>();

            var user1 = new SystemUser
            {
                Id = Guid.NewGuid(),
                ["fullname"] = "User1"
            };
            entities.Add(user1);

            var businessUnit = new BusinessUnit
            {
                Id = Guid.NewGuid(),
                Name = "BusinessUnit1"
            };
            entities.Add(businessUnit);

            var user2 = new SystemUser
            {
                Id = Guid.NewGuid(),
                BusinessUnitId = businessUnit.ToEntityReference(),
                ["fullname"] = "User2",
                ["modifiedby"] = user1.ToEntityReference()
            };
            entities.Add(user2);

            _context.Initialize(entities);

            var query = new QueryExpression(SystemUser.EntityLogicalName)
            {
                LinkEntities =
                {
                    new LinkEntity(SystemUser.EntityLogicalName, SystemUser.EntityLogicalName, "modifiedby", "systemuserid", JoinOperator.Inner)
                    {
                        Columns = new ColumnSet("fullname"),
                    },
                    new LinkEntity(SystemUser.EntityLogicalName, BusinessUnit.EntityLogicalName, "businessunitid", "businessunitid", JoinOperator.Inner)
                    {
                        Columns = new ColumnSet("name"),
                    }
                }
            };

            var result = _service.RetrieveMultiple(query);
            var resultingEntity = result.Entities[0];
            Assert.Equal(3, resultingEntity.Attributes.Count);
            Assert.Equal("User1", ((AliasedValue)resultingEntity["systemuser1.fullname"]).Value);
            Assert.Equal("BusinessUnit1", ((AliasedValue)resultingEntity["businessunit1.name"]).Value);
        }

        [Fact]
        public void When_There_Are_Multiple_LinkedEntities_With_The_Same_Entitiy_The_Output_EntityAlias_Should_All_Be_Suffixed_With_Incrementally()
        {
            
            

            var entities = new List<Entity>();

            var user1 = new SystemUser
            {
                Id = Guid.NewGuid(),
                ["fullname"] = "User1"
            };
            entities.Add(user1);

            var user2 = new SystemUser
            {
                Id = Guid.NewGuid(),
                ["fullname"] = "User2",
                ["modifiedby"] = user1.ToEntityReference()
            };
            entities.Add(user2);

            var user3 = new SystemUser
            {
                Id = Guid.NewGuid(),
                ["fullname"] = "User3",
                ["modifiedby"] = user2.ToEntityReference()
            };
            entities.Add(user3);

            _context.Initialize(entities);

            var query = new QueryExpression(SystemUser.EntityLogicalName)
            {
                LinkEntities =
                {
                    new LinkEntity(SystemUser.EntityLogicalName, SystemUser.EntityLogicalName, "modifiedby", "systemuserid", JoinOperator.Inner)
                    {
                        Columns = new ColumnSet("fullname"),
                        LinkEntities =
                        {
                            new LinkEntity(SystemUser.EntityLogicalName, SystemUser.EntityLogicalName, "modifiedby", "systemuserid", JoinOperator.Inner)
                            {
                                Columns = new ColumnSet("fullname"),
                            }
                        }
                    }
                }
            };

            var result = _service.RetrieveMultiple(query);
            var resultingEntity = result.Entities[0];
            Assert.Equal(3, resultingEntity.Attributes.Count);
            Assert.Equal("User2", ((AliasedValue)resultingEntity["systemuser1.fullname"]).Value);
            Assert.Equal("User1", ((AliasedValue)resultingEntity["systemuser2.fullname"]).Value);
        }

        [Fact]
        public void When_There_Are_Multiple_LinkedEntities_With_The_Same_Entitiy_And_One_Has_An_Alias_The_Output_EntityAlias_Should_All_Be_Suffixed_With_Incrementally_Ignoring_The_Aliased_One()
        {
            
            

            var entities = new List<Entity>();

            var user1 = new SystemUser
            {
                Id = Guid.NewGuid(),
                ["fullname"] = "User1"
            };
            entities.Add(user1);

            var user2 = new SystemUser
            {
                Id = Guid.NewGuid(),
                ["fullname"] = "User2",
                ["modifiedby"] = user1.ToEntityReference()
            };
            entities.Add(user2);

            var user3 = new SystemUser
            {
                Id = Guid.NewGuid(),
                ["fullname"] = "User3",
                ["modifiedby"] = user2.ToEntityReference()
            };
            entities.Add(user3);

            var user4 = new SystemUser
            {
                Id = Guid.NewGuid(),
                ["fullname"] = "User4",
                ["modifiedby"] = user3.ToEntityReference()
            };
            entities.Add(user4);

            _context.Initialize(entities);

            var query = new QueryExpression(SystemUser.EntityLogicalName)
            {
                LinkEntities =
                {
                    new LinkEntity(SystemUser.EntityLogicalName, SystemUser.EntityLogicalName, "modifiedby", "systemuserid", JoinOperator.Inner)
                    {
                        Columns = new ColumnSet("fullname"),
                        LinkEntities =
                        {
                            new LinkEntity(SystemUser.EntityLogicalName, SystemUser.EntityLogicalName, "modifiedby", "systemuserid", JoinOperator.Inner)
                            {
                                EntityAlias = "systemuserwithalias",
                                Columns = new ColumnSet("fullname"),
                                LinkEntities =
                                {
                                    new LinkEntity(SystemUser.EntityLogicalName, SystemUser.EntityLogicalName, "modifiedby", "systemuserid", JoinOperator.Inner)
                                    {
                                        Columns = new ColumnSet("fullname")
                                    }
                                }
                            }
                        }
                    }
                }
            };

            var result = _service.RetrieveMultiple(query);
            var resultingEntity = result.Entities[0];
            Assert.Equal(4, resultingEntity.Attributes.Count);
            Assert.Equal("User3", ((AliasedValue)resultingEntity["systemuser1.fullname"]).Value);
            Assert.Equal("User2", ((AliasedValue)resultingEntity["systemuserwithalias.fullname"]).Value);
            Assert.Equal("User1", ((AliasedValue)resultingEntity["systemuser2.fullname"]).Value);
        }

        [Fact]
        public void TestRetriveMultipleWithLinkEntityWithAlternateNullField()
        {
            // ARRANGE

            List<Entity> initialEntities = new List<Entity>();

            Entity parentEntity = new Entity("parent");
            parentEntity["parentname"] = "parent name";
            parentEntity.Id = Guid.NewGuid();
            initialEntities.Add(parentEntity);

            // create the first child which has the "myvalue" field set to "value"
            Entity childEntity1 = new Entity("child");
            childEntity1["parent"] = parentEntity.ToEntityReference();
            childEntity1["name"] = "entity1";
            childEntity1["myvalue"] = "value";
            childEntity1.Id = Guid.NewGuid();
            initialEntities.Add(childEntity1);

            // create the second child which has the "myvalue" field set to null
            Entity childEntity2 = new Entity("child");
            childEntity2["parent"] = parentEntity.ToEntityReference();
            childEntity2["name"] = "entity2";
            childEntity2["myvalue"] = null;
            childEntity2.Id = Guid.NewGuid();
            initialEntities.Add(childEntity2);

            _context.Initialize(initialEntities);

            // the query selects the "parent" entity, and joins to the "child" entities
            QueryExpression query = new QueryExpression("parent");
            query.ColumnSet = new ColumnSet("parentname");

            LinkEntity link = new LinkEntity("parent", "child", "parentid", "parent", JoinOperator.Inner);
            link.EntityAlias = "c";
            link.Columns = new ColumnSet("name", "myvalue");

            query.LinkEntities.Add(link);

            // ACT

            DataCollection<Entity> results = _service.RetrieveMultiple(query).Entities;

            // ASSERT

            // fields for the first entity work as expected...
            string entity1Name = results[0].GetAttributeValue<AliasedValue>("c.name").Value as string;
            string entity1Value = results[0].GetAttributeValue<AliasedValue>("c.myvalue").Value as string;

            Assert.Equal("entity1", entity1Name);
            Assert.Equal("value", entity1Value);

            // fields for the second entity do not.  
            // The child "name" field is correct, but the "myvalue" field is returning the value of the previous
            // entity when it should be returning null
            string entity2Name = results[1].GetAttributeValue<AliasedValue>("c.name").Value as string;
            string entity2Value = results[1].GetAttributeValue<AliasedValue>("c.myvalue")?.Value as string;

            // this works fine:
            Assert.Equal("entity2", entity2Name);

            // this fails (entity2Value is "value")
            Assert.Null(entity2Value);
        }

        [Fact]
        public void Should_Find_Record_With_Associations_When_Exists_Join_Is_Used()
        {
            // Arrange
            Entity account = new Entity("account", Guid.NewGuid());

            Entity contact = new Entity("contact", Guid.NewGuid())
            {
                ["parentcustomerid"] = account.ToEntityReference(),
            };
            List<Entity> entities = new List<Entity>()
            {
                account, contact
            };

            _context.Initialize(entities);

            QueryExpression queryExpression = new QueryExpression("account");
            queryExpression.ColumnSet = new ColumnSet(true);

            LinkEntity contactQuery = queryExpression.AddLink("contact", "accountid", "parentcustomerid");
            contactQuery.JoinOperator = JoinOperator.Exists;
            contactQuery.EntityAlias = "c";
            
            // Act
            EntityCollection results = _service.RetrieveMultiple(queryExpression);
            
            // Assert
            Assert.Single(results.Entities);
        }
        
        
        [Fact]
        public void Should_Not_Find_Record_Without_Associations_When_Exists_Join_Is_Used()
        {
            // Arrange
            Entity account = new Entity("account", Guid.NewGuid());

            Entity contact = new Entity("contact", Guid.NewGuid());
            List<Entity> entities = new List<Entity>()
            {
                account, contact
            };

            _context.Initialize(entities);

            QueryExpression queryExpression = new QueryExpression("account");
            queryExpression.ColumnSet = new ColumnSet(true);

            LinkEntity contactQuery = queryExpression.AddLink("contact", "accountid", "parentcustomerid");
            contactQuery.JoinOperator = JoinOperator.Exists;
            contactQuery.EntityAlias = "c";
            
            // Act
            EntityCollection result = _service.RetrieveMultiple(queryExpression);
            
            // Assert
            Assert.Empty(result.Entities);
        }

        [Fact]
        public void Should_Return_Only_Outer_Record_When_Exists_Join_Is_Used()
        {
            // Arrange
            Entity account = new Entity("account", Guid.NewGuid());

            Entity contact = new Entity("contact", Guid.NewGuid())
            {
                ["parentcustomerid"] = account.ToEntityReference(),
                ["firstname"] = "firstname"
            };
            List<Entity> entities = new List<Entity>()
            {
                account, contact
            };

            _context.Initialize(entities);

            QueryExpression queryExpression = new QueryExpression("account");
            queryExpression.ColumnSet = new ColumnSet(true);

            LinkEntity contactQuery = queryExpression.AddLink("contact", "accountid", "parentcustomerid");
            contactQuery.JoinOperator = JoinOperator.Exists;
            contactQuery.EntityAlias = "c";
            contactQuery.Columns = new ColumnSet(true);
            
            // Act
            EntityCollection result = _service.RetrieveMultiple(queryExpression);
            
            // Assert
            Assert.Single(result.Entities);
            
            Entity outerRecord = result.Entities.First();
            Assert.Null(outerRecord.GetAttributeValue<AliasedValue>("c.firstname"));
        }
    }
}