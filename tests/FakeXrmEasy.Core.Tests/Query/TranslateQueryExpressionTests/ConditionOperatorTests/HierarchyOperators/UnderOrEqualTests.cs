#if FAKE_XRM_EASY_365 || FAKE_XRM_EASY_9
using System;
using System.Collections.Generic;
using System.Linq;
using DataverseEntities;
using FakeXrmEasy.Abstractions;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Xunit;

namespace FakeXrmEasy.Core.Tests.Query.TranslateQueryExpressionTests.ConditionOperatorTests.HierarchyOperators
{
    public class UnderOrEqualTests: HierarchyOperatorTestsBase
    {
        [Fact]
        public void Should_return_the_one_account_when_querying_under_the_most_nested_child_account()
        {
            InitializeDefaultDataSet();
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
            InitializeDefaultDataSet();
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
            InitializeDefaultDataSet();
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
        
        [Fact]
        public void Should_throw_exception_when_the_entity_does_not_have_a_hierarchical_relationship()
        {
            var query = new QueryExpression { EntityName = dv_test.EntityLogicalName, ColumnSet = new ColumnSet(true) };
            query.Criteria.AddCondition("dv_testid", ConditionOperator.UnderOrEqual, _noHierarchyEntity.Id); 
            
            XAssert.ThrowsFaultCode(ErrorCodes.MissingHierarchicalRelationshipForOperator, () => _service.RetrieveMultiple(query));
        }
        
        [Fact]
        public void Should_detect_and_raise_appropriate_infinite_loop_exception()
        {
            var parentAccount = new Account()
            {
                Id = Guid.NewGuid(),
                Name = "Parent"
            };
            var childAccount = new Account()
            {
                Id = Guid.NewGuid(),
                Name =  "Child",
                ParentAccountId = parentAccount.ToEntityReference()
            };
            parentAccount.ParentAccountId = childAccount.ToEntityReference();
            
            _context.Initialize(new List<Entity>()
            {
                parentAccount, childAccount
            });
            
            var query = new QueryExpression { EntityName = Account.EntityLogicalName, ColumnSet = new ColumnSet(true) };
            query.Criteria.AddCondition("accountid", ConditionOperator.UnderOrEqual, parentAccount.Id); 
            
            var ex = XAssert.ThrowsFaultCode(ErrorCodes.EntityLoopExists, () => _service.RetrieveMultiple(query));
            Assert.Contains("Loop exists in this entity hierarchy", ex.Message);
        }
    }
}
#endif