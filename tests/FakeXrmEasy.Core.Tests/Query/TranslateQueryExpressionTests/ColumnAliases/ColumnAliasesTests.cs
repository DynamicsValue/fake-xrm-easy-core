using System;
using System.Collections.Generic;
using DataverseEntities;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Xunit;

namespace FakeXrmEasy.Core.Tests.Query.TranslateQueryExpressionTests.ColumnAliases
{
    public class ColumnAliasesTests: FakeXrmEasyTestsBase
    {
        private readonly Account _account;
        private readonly Contact _contact;
        
        public ColumnAliasesTests()
        {
            _contact = new Contact()
            {
                Id = Guid.NewGuid(),
                AccountRoleCode = contact_accountrolecode.Influencer,
                ["fullname"] = "Andy Timmons"
            };
            
            _account = new Account()
            {
                Id = Guid.NewGuid(),
                Name = "Contoso",
                AccountClassificationCode = account_accountclassificationcode.DefaultValue
            };
        }

        [Fact]
        public void Should_return_column_aliases_as_an_aliased_value()
        {
            _context.Initialize(_account);
            
            QueryExpression query = new QueryExpression("account")
            {
                TopCount = 3,
                ColumnSet = new ColumnSet("name")
                {
                    AttributeExpressions = {
                        new XrmAttributeExpression{
                            AttributeName = "accountclassificationcode",
                            Alias = "classificationcode"
                        },
                        new XrmAttributeExpression{
                            AttributeName = "createdby",
                            Alias = "whocreated"
                        },
                        new XrmAttributeExpression{
                            AttributeName = "createdon",
                            Alias = "whencreated"
                        }
                    }
                }
            };
            
            var entityCollection = _service.RetrieveMultiple(query);
            Assert.Single(entityCollection.Entities);

            var resultAccount = entityCollection.Entities[0];
            Assert.IsNotType<AliasedValue>(resultAccount["name"]);
            Assert.IsType<AliasedValue>(resultAccount["classificationcode"]);
            Assert.IsType<AliasedValue>(resultAccount["whocreated"]);
            Assert.IsType<AliasedValue>(resultAccount["whencreated"]);
        }

        [Fact]
        public void Should_return_column_alias_in_linked_entity_without_prepending_the_table_alias()
        {
            _account.PrimaryContactId = _contact.ToEntityReference();
            _context.Initialize(new List<Entity>() {_account, _contact});
            
            QueryExpression query = new QueryExpression("account")
            {
                TopCount = 3,
                ColumnSet = new ColumnSet("name")
                {
                    AttributeExpressions = {
                        new XrmAttributeExpression{
                            AttributeName = "accountclassificationcode",
                            Alias = "classificationcode"
                        }
                    }
                },
                LinkEntities = {
                    new LinkEntity()
                    {
                        LinkFromEntityName = "account",
                        LinkToEntityName = "contact",
                        LinkFromAttributeName = "primarycontactid",
                        LinkToAttributeName = "contactid",
                        JoinOperator = JoinOperator.Inner,
                        EntityAlias = "person",
                        Columns = new ColumnSet("fullname"){
                            AttributeExpressions = {
                                new XrmAttributeExpression{
                                    AttributeName = "accountrolecode",
                                    Alias = "role"
                                }
                            }
                        }
                    }
                }
            };
            
            var entityCollection = _service.RetrieveMultiple(query);
            Assert.Single(entityCollection.Entities);
            
            var resultAccount = entityCollection.Entities[0];
            Assert.True(resultAccount.Attributes.ContainsKey("name"));
            Assert.True(resultAccount.Attributes.ContainsKey("classificationcode"));
            Assert.True(resultAccount.Attributes.ContainsKey("person.fullname"));
            Assert.True(resultAccount.Attributes.ContainsKey("role"));
            
            Assert.IsNotType<AliasedValue>(resultAccount["name"]);
            Assert.IsType<AliasedValue>(resultAccount["classificationcode"]);
            Assert.IsType<AliasedValue>(resultAccount["person.fullname"]);
            Assert.IsType<AliasedValue>(resultAccount["role"]);
        }
    }
}