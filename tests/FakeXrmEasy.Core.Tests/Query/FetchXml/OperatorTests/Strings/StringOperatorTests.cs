using System;

using Crm;
using Microsoft.Xrm.Sdk.Query;
using System.Reflection;
using Xunit;
using FakeXrmEasy.Query;

namespace FakeXrmEasy.Tests.FakeContextTests.FetchXml.OperatorTests.Strings
{
    public class StringOperatorTests : FakeXrmEasyTestsBase
    {
        [Fact]
        public void FetchXml_Operator_Lt_Translation()
        {
            _context.EnableProxyTypes(Assembly.GetAssembly(typeof(Contact)));

            var fetchXml = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                              <entity name='contact'>
                                    <attribute name='fullname' />
                                    <attribute name='contactid' />
                                        <filter type='and'>
                                            <condition attribute='nickname' operator='lt' value='Bob' />
                                        </filter>
                                  </entity>
                            </fetch>";

            var ct = new Contact();

            var query = fetchXml.ToQueryExpression(_context);

            Assert.True(query.Criteria != null);
            Assert.Equal(1, query.Criteria.Conditions.Count);
            Assert.Equal("nickname", query.Criteria.Conditions[0].AttributeName);
            Assert.Equal(ConditionOperator.LessThan, query.Criteria.Conditions[0].Operator);
            Assert.Equal("Bob", query.Criteria.Conditions[0].Values[0]);
        }

        [Fact]
        public void FetchXml_Operator_Lt_Execution()
        {
            var fetchXml = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                              <entity name='contact'>
                                    <attribute name='nickname' />
                                        <filter type='and'>
                                            <condition attribute='nickname' operator='lt' value='C' />
                                        </filter>
                                  </entity>
                            </fetch>";

            var ct1 = new Contact() { Id = Guid.NewGuid(), NickName = "Alice" };
            var ct2 = new Contact() { Id = Guid.NewGuid(), NickName = "Bob" };
            var ct3 = new Contact() { Id = Guid.NewGuid(), NickName = "Nati" };
            
            _context.Initialize(new[] { ct1, ct2, ct3 });

            var collection = _service.RetrieveMultiple(new FetchExpression(fetchXml));

            Assert.Equal(2, collection.Entities.Count);
            Assert.Equal("Alice", collection.Entities[0]["nickname"]);
            Assert.Equal("Bob", collection.Entities[1]["nickname"]);
        }

        [Fact]
        public void FetchXml_Operator_Gt_Translation()
        {
            _context.EnableProxyTypes(Assembly.GetAssembly(typeof(Contact)));

            var fetchXml = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                              <entity name='contact'>
                                    <attribute name='fullname' />
                                    <attribute name='contactid' />
                                        <filter type='and'>
                                            <condition attribute='nickname' operator='gt' value='Bob' />
                                        </filter>
                                  </entity>
                            </fetch>";

            var ct = new Contact();

            var query = fetchXml.ToQueryExpression(_context);

            Assert.True(query.Criteria != null);
            Assert.Single(query.Criteria.Conditions);
            Assert.Equal("nickname", query.Criteria.Conditions[0].AttributeName);
            Assert.Equal(ConditionOperator.GreaterThan, query.Criteria.Conditions[0].Operator);
            Assert.Equal("Bob", query.Criteria.Conditions[0].Values[0]);
        }

        [Fact]
        public void FetchXml_Operator_Gt_Execution()
        {
            var fetchXml = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                              <entity name='contact'>
                                    <attribute name='nickname' />
                                        <filter type='and'>
                                            <condition attribute='nickname' operator='gt' value='Alice' />
                                        </filter>
                                  </entity>
                            </fetch>";

            var ct1 = new Contact() { Id = Guid.NewGuid(), NickName = "Alice" };
            var ct2 = new Contact() { Id = Guid.NewGuid(), NickName = "Bob" };
            var ct3 = new Contact() { Id = Guid.NewGuid(), NickName = "Nati" };
            
            _context.Initialize(new[] { ct1, ct2, ct3 });

            var collection = _service.RetrieveMultiple(new FetchExpression(fetchXml));

            Assert.Equal(2, collection.Entities.Count);
            Assert.Equal("Bob", collection.Entities[0]["nickname"]);
            Assert.Equal("Nati", collection.Entities[1]["nickname"]);
        }
    }
}
