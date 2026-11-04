#if FAKE_XRM_EASY_365 || FAKE_XRM_EASY_9
using System.Linq;
using DataverseEntities;
using Microsoft.Xrm.Sdk.Query;
using Xunit;

namespace FakeXrmEasy.Core.Tests.Query.TranslateQueryExpressionTests.ConditionOperatorTests.HierarchyOperators
{
    public class UnderOrEqualTests: HierarchyOperatorTestsBase
    {
        [Fact]
        public void Should_return_the_one_account_when_querying_under_the_most_nested_child_account()
        {
            var query = new QueryExpression { EntityName = Account.EntityLogicalName, ColumnSet = new ColumnSet(true) };
            query.Criteria.AddCondition("accountid", ConditionOperator.UnderOrEqual, _nestedChildAccount1.Id);

            var result = _service.RetrieveMultiple(query);

            Assert.NotNull(result);
            Assert.Single(result.Entities);
            
            Assert.NotNull(result.Entities.FirstOrDefault(e => e.Id == _nestedChildAccount1.Id));
        }
        
        [Fact]
        public void Should_return_the_two_nested_child_accounts_and_the_parent_when_querying_the_child_account()
        {
            var query = new QueryExpression { EntityName = Account.EntityLogicalName, ColumnSet = new ColumnSet(true) };
            query.Criteria.AddCondition("accountid", ConditionOperator.UnderOrEqual, _childAccount.Id);

            var result = _service.RetrieveMultiple(query);

            Assert.NotNull(result);
            Assert.Equal(3, result.Entities.Count);
            Assert.NotNull(result.Entities.FirstOrDefault(e => e.Id == _childAccount.Id));
            Assert.NotNull(result.Entities.FirstOrDefault(e => e.Id == _nestedChildAccount1.Id));
            Assert.NotNull(result.Entities.FirstOrDefault(e => e.Id == _nestedChildAccount2.Id));
        }
        
        [Fact]
        public void Should_return_all_the_hierarchy_under_the_grand_parent_account_including_the_parent_account()
        {
            var query = new QueryExpression { EntityName = Account.EntityLogicalName, ColumnSet = new ColumnSet(true) };
            query.Criteria.AddCondition("accountid", ConditionOperator.UnderOrEqual, _grandParentAccount.Id);

            var result = _service.RetrieveMultiple(query);

            Assert.NotNull(result);
            Assert.Equal(5, result.Entities.Count);
            
            Assert.NotNull(result.Entities.FirstOrDefault(e => e.Id == _grandParentAccount.Id));
            Assert.NotNull(result.Entities.FirstOrDefault(e => e.Id == _parentAccount.Id));
            Assert.NotNull(result.Entities.FirstOrDefault(e => e.Id == _childAccount.Id));
            Assert.NotNull(result.Entities.FirstOrDefault(e => e.Id == _nestedChildAccount1.Id));
            Assert.NotNull(result.Entities.FirstOrDefault(e => e.Id == _nestedChildAccount2.Id));
        }
    }
}
#endif