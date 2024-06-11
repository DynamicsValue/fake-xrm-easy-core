using FakeXrmEasy.Abstractions;
using Microsoft.Xrm.Sdk.Query;
using Xunit;

namespace FakeXrmEasy.Core.Tests.Issues
{
    public class Issue0096 : FakeXrmEasyTestsBase
    {
        [Fact]
        public void Reproduce_issue_96()
        {
            var query = new QueryExpression("account");
            query.TopCount = 2;
            query.Criteria.AddCondition("numberofemployees", ConditionOperator.In, new int[] { 0, 1 });

            var ex = XAssert.ThrowsFaultCode(ErrorCodes.InvalidArgument, () => _service.RetrieveMultiple(query));
            Assert.NotNull(ex);
            Assert.Equal("Condition for attribute 'account.numberofemployees': expected argument(s) of a different type but received 'System.Int32[]'.", ex.Message);
        }
    }
}
