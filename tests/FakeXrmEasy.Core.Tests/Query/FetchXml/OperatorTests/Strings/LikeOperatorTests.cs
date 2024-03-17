using System.Reflection;
using DataverseEntities;
using FakeXrmEasy.Query;
using Microsoft.Xrm.Sdk.Query;
using Xunit;

namespace FakeXrmEasy.Core.Tests.FakeContextTests.FetchXml.OperatorTests.Strings
{
    public class LikeOperatorTests: FakeXrmEasyTestsBase
    {
        public LikeOperatorTests()
        {
            _context.EnableProxyTypes(Assembly.GetAssembly(typeof(Contact)));
        }
        
        [Fact]
        public void FetchXml_Operator_Like_As_BeginsWith()
        {
            var fetchXml = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                              <entity name='contact'>
                                    <attribute name='fullname' />
                                    <attribute name='telephone1' />
                                    <attribute name='contactid' />
                                        <filter type='and'>
                                            <condition attribute='fullname' operator='like' value='Messi%' />
                                        </filter>
                                  </entity>
                            </fetch>";

            var query = fetchXml.ToQueryExpression(_context);

            Assert.True(query.Criteria != null);
            Assert.Single(query.Criteria.Conditions);
            Assert.Equal("fullname", query.Criteria.Conditions[0].AttributeName);
            Assert.Equal(ConditionOperator.Like, query.Criteria.Conditions[0].Operator);
            Assert.Equal("Messi%", query.Criteria.Conditions[0].Values[0].ToString());
        }
        
        [Fact]
        public void FetchXml_Operator_Like_As_EndsWith()
        {
            var fetchXml = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                              <entity name='contact'>
                                    <attribute name='fullname' />
                                    <attribute name='telephone1' />
                                    <attribute name='contactid' />
                                        <filter type='and'>
                                            <condition attribute='fullname' operator='like' value='%Messi' />
                                        </filter>
                                  </entity>
                            </fetch>";

            var query = fetchXml.ToQueryExpression(_context);

            Assert.True(query.Criteria != null);
            Assert.Single(query.Criteria.Conditions);
            Assert.Equal("fullname", query.Criteria.Conditions[0].AttributeName);
            Assert.Equal(ConditionOperator.Like, query.Criteria.Conditions[0].Operator);
            Assert.Equal("%Messi", query.Criteria.Conditions[0].Values[0].ToString());
        }
        
        [Fact]
        public void FetchXml_Operator_Like()
        {
            var fetchXml = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                              <entity name='contact'>
                                    <attribute name='fullname' />
                                    <attribute name='telephone1' />
                                    <attribute name='contactid' />
                                        <filter type='and'>
                                            <condition attribute='fullname' operator='like' value='%Messi%' />
                                        </filter>
                                  </entity>
                            </fetch>";

            var query = fetchXml.ToQueryExpression(_context);

            Assert.True(query.Criteria != null);
            Assert.Single(query.Criteria.Conditions);
            Assert.Equal("fullname", query.Criteria.Conditions[0].AttributeName);
            Assert.Equal(ConditionOperator.Like, query.Criteria.Conditions[0].Operator);
            Assert.Equal("%Messi%", query.Criteria.Conditions[0].Values[0].ToString());
        }
        
        [Fact]
        public void FetchXml_Operator_NotLike()
        {
            var fetchXml = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                              <entity name='contact'>
                                    <attribute name='fullname' />
                                    <attribute name='telephone1' />
                                    <attribute name='contactid' />
                                        <filter type='and'>
                                            <condition attribute='fullname' operator='not-like' value='%Messi%' />
                                        </filter>
                                  </entity>
                            </fetch>";

            var query = fetchXml.ToQueryExpression(_context);

            Assert.True(query.Criteria != null);
            Assert.Single(query.Criteria.Conditions);
            Assert.Equal("fullname", query.Criteria.Conditions[0].AttributeName);
            Assert.Equal(ConditionOperator.NotLike, query.Criteria.Conditions[0].Operator);
            Assert.Equal("%Messi%", query.Criteria.Conditions[0].Values[0].ToString());
        }
        
        [Fact]
        public void FetchXml_Operator_NotLike_As_Not_BeginWith()
        {
            var fetchXml = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                              <entity name='contact'>
                                    <attribute name='fullname' />
                                    <attribute name='telephone1' />
                                    <attribute name='contactid' />
                                        <filter type='and'>
                                            <condition attribute='fullname' operator='not-like' value='Messi%' />
                                        </filter>
                                  </entity>
                            </fetch>";

            var query = fetchXml.ToQueryExpression(_context);

            Assert.True(query.Criteria != null);
            Assert.Single(query.Criteria.Conditions);
            Assert.Equal("fullname", query.Criteria.Conditions[0].AttributeName);
            Assert.Equal(ConditionOperator.NotLike, query.Criteria.Conditions[0].Operator);
            Assert.Equal("Messi%", query.Criteria.Conditions[0].Values[0].ToString());
        }

        [Fact]
        public void FetchXml_Operator_NotLike_As_Not_EndWith()
        {
            var fetchXml = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                              <entity name='contact'>
                                    <attribute name='fullname' />
                                    <attribute name='telephone1' />
                                    <attribute name='contactid' />
                                        <filter type='and'>
                                            <condition attribute='fullname' operator='not-like' value='%Messi' />
                                        </filter>
                                  </entity>
                            </fetch>";

            var query = fetchXml.ToQueryExpression(_context);

            Assert.True(query.Criteria != null);
            Assert.Single(query.Criteria.Conditions);
            Assert.Equal("fullname", query.Criteria.Conditions[0].AttributeName);
            Assert.Equal(ConditionOperator.NotLike, query.Criteria.Conditions[0].Operator);
            Assert.Equal("%Messi", query.Criteria.Conditions[0].Values[0].ToString());
        }
    }
}