#if FAKE_XRM_EASY_365 || FAKE_XRM_EASY_9
using System.Linq;
using DataverseEntities;
using FakeXrmEasy.Abstractions;
using Microsoft.Xrm.Sdk.Query;
using Xunit;

namespace FakeXrmEasy.Core.Tests.Query.TranslateQueryExpressionTests.ConditionOperatorTests.HierarchyOperators
{
    public class AboveOperatorTests: HierarchyOperatorTestsBase
    {
        [Fact]
        public void Should_return_no_accounts_when_querying_above_the_top_most_account()
        {
            var query = new QueryExpression { EntityName = Account.EntityLogicalName, ColumnSet = new ColumnSet(true) };
            query.Criteria.AddCondition("accountid", ConditionOperator.Above, _grandParentAccount.Id);

            var result = _service.RetrieveMultiple(query);

            Assert.NotNull(result);
            Assert.Empty(result.Entities);
        }
        
        [Fact]
        public void Should_return_grand_parent_account_when_querying_the_parent_account()
        {
            var query = new QueryExpression { EntityName = Account.EntityLogicalName, ColumnSet = new ColumnSet(true) };
            query.Criteria.AddCondition("accountid", ConditionOperator.Above, _parentAccount.Id);

            var result = _service.RetrieveMultiple(query);

            Assert.NotNull(result);
            Assert.Single(result.Entities);
            Assert.Equal(_grandParentAccount.Id, result.Entities[0].Id);
        }
        
        [Fact]
        public void Should_return_all_the_parent_accounts_of_the_most_nested_child_account()
        {
            var query = new QueryExpression { EntityName = Account.EntityLogicalName, ColumnSet = new ColumnSet(true) };
            query.Criteria.AddCondition("accountid", ConditionOperator.Above, _nestedChildAccount1.Id);

            var result = _service.RetrieveMultiple(query);

            Assert.NotNull(result);
            Assert.Equal(3, result.Entities.Count);
            
            Assert.NotNull(result.Entities.FirstOrDefault(e => e.Id == _grandParentAccount.Id));
            Assert.NotNull(result.Entities.FirstOrDefault(e => e.Id == _parentAccount.Id));
            Assert.NotNull(result.Entities.FirstOrDefault(e => e.Id == _childAccount.Id));
        }
        
        [Fact]
        public void Should_throw_exception_when_the_entity_does_not_have_a_hierarchical_relationship()
        {
            var query = new QueryExpression { EntityName = dv_test.EntityLogicalName, ColumnSet = new ColumnSet(true) };
            query.Criteria.AddCondition("dv_testid", ConditionOperator.Above, _noHierarchyEntity.Id); 
            
            XAssert.ThrowsFaultCode(ErrorCodes.MissingHierarchicalRelationshipForOperator, () => _service.RetrieveMultiple(query));
        }
    }
}
#endif