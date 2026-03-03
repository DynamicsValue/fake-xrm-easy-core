#if FAKE_XRM_EASY_9
using FakeXrmEasy.Abstractions;
using Microsoft.Xrm.Sdk.Query;
using Xunit;

namespace FakeXrmEasy.Core.Tests.Query.TranslateQueryExpressionTests.AggregationsTests
{
    public class InvalidAggregateTests: FakeXrmEasyTestsBase
    {
        [Fact]
        public void Should_return_invalid_operation_exception_when_using_aggregate_and_specifying_all_columns()
        {
            QueryExpression query = new QueryExpression("account")
            {
                ColumnSet = new ColumnSet(true)
                {
                    AttributeExpressions = {
                        new XrmAttributeExpression{
                            AttributeName = "accountid",
                            Alias = "accountcount",
                            AggregateType = XrmAggregateType.Count
                        }
                    }
                }
            };
            
            var ex = XAssert.ThrowsFaultCode(ErrorCodes.InvalidOperation, () => _service.RetrieveMultiple(query));
            Assert.Equal("Attribute can not be specified if an aggregate operation is requested.", ex.Message);
        }
        
        [Fact]
        public void Should_return_invalid_operation_exception_when_using_aggregate_and_specifying_a_column_in_the_column_set()
        {
            QueryExpression query = new QueryExpression("account")
            {
                ColumnSet = new ColumnSet("name")
                {
                    AttributeExpressions = {
                        new XrmAttributeExpression{
                            AttributeName = "accountid",
                            Alias = "accountcount",
                            AggregateType = XrmAggregateType.Count
                        }
                    }
                }
            };
            
            var ex = XAssert.ThrowsFaultCode(ErrorCodes.InvalidOperation, () => _service.RetrieveMultiple(query));
            Assert.Equal("Attribute can not be specified if an aggregate operation is requested.", ex.Message);
        }
    }
}
#endif