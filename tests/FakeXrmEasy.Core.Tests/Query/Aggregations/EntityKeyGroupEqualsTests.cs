#if FAKE_XRM_EASY_9
using System;
using System.Collections.Generic;
using FakeXrmEasy.Core.Query.Aggregations;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Xunit;

namespace FakeXrmEasy.Core.Tests.Query.Aggregations
{
    public class EntityKeyGroupEqualsTests: FakeXrmEasyTestsBase
    {
        private readonly Entity _entity;
        private readonly Entity _otherEntity;
        private readonly List<XrmAttributeExpression> _attributeExpressions;
        
        public EntityKeyGroupEqualsTests()
        {
            _entity = new Entity();
            _otherEntity = new Entity();
            _attributeExpressions = new List<XrmAttributeExpression>()
            {
                new XrmAttributeExpression()
                {
                    AttributeName = "name",
                    Alias = "nameAlias",
                    HasGroupBy = true
                },
                new XrmAttributeExpression()
                {
                    AttributeName = "desc",
                    Alias = "descAlias",
                    HasGroupBy = true
                },
                new XrmAttributeExpression()
                {
                    AttributeName = "createdon",
                    Alias = "createdOnAlias",
                    HasGroupBy = true,
                }
            };
        }
        
        [Fact]
        public void Should_return_false_when_other_object_is_not_an_entity_group_key()
        {
            var entityGroupKey = new EntityGroupKey(_context, _entity, _attributeExpressions);
            
            Assert.False(entityGroupKey.Equals(_entity));
        }
        
        [Fact]
        public void Should_return_false_when_other_object_doesnt_have_the_same_attribute_keys()
        {
            _entity["name"] = "test";
            _otherEntity["othername"] = "test";
            
            var entityGroupKey = new EntityGroupKey(_context, _entity, _attributeExpressions);
            var otherEntityGroupKey = new EntityGroupKey(_context, _otherEntity, _attributeExpressions);
            
            Assert.False(entityGroupKey.Equals(otherEntityGroupKey));
        }
        
        [Fact]
        public void Should_only_project_attributes_present_in_group_by_attribute_expression_that_are_not_null()
        {
            _entity["name"] = "test";
            _entity["createdon"] = DateTime.UtcNow;
            _entity["desc"] = null;
            _entity["othernamenotgrouped"] = "test";
            
            var entityGroupKey = new EntityGroupKey(_context, _entity, _attributeExpressions);
            
            Assert.True(entityGroupKey._attributes.ContainsKey("nameAlias"));
            Assert.True(entityGroupKey._attributes.ContainsKey("createdOnAlias"));
            Assert.False(entityGroupKey._attributes.ContainsKey("descAlias"));
            Assert.False(entityGroupKey._attributes.ContainsKey("othernamenotgrouped"));

            var nameAlias = (AliasedValue)entityGroupKey._attributes["nameAlias"];
            var createdOnAlias = (AliasedValue)entityGroupKey._attributes["createdOnAlias"];
            Assert.Equal("test", nameAlias.Value);
            Assert.Equal(_entity["createdon"], createdOnAlias.Value);
        }
        
        [Theory]
        [InlineData(null, "notnull")]
        [InlineData("notnull", null)]
        [InlineData("someValue","otherValue")]
        public void Should_return_false_when_other_object_doesnt_have_the_same_values(string value1, string value2)
        {
            _entity["name"] = value1;
            _otherEntity["name"] = value2;
            
            var entityGroupKey = new EntityGroupKey(_context, _entity, _attributeExpressions);
            var otherEntityGroupKey = new EntityGroupKey(_context, _otherEntity, _attributeExpressions);
            
            Assert.False(entityGroupKey.Equals(otherEntityGroupKey));
        }
        
        [Fact]
        public void Should_return_true_when_other_object_has_the_same_attribute_keys_and_values()
        {
            _entity["name"] = "test";
            _entity["desc"] = "desc";
            
            _otherEntity["name"] = "test";
            _otherEntity["desc"] = "desc";
            
            var entityGroupKey = new EntityGroupKey(_context, _entity, _attributeExpressions);
            var otherEntityGroupKey = new EntityGroupKey(_context, _otherEntity, _attributeExpressions);
            
            Assert.True(entityGroupKey.Equals(otherEntityGroupKey));
        }
    }
}
#endif