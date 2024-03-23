#if FAKE_XRM_EASY_9
using System;
using System.Linq;
using System.ServiceModel;
using Crm;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Xunit;

namespace FakeXrmEasy.Core.Tests.Query.TranslateQueryExpressionTests.OperatorTests.MultiSelectOptionSet
{
    public class MultiSelectOptionSetTests: FakeXrmEasyTestsBase
    {
        private readonly Contact _contactWithOptions12;
        private readonly Contact _contactWithOption2;
        private readonly Contact _contactWithOptions23;
        private readonly Contact _contactWithOptions123;
        private readonly Contact _contactWithNullOptions;

        public MultiSelectOptionSetTests()
        {
            _contactWithOptions12 = new Contact()
            {
                Id = Guid.NewGuid(),
                FirstName = "1,2",
                new_MultiSelectAttribute = new OptionSetValueCollection()
                    { new OptionSetValue(1), new OptionSetValue(2) }
            };

            _contactWithOption2 = new Contact()
            {
                Id = Guid.NewGuid(),
                FirstName = "2", new_MultiSelectAttribute = new OptionSetValueCollection() { new OptionSetValue(2) }
            };

            _contactWithOptions23 = new Contact()
            {
                Id = Guid.NewGuid(),
                FirstName = "2,3",
                new_MultiSelectAttribute = new OptionSetValueCollection()
                    { new OptionSetValue(2), new OptionSetValue(3) }
            };
            _contactWithOptions123 = new Contact()
            {
                Id = Guid.NewGuid(),
                FirstName = "1,2,3",
                new_MultiSelectAttribute = new OptionSetValueCollection()
                    { new OptionSetValue(1), new OptionSetValue(2), new OptionSetValue(3) }
            };
            _contactWithNullOptions = new Contact()
            {
                Id = Guid.NewGuid(),
                FirstName = "null"
            };
        }
        
        [Fact]
        public void When_executing_a_query_expression_equal_operator_returns_exact_matches_for_int_right_hand_side()
        {
            _context.Initialize(new []
            {
                _contactWithOptions12, _contactWithOption2, _contactWithOptions23, _contactWithOptions123, _contactWithNullOptions
            });

            var qe = new QueryExpression("contact");
            qe.ColumnSet = new ColumnSet(new[] { "firstname" });
            qe.Criteria.AddCondition("new_multiselectattribute", ConditionOperator.Equal, 2);

            var entities = _service.RetrieveMultiple(qe).Entities;

            Assert.Single(entities);
            Assert.Equal("2", entities[0]["firstname"]);
        }

        [Fact]
        public void When_executing_a_query_expression_equal_operator_returns_exact_matches_for_string_right_hand_side()
        {
            _context.Initialize(new []
            {
                _contactWithOptions12, _contactWithOption2, _contactWithOptions23, _contactWithOptions123, _contactWithNullOptions
            });

            var qe = new QueryExpression("contact");
            qe.ColumnSet = new ColumnSet(new[] { "firstname" });
            qe.Criteria.AddCondition("new_multiselectattribute", ConditionOperator.Equal, "2");

            var entities = _service.RetrieveMultiple(qe).Entities;

            Assert.Single(entities);
            Assert.Equal("2", entities[0]["firstname"]);
        }

        [Fact]
        public void When_executing_a_query_expression_equal_operator_throws_exception_for_optionsetvalue_right_hand_side()
        {
            _service.Create(new Contact { FirstName = "2", new_MultiSelectAttribute = new OptionSetValueCollection() { new OptionSetValue(2) } });

            var qe = new QueryExpression("contact");
            qe.Criteria.AddCondition("new_multiselectattribute", ConditionOperator.Equal, new OptionSetValue(2));

            Assert.Throws<FaultException<OrganizationServiceFault>>(() => _service.RetrieveMultiple(qe));
        }

        [Fact]
        public void When_executing_a_query_expression_equal_operator_throws_exception_for_optionsetvaluecollection_right_hand_side()
        {
            _service.Create(new Contact { FirstName = "1,2,3", new_MultiSelectAttribute = new OptionSetValueCollection() { new OptionSetValue(1), new OptionSetValue(2) } });

            var qe = new QueryExpression("contact");
            qe.Criteria.AddCondition("new_multiselectattribute", ConditionOperator.Equal, new OptionSetValueCollection() { new OptionSetValue(1), new OptionSetValue(2) });

            Assert.Throws<FaultException<OrganizationServiceFault>>(() => _service.RetrieveMultiple(qe));
        }

        [Fact]
        public void When_executing_a_query_expression_equal_operator_returns_exact_matches_for_single_int_array_right_hand_side()
        {
            _context.Initialize(new []
            {
                _contactWithOptions12, _contactWithOption2, _contactWithOptions23, _contactWithOptions123, _contactWithNullOptions
            });

            var qe = new QueryExpression("contact");
            qe.ColumnSet = new ColumnSet(new[] { "firstname" });
            qe.Criteria.AddCondition("new_multiselectattribute", ConditionOperator.Equal, new[] { 2 });

            var entities = _service.RetrieveMultiple(qe).Entities;

            Assert.Single(entities);
            Assert.Equal("2", entities[0]["firstname"]);
        }

        [Fact]
        public void When_executing_a_query_expression_equal_operator_throws_exception_for_int_array_right_hand_side()
        {
            _service.Create(new Contact { FirstName = "1,2,3", new_MultiSelectAttribute = new OptionSetValueCollection() { new OptionSetValue(1), new OptionSetValue(2), new OptionSetValue(3) } });

            var qe = new QueryExpression("contact");
            qe.Criteria.AddCondition("new_multiselectattribute", ConditionOperator.Equal, new[] { 1, 2, 3 });

            Assert.Throws<FaultException<OrganizationServiceFault>>(() => _service.RetrieveMultiple(qe));
        }

        [Fact]
        public void When_executing_a_query_expression_equal_operator_throws_exception_for_string_array_right_hand_side()
        {
            _service.Create(new Contact { FirstName = "1,2,3", new_MultiSelectAttribute = new OptionSetValueCollection() { new OptionSetValue(1), new OptionSetValue(2), new OptionSetValue(3) } });

            var qe = new QueryExpression("contact");
            qe.Criteria.AddCondition("new_multiselectattribute", ConditionOperator.Equal, new[] { "1", "2", "3" });

            Assert.Throws<FaultException<OrganizationServiceFault>>(() => _service.RetrieveMultiple(qe));
        }

        [Fact]
        public void When_executing_a_query_expression_notequal_operator_excludes_exact_matches_for_int_right_hand_side()
        {
            _context.Initialize(new []
            {
                _contactWithOptions12, _contactWithOption2, _contactWithOptions23, _contactWithOptions123, _contactWithNullOptions
            });

            var qe = new QueryExpression("contact");
            qe.ColumnSet = new ColumnSet(new[] { "firstname" });
            qe.Criteria.AddCondition("new_multiselectattribute", ConditionOperator.NotEqual, 2);

            var entities = _service.RetrieveMultiple(qe).Entities;

            Assert.Equal(4, entities.Count);
            Assert.True(entities.All(e => (e["firstname"] as string) != "2"));
        }

        [Fact]
        public void When_executing_a_query_expression_notequal_operator_excludes_exact_matches_for_single_int_array_right_hand_side()
        {
            _context.Initialize(new []
            {
                _contactWithOptions12, _contactWithOption2, _contactWithOptions23, _contactWithOptions123, _contactWithNullOptions
            });

            var qe = new QueryExpression("contact");
            qe.ColumnSet = new ColumnSet(new[] { "firstname" });
            qe.Criteria.AddCondition("new_multiselectattribute", ConditionOperator.NotEqual, new[] { 2 });

            var entities = _service.RetrieveMultiple(qe).Entities;

            Assert.Equal(4, entities.Count);
            Assert.True(entities.All(e => (e["firstname"] as string) != "2"));
        }

        [Fact]
        public void When_executing_a_query_expression_in_operator_returns_exact_matches_for_int_right_hand_side()
        {
            _context.Initialize(new []
            {
                _contactWithOptions12, _contactWithOption2, _contactWithOptions23, _contactWithOptions123, _contactWithNullOptions
            });

            var qe = new QueryExpression("contact");
            qe.ColumnSet = new ColumnSet(new[] { "firstname" });
            qe.Criteria.AddCondition("new_multiselectattribute", ConditionOperator.In, 2);

            var entities = _service.RetrieveMultiple(qe).Entities;

            Assert.Single(entities);
            Assert.Equal("2", entities[0]["firstname"]);
        }

        [Fact]
        public void When_executing_a_query_expression_in_operator_returns_exact_matches_for_string_right_hand_side()
        {
            _context.Initialize(new []
            {
                _contactWithOptions12, _contactWithOption2, _contactWithOptions23, _contactWithOptions123, _contactWithNullOptions
            });

            var qe = new QueryExpression("contact");
            qe.ColumnSet = new ColumnSet(new[] { "firstname" });
            qe.Criteria.AddCondition("new_multiselectattribute", ConditionOperator.In, "2");

            var entities = _service.RetrieveMultiple(qe).Entities;

            Assert.Single(entities);
            Assert.Equal("2", entities[0]["firstname"]);
        }

        [Fact]
        public void When_executing_a_query_expression_in_operator_throws_exception_for_optionsetvalue_right_hand_side()
        {
            _service.Create(new Contact { FirstName = "2", new_MultiSelectAttribute = new OptionSetValueCollection() { new OptionSetValue(2) } });

            var qe = new QueryExpression("contact");
            qe.Criteria.AddCondition("new_multiselectattribute", ConditionOperator.In, new OptionSetValue(2));

            Assert.Throws<FaultException<OrganizationServiceFault>>(() => _service.RetrieveMultiple(qe));
        }

        [Fact]
        public void When_executing_a_query_expression_in_operator_throws_exception_for_optionsetvaluecollection_right_hand_side()
        {
            _service.Create(new Contact { FirstName = "1,2,3", new_MultiSelectAttribute = new OptionSetValueCollection() { new OptionSetValue(1), new OptionSetValue(2) } });

            var qe = new QueryExpression("contact");
            qe.Criteria.AddCondition("new_multiselectattribute", ConditionOperator.In, new OptionSetValueCollection() { new OptionSetValue(1), new OptionSetValue(2) });

            Assert.Throws<FaultException<OrganizationServiceFault>>(() => _service.RetrieveMultiple(qe));
        }

        [Fact]
        public void When_executing_a_query_expression_in_operator_returns_exact_matches_for_single_int_array_right_hand_side()
        {
            _context.Initialize(new []
            {
                _contactWithOptions12, _contactWithOption2, _contactWithOptions23, _contactWithOptions123, _contactWithNullOptions
            });

            var qe = new QueryExpression("contact");
            qe.ColumnSet = new ColumnSet(new[] { "firstname" });
            qe.Criteria.AddCondition("new_multiselectattribute", ConditionOperator.In, new object[] { 2 });

            var entities = _service.RetrieveMultiple(qe).Entities;

            Assert.Single(entities);
            Assert.Equal("2", entities[0]["firstname"]);
        }

        [Fact]
        public void When_executing_a_query_expression_in_operator_returns_exact_matches_for_int_array_right_hand_side()
        {
            _context.Initialize(new []
            {
                _contactWithOptions12, _contactWithOption2, _contactWithOptions23, _contactWithOptions123, _contactWithNullOptions
            });

            var qe = new QueryExpression("contact");
            qe.ColumnSet = new ColumnSet(new[] { "firstname" });
            qe.Criteria.AddCondition("new_multiselectattribute", ConditionOperator.In, new object[] { 2, 3 });

            var entities = _service.RetrieveMultiple(qe).Entities;

            Assert.Single(entities);
            Assert.Equal("2,3", entities[0]["firstname"]);
        }

        [Fact]
        public void When_executing_a_query_expression_in_operator_returns_exact_matches_for_out_of_order_int_array_right_hand_side()
        {
            _context.Initialize(new []
            {
                _contactWithOptions12, _contactWithOption2, _contactWithOptions23, _contactWithOptions123, _contactWithNullOptions
            });

            var qe = new QueryExpression("contact");
            qe.ColumnSet = new ColumnSet(new[] { "firstname" });
            qe.Criteria.AddCondition("new_multiselectattribute", ConditionOperator.In, new object[] { 3, 1, 2 });

            var entities = _service.RetrieveMultiple(qe).Entities;

            Assert.Single(entities);
            Assert.Equal("1,2,3", entities[0]["firstname"]);
        }

        [Fact]
        public void When_executing_a_query_expression_in_operator_returns_exact_matches_for_out_of_order_int_params_right_hand_side()
        {
            _context.Initialize(new []
            {
                _contactWithOptions12, _contactWithOption2, _contactWithOptions23, _contactWithOptions123, _contactWithNullOptions
            });

            var qe = new QueryExpression("contact");
            qe.ColumnSet = new ColumnSet(new[] { "firstname" });
            qe.Criteria.AddCondition("new_multiselectattribute", ConditionOperator.In, 3, 1, 2);

            var entities = _service.RetrieveMultiple(qe).Entities;

            Assert.Single(entities);
            Assert.Equal("1,2,3", entities[0]["firstname"]);
        }

        [Fact]
        public void When_executing_a_query_expression_in_operator_returns_exact_matches_for_string_array_right_hand_side()
        {
            _context.Initialize(new []
            {
                _contactWithOptions12, _contactWithOption2, _contactWithOptions23, _contactWithOptions123, _contactWithNullOptions
            });

            var qe = new QueryExpression("contact");
            qe.ColumnSet = new ColumnSet(new[] { "firstname" });
            qe.Criteria.AddCondition("new_multiselectattribute", ConditionOperator.In, new[] { "2", "3" });

            var entities = _service.RetrieveMultiple(qe).Entities;

            Assert.Single(entities);
            Assert.Equal("2,3", entities[0]["firstname"]);
        }

        [Fact]
        public void When_executing_a_query_expression_in_operator_returns_exact_matches_for_string_params_right_hand_side()
        {
            _context.Initialize(new []
            {
                _contactWithOptions12, _contactWithOption2, _contactWithOptions23, _contactWithOptions123, _contactWithNullOptions
            });

            var qe = new QueryExpression("contact");
            qe.ColumnSet = new ColumnSet(new[] { "firstname" });
            qe.Criteria.AddCondition("new_multiselectattribute", ConditionOperator.In, "2", "3");

            var entities = _service.RetrieveMultiple(qe).Entities;

            Assert.Single(entities);
            Assert.Equal("2,3", entities[0]["firstname"]);
        }

        [Fact]
        public void When_executing_a_query_expression_in_operator_returns_exact_matches_for_out_of_order_string_array_right_hand_side()
        {
            _context.Initialize(new []
            {
                _contactWithOptions12, _contactWithOption2, _contactWithOptions23, _contactWithOptions123, _contactWithNullOptions
            });

            var qe = new QueryExpression("contact");
            qe.ColumnSet = new ColumnSet(new[] { "firstname" });
            qe.Criteria.AddCondition("new_multiselectattribute", ConditionOperator.In, new[] { "3", "2" });

            var entities = _service.RetrieveMultiple(qe).Entities;

            Assert.Single(entities);
            Assert.Equal("2,3", entities[0]["firstname"]);
        }

        [Fact]
        public void When_executing_a_query_expression_notin_operator_excludes_exact_matches_for_int_array_right_hand_side()
        {            
            _context.Initialize(new []
            {
                _contactWithOptions12, _contactWithOption2, _contactWithOptions23, _contactWithOptions123, _contactWithNullOptions
            });

            var qe = new QueryExpression("contact");
            qe.ColumnSet = new ColumnSet(new[] { "firstname" });
            qe.Criteria.AddCondition("new_multiselectattribute", ConditionOperator.NotIn, new object[] { 2, 3 });

            var entities = _service.RetrieveMultiple(qe).Entities;

            Assert.Equal(4, entities.Count);
            Assert.True(entities.All(e => (e["firstname"] as string) != "2,3"));
        }


        [Fact]
        public void When_executing_a_query_expression_notin_operator_excludes_exact_matches_for_out_of_order_string_params_right_hand_side()
        {
            _context.Initialize(new []
            {
                _contactWithOptions12, _contactWithOption2, _contactWithOptions23, _contactWithOptions123, _contactWithNullOptions
            });

            var qe = new QueryExpression("contact");
            qe.ColumnSet = new ColumnSet(new[] { "firstname" });
            qe.Criteria.AddCondition("new_multiselectattribute", ConditionOperator.NotIn, "3", "2");

            var entities = _service.RetrieveMultiple(qe).Entities;

            Assert.Equal(4, entities.Count);
            Assert.True(entities.All(e => (e["firstname"] as string) != "2,3"));
        }

        [Fact]
        public void When_executing_a_query_expression_containvalues_operator_returns_partial_matches_for_int_right_hand_side()
        {
            _context.Initialize(new []
            {
                _contactWithOptions12, _contactWithOption2, _contactWithOptions23, _contactWithOptions123, _contactWithNullOptions
            });

            var qe = new QueryExpression("contact");
            qe.ColumnSet = new ColumnSet(new[] { "firstname" });
            qe.Criteria.AddCondition("new_multiselectattribute", ConditionOperator.ContainValues, 1);

            var entities = _service.RetrieveMultiple(qe).Entities;

            Assert.Equal(2, entities.Count);
            Assert.True(entities.All(e => (e["firstname"] as string).Contains("1")));
        }

        [Fact]
        public void When_executing_a_query_expression_containvalues_operator_returns_partial_matches_for_string_right_hand_side()
        {
            _context.Initialize(new []
            {
                _contactWithOptions12, _contactWithOption2, _contactWithOptions23, _contactWithOptions123, _contactWithNullOptions
            });

            var qe = new QueryExpression("contact");
            qe.ColumnSet = new ColumnSet(new[] { "firstname" });
            qe.Criteria.AddCondition("new_multiselectattribute", ConditionOperator.ContainValues, "1");

            var entities = _service.RetrieveMultiple(qe).Entities;

            Assert.Equal(2, entities.Count);
            Assert.True(entities.All(e => (e["firstname"] as string).Contains("1")));
        }

        [Fact]
        public void When_executing_a_query_expression_containvalues_operator_throws_exception_for_optionsetvalue_right_hand_side()
        {
            _service.Create(new Contact { FirstName = "2", new_MultiSelectAttribute = new OptionSetValueCollection() { new OptionSetValue(2) } });

            var qe = new QueryExpression("contact");
            qe.Criteria.AddCondition("new_multiselectattribute", ConditionOperator.ContainValues, new OptionSetValue(2));

            Assert.Throws<FaultException<OrganizationServiceFault>>(() => _service.RetrieveMultiple(qe));
        }

        [Fact]
        public void When_executing_a_query_expression_containvalues_operator_throws_exception_for_optionsetvaluecollection_right_hand_side()
        {
            _service.Create(new Contact { FirstName = "1,2,3", new_MultiSelectAttribute = new OptionSetValueCollection() { new OptionSetValue(1), new OptionSetValue(2) } });

            var qe = new QueryExpression("contact");
            qe.Criteria.AddCondition("new_multiselectattribute", ConditionOperator.ContainValues, new OptionSetValueCollection() { new OptionSetValue(1), new OptionSetValue(2) });

            Assert.Throws<FaultException<OrganizationServiceFault>>(() => _service.RetrieveMultiple(qe));
        }

        [Fact]
        public void When_executing_a_query_expression_containvalues_operator_returns_partial_matches_for_single_int_array_right_hand_side()
        {
            _context.Initialize(new []
            {
                _contactWithOptions12, _contactWithOption2, _contactWithOptions23, _contactWithOptions123, _contactWithNullOptions
            });

            var qe = new QueryExpression("contact");
            qe.ColumnSet = new ColumnSet(new[] { "firstname" });
            qe.Criteria.AddCondition("new_multiselectattribute", ConditionOperator.ContainValues, new[] { 1 });

            var entities = _service.RetrieveMultiple(qe).Entities;

            Assert.Equal(2, entities.Count);
            Assert.True(entities.All(e => (e["firstname"] as string).Contains("1")));
        }

        [Fact]
        public void When_executing_a_query_expression_containvalues_operator_returns_partial_matches_for_int_array_right_hand_side()
        {
            _context.Initialize(new []
            {
                _contactWithOptions12, _contactWithOption2, _contactWithOptions23, _contactWithOptions123, _contactWithNullOptions
            });

            var qe = new QueryExpression("contact");
            qe.ColumnSet = new ColumnSet(new[] { "firstname" });
            qe.Criteria.AddCondition("new_multiselectattribute", ConditionOperator.ContainValues, new[] { 1, 3 });

            var entities = _service.RetrieveMultiple(qe).Entities;

            Assert.Equal(3, entities.Count);
            Assert.True(entities.All(e => (e["firstname"] as string).Contains("1") || (e["firstname"] as string).Contains("3")));
        }

        [Fact]
        public void When_executing_a_query_expression_containvalues_operator_returns_partial_matches_for_int_params_right_hand_side()
        {
            _context.Initialize(new []
            {
                _contactWithOptions12, _contactWithOption2, _contactWithOptions23, _contactWithOptions123, _contactWithNullOptions
            });

            var qe = new QueryExpression("contact");
            qe.ColumnSet = new ColumnSet(new[] { "firstname" });
            qe.Criteria.AddCondition("new_multiselectattribute", ConditionOperator.ContainValues, 1, 3);

            var entities = _service.RetrieveMultiple(qe).Entities;

            Assert.Equal(3, entities.Count);
            Assert.True(entities.All(e => (e["firstname"] as string).Contains("1") || (e["firstname"] as string).Contains("3")));
        }

        [Fact]
        public void When_executing_a_query_expression_containvalues_operator_returns_partial_matches_for_out_of_order_int_array_right_hand_side()
        {
            _context.Initialize(new []
            {
                _contactWithOptions12, _contactWithOption2, _contactWithOptions23, _contactWithOptions123, _contactWithNullOptions
            });

            var qe = new QueryExpression("contact");
            qe.ColumnSet = new ColumnSet(new[] { "firstname" });
            qe.Criteria.AddCondition("new_multiselectattribute", ConditionOperator.ContainValues, new[] { 3, 2 });

            var entities = _service.RetrieveMultiple(qe).Entities;

            Assert.Equal(4, entities.Count);
            Assert.True(entities.All(e => (e["firstname"] as string).Contains("3") || (e["firstname"] as string).Contains("2")));
        }

        [Fact]
        public void When_executing_a_query_expression_containvalues_operator_returns_partial_matches_for_string_array_right_hand_side()
        {
            _context.Initialize(new []
            {
                _contactWithOptions12, _contactWithOption2, _contactWithOptions23, _contactWithOptions123, _contactWithNullOptions
            });

            var qe = new QueryExpression("contact");
            qe.ColumnSet = new ColumnSet(new[] { "firstname" });
            qe.Criteria.AddCondition("new_multiselectattribute", ConditionOperator.ContainValues, new[] { "1", "3" });

            var entities = _service.RetrieveMultiple(qe).Entities;

            Assert.Equal(3, entities.Count);
            Assert.True(entities.All(e => (e["firstname"] as string).Contains("1") || (e["firstname"] as string).Contains("3")));
        }

        [Fact]
        public void When_executing_a_query_expression_containvalues_operator_returns_partial_matches_for_out_of_order_string_array_right_hand_side()
        {
            _context.Initialize(new []
            {
                _contactWithOptions12, _contactWithOption2, _contactWithOptions23, _contactWithOptions123, _contactWithNullOptions
            });

            var qe = new QueryExpression("contact");
            qe.ColumnSet = new ColumnSet(new[] { "firstname" });
            qe.Criteria.AddCondition("new_multiselectattribute", ConditionOperator.ContainValues, new[] { "3", "1" });

            var entities = _service.RetrieveMultiple(qe).Entities;

            Assert.Equal(3, entities.Count);
            Assert.True(entities.All(e => (e["firstname"] as string).Contains("3") || (e["firstname"] as string).Contains("1")));
        }

        [Fact]
        public void When_executing_a_query_expression_doesnotcontainvalues_operator_excludes_partial_matches_for_int_array_right_hand_side()
        {
            _context.Initialize(new []
            {
                _contactWithOptions12, _contactWithOption2, _contactWithOptions23, _contactWithOptions123, _contactWithNullOptions
            });

            var qe = new QueryExpression("contact");
            qe.ColumnSet = new ColumnSet(new[] { "firstname" });
            qe.Criteria.AddCondition("new_multiselectattribute", ConditionOperator.DoesNotContainValues, new[] { 2, 3 });

            var entities = _service.RetrieveMultiple(qe).Entities;

            Assert.Single(entities);
            Assert.Equal("null", entities[0]["firstname"]);
        }

        [Fact]
        public void When_executing_a_query_expression_doesnotcontainvalues_operator_excludes_partial_matches_for_out_of_order_string_params_right_hand_side()
        {
            _context.Initialize(new []
            {
                _contactWithOptions12, _contactWithOption2, _contactWithOptions23, _contactWithOptions123, _contactWithNullOptions
            });

            var qe = new QueryExpression("contact");
            qe.ColumnSet = new ColumnSet(new[] { "firstname" });
            qe.Criteria.AddCondition("new_multiselectattribute", ConditionOperator.DoesNotContainValues, "3", "1");

            var entities = _service.RetrieveMultiple(qe).Entities;

            Assert.Equal(2, entities.Count);
            Assert.True(entities.All(e => !(e["firstname"] as string).Contains("3") && !(e["firstname"] as string).Contains("1")));
        }
    }
}
#endif