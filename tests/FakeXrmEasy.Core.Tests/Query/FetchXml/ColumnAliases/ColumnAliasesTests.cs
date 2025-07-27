using System;
using System.Reflection;
using DataverseEntities;
using FakeXrmEasy.Query;
using Microsoft.Xrm.Sdk.Query;
using Xunit;

namespace FakeXrmEasy.Core.Tests.FakeContextTests.FetchXml.ColumnAliases
{
    public class ColumnAliasesTests: FakeXrmEasyTestsBase
    {
        private readonly Contact _contact;
        
        public ColumnAliasesTests()
        {
            _context.EnableProxyTypes(Assembly.GetAssembly(typeof(Contact)));

            _contact = new Contact()
            {
                Id = Guid.NewGuid(),
                AccountRoleCode = contact_accountrolecode.Influencer
            };
        }

        [Fact]
        public void Should_translate_a_column_alias_into_an_xrm_attribute_expression()
        {
            _context.Initialize(_contact);
            
            var fetchXml = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                              <entity name='contact'>
                                    <attribute name='accountrolecode' alias='role' />
                                  </entity>
                            </fetch>";

            var query = fetchXml.ToQueryExpression(_context);

            Assert.True(query.ColumnSet != null);
            Assert.Single(query.ColumnSet.AttributeExpressions);

            var attributeExpression = query.ColumnSet.AttributeExpressions[0];
            Assert.Equal("role", attributeExpression.Alias);
            Assert.Equal("accountrolecode", attributeExpression.AttributeName);
            Assert.Equal(XrmAggregateType.None, attributeExpression.AggregateType);
        }
    }
}