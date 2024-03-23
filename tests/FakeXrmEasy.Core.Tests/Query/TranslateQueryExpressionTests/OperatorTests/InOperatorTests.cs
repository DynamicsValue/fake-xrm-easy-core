using System;
using DataverseEntities;
using FakeXrmEasy.Abstractions;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Xunit;

namespace FakeXrmEasy.Core.Tests.Query.TranslateQueryExpressionTests.OperatorTests
{
    public class InOperatorTests: FakeXrmEasyTestsBase
    {
        private readonly dv_test _testEntity;
        private readonly Entity _testLateBoundEntity;
        private readonly Account _account;

        public InOperatorTests()
        {
            _account = new Account()
            {
                Id = Guid.NewGuid(),
                StatusCode = account_statuscode.Active
            };
            _testEntity = new dv_test()
            {
                Id = Guid.NewGuid(),
                dv_choice_multiple = new[] { dv_test_dv_choice_multiple.Option1 , dv_test_dv_choice_multiple.Option2 }
            };
            _testLateBoundEntity = new Entity("dv_test")
            {
                Id = Guid.NewGuid(),
                ["dv_choice_multiple"] = new[] { dv_test_dv_choice_multiple.Option1 , dv_test_dv_choice_multiple.Option2 }
            };
        }
        [Fact]
        public void Should_throw_exception_when_an_array_of_int_is_used_to_filter_status_code()
        {
            _context.Initialize(_account);
            
            QueryExpression query = new QueryExpression("account") { TopCount = 10 };
            query.Criteria.AddCondition("statuscode", ConditionOperator.In, new int[] { 0, 1 });

            var ex = XAssert.ThrowsFaultCode(ErrorCodes.InvalidArgument, () => _service.RetrieveMultiple(query));
            Assert.Equal("Condition for attribute 'account.statuscode': expected argument(s) of a different type but received 'System.Int32[]'.", ex.Message);
        }
        
        [Fact]
        public void Should_throw_exception_when_an_array_of_enum_is_used_to_filter_status_code()
        {
            _context.Initialize(_account);
            
            QueryExpression query = new QueryExpression("account") { TopCount = 10 };
            query.Criteria.AddCondition("statuscode", ConditionOperator.In, new account_statuscode[] { account_statuscode.Active });

            var ex = XAssert.ThrowsFaultCode(ErrorCodes.InvalidArgument, () => _service.RetrieveMultiple(query));
            Assert.Equal("Condition for attribute 'account.statuscode': expected argument(s) of a different type but received 'DataverseEntities.account_statuscode[]'.", ex.Message);
        }
        
        [Fact]
        public void Should_succeed_when_several_int_parameters_are_used_to_filter_status_code()
        {
            _context.Initialize(_account);
            
            QueryExpression query = new QueryExpression("account") { TopCount = 10 };
            query.Criteria.AddCondition("statuscode", ConditionOperator.In, 0, 1);

            var result = _service.RetrieveMultiple(query);
            Assert.NotEmpty(result.Entities);
        }


        [Fact]
        public void Should_throw_exception_when_an_array_of_int_is_used_to_filter_a_multi_option_set()
        {
            _context.Initialize(_testEntity);
            
            QueryExpression query = new QueryExpression(dv_test.EntityLogicalName) { TopCount = 10 };
            query.Criteria.AddCondition(dv_test.Fields.dv_choice_multiple, ConditionOperator.In, new int[] { 0, 1 });

            var ex = XAssert.ThrowsFaultCode(ErrorCodes.InvalidArgument, () => _service.RetrieveMultiple(query));
            Assert.Equal("Condition for attribute 'dv_test.dv_choice_multiple': expected argument(s) of a different type but received 'System.Int32[]'.", ex.Message);
        }
        
        [Fact]
        public void Should_return_results_when_a_non_empty_string_array_is_used_to_filter_a_multi_option_set_early_bound_entity_record()
        {
            _context.Initialize(_testEntity);
            
            QueryExpression query = new QueryExpression(dv_test.EntityLogicalName) { TopCount = 10 };
            query.Criteria.AddCondition(dv_test.Fields.dv_choice_multiple, ConditionOperator.In, 
                new string[] { ((int)dv_test_dv_choice_multiple.Option1).ToString(),((int)dv_test_dv_choice_multiple.Option2).ToString() });

            var result = _service.RetrieveMultiple(query);
            Assert.NotEmpty(result.Entities);
        }
        
        [Fact]
        public void Should_return_invalid_cast_exception_a_non_empty_string_array_is_used_to_filter_a_multi_option_set_late_bound_entity_record()
        {
            _context.Initialize(_testLateBoundEntity);
            
            QueryExpression query = new QueryExpression(dv_test.EntityLogicalName) { TopCount = 10 };
            query.Criteria.AddCondition(dv_test.Fields.dv_choice_multiple, ConditionOperator.In, 
                new string[] { ((int)dv_test_dv_choice_multiple.Option1).ToString(),((int)dv_test_dv_choice_multiple.Option2).ToString() });

            //There is no type information to convert a string to an option set value collection and an integer value is assumed in that case
            Assert.Throws<InvalidCastException>(() => _service.RetrieveMultiple(query));
        }
    }
}