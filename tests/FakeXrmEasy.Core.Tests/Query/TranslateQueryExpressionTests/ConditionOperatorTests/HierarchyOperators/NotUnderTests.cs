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
    public class NotUnderTests: HierarchyOperatorTestsBase
    {
        [Fact]
        public void Should_return_the_grand_parent_account_when_querying_not_under_the_top_most_account()
        {
            InitializeDefaultDataSet();
            var query = new QueryExpression { EntityName = Account.EntityLogicalName, ColumnSet = new ColumnSet(true) };
            query.Criteria.AddCondition("accountid", ConditionOperator.NotUnder, _grandParentAccount.Id);

            var result = _service.RetrieveMultiple(query);

            Assert.NotNull(result);
            Assert.Equal(2, result.Entities.Count);
            Assert.NotNull(result.Entities.FirstOrDefault(e => e.Id == _grandParentAccount.Id));
            Assert.NotNull(result.Entities.FirstOrDefault(e => e.Id == _unrelatedAccount.Id));
        }
        
        [Fact]
        public void Should_return_parent_and_grand_parent_account_when_querying_the_parent_account()
        {
            InitializeDefaultDataSet();
            var query = new QueryExpression { EntityName = Account.EntityLogicalName, ColumnSet = new ColumnSet(true) };
            query.Criteria.AddCondition("accountid", ConditionOperator.NotUnder, _parentAccount.Id);

            var result = _service.RetrieveMultiple(query);

            Assert.NotNull(result);
            Assert.Equal(3, result.Entities.Count);
            Assert.NotNull(result.Entities.FirstOrDefault(e => e.Id == _grandParentAccount.Id));
            Assert.NotNull(result.Entities.FirstOrDefault(e => e.Id == _parentAccount.Id));
            Assert.NotNull(result.Entities.FirstOrDefault(e => e.Id == _unrelatedAccount.Id));
        }
        
        [Fact]
        public void Should_return_all_accounts_not_under_nested_child_account1_which_also_includes_above_or_equal_and_its_siblings()
        {
            InitializeDefaultDataSet();
            var query = new QueryExpression { EntityName = Account.EntityLogicalName, ColumnSet = new ColumnSet(true) };
            query.Criteria.AddCondition("accountid", ConditionOperator.NotUnder, _nestedChildAccount1.Id);

            var result = _service.RetrieveMultiple(query);

            Assert.NotNull(result);
            Assert.Equal(6, result.Entities.Count);
            
            Assert.NotNull(result.Entities.FirstOrDefault(e => e.Id == _grandParentAccount.Id));
            Assert.NotNull(result.Entities.FirstOrDefault(e => e.Id == _parentAccount.Id));
            Assert.NotNull(result.Entities.FirstOrDefault(e => e.Id == _childAccount.Id));
            Assert.NotNull(result.Entities.FirstOrDefault(e => e.Id == _nestedChildAccount1.Id));
            Assert.NotNull(result.Entities.FirstOrDefault(e => e.Id == _nestedChildAccount2.Id));
            Assert.NotNull(result.Entities.FirstOrDefault(e => e.Id == _unrelatedAccount.Id));
        }
        
        [Fact]
        public void Should_throw_exception_when_the_entity_does_not_have_a_hierarchical_relationship()
        {
            var query = new QueryExpression { EntityName = dv_test.EntityLogicalName, ColumnSet = new ColumnSet(true) };
            query.Criteria.AddCondition("dv_testid", ConditionOperator.NotUnder, _noHierarchyEntity.Id); 
            
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
            query.Criteria.AddCondition("accountid", ConditionOperator.NotUnder, childAccount.Id); 
            
            var ex = XAssert.ThrowsFaultCode(ErrorCodes.EntityLoopExists, () => _service.RetrieveMultiple(query));
            Assert.Contains("Loop exists in this entity hierarchy", ex.Message);
        }
        
        [Fact]
        public void Should_return_all_accounts_when_querying_a_non_existing_account()
        {
            InitializeDefaultDataSet();
            var query = new QueryExpression { EntityName = Account.EntityLogicalName, ColumnSet = new ColumnSet(true) };
            query.Criteria.AddCondition("accountid", ConditionOperator.NotUnder, Guid.NewGuid());

            var result = _service.RetrieveMultiple(query);
            
            Assert.NotNull(result);
            Assert.Equal(6, result.Entities.Count);
            
            Assert.NotNull(result.Entities.FirstOrDefault(e => e.Id == _grandParentAccount.Id));
            Assert.NotNull(result.Entities.FirstOrDefault(e => e.Id == _parentAccount.Id));
            Assert.NotNull(result.Entities.FirstOrDefault(e => e.Id == _childAccount.Id));
            Assert.NotNull(result.Entities.FirstOrDefault(e => e.Id == _nestedChildAccount1.Id));
            Assert.NotNull(result.Entities.FirstOrDefault(e => e.Id == _nestedChildAccount2.Id));
            Assert.NotNull(result.Entities.FirstOrDefault(e => e.Id == _unrelatedAccount.Id));
        }
    }
}
#endif