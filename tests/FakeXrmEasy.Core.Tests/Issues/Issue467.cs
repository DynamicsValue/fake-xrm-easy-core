#if FAKE_XRM_EASY_9 
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using Xunit;

namespace FakeXrmEasy.Core.Tests.Issues
{
    // https://github.com/jordimontana82/fake-xrm-easy/issues/467

    public class Issue467 : FakeXrmEasyTestsBase
    {
        Entity _product;
        // setup

        public Issue467()
        {
            _product = new Entity("product", Guid.NewGuid());
            var productCategories = new OptionSetValueCollection
            {
                new OptionSetValue(121720003),
                new OptionSetValue(121720002)
            };
            _product["name"] = "Test product";
            _product["abc_productcategories"] = productCategories;

            _context.Initialize(new List<Entity> { _product });
        }

        // This test currently fails - Object reference not set to an instance of an object
        [Fact]
        public void When_A_Query_Expression_Has_A_ContainsValues_Clause_On_A_MultiSelectOptionSet_It_Should_Not_Fail()
        {
            var query = new QueryExpression("product");
            query.ColumnSet = new ColumnSet(new[] { "name" });
            query.Criteria.AddCondition("abc_productcategories", ConditionOperator.ContainValues, 121720003, 121720002);            

            EntityCollection products = _service.RetrieveMultiple(query);
            Assert.Equal(_product.Id, products.Entities[0].Id);
        }

        // This test currently fails - Object reference not set to an instance of an object
        [Fact]
        public void When_A_Query_Expression_Has_A_ContainsValues_Array_Clause_On_A_MultiSelectOptionSet_It_Should_Not_Fail()
        {
            var query = new QueryExpression("product");
            query.Criteria.AddCondition("abc_productcategories", ConditionOperator.ContainValues, new[] { 121720003, 121720002 });

            EntityCollection products = _service.RetrieveMultiple(query);
            Assert.Equal(_product.Id, products.Entities[0].Id);
        }

        // This test currently fails - Object reference not set to an instance of an object
        [Fact]
        public void When_A_Query_Expression_Has_A_DoesNotContainValues_Clause_On_A_MultiSelectOptionSet_It_Should_Not_Fail()
        {
            var query = new QueryExpression("product");
            query.Criteria.AddCondition("abc_productcategories", ConditionOperator.DoesNotContainValues, 121720003, 121720002);

            EntityCollection products = _service.RetrieveMultiple(query);
            Assert.Empty(products.Entities);
        }

        // This test should also pass, but fails with this error
        // When using arithmetic values in Fetch a ProxyTypesAssembly must be used in order to know which types to cast values to.
        // This is a requirement of FXE, but I don't think any of the Early bound classes have multi-select option set attributes so difficult to test in the core project...
        // 
        [Fact]
        public void When_A_FetchXml_Query_Has_A_Contains_Clause_On_A_MultiSelectOptionSet_It_Should_Not_Fail()
        {
            string fetchXml = @"<fetch top='50'>
                                    <entity name='product'>
                                    <filter>
                                        <condition attribute='abc_productcategories' operator='contain-values'>
                                        <value>121720003</value>
                                        <value>121720002</value>
                                        </condition>
                                    </filter>
                                    </entity>
                                </fetch>";

            EntityCollection products = _service.RetrieveMultiple(new FetchExpression(fetchXml));
            Assert.Equal(_product.Id, products.Entities[0].Id);
        }

    }
}
#endif