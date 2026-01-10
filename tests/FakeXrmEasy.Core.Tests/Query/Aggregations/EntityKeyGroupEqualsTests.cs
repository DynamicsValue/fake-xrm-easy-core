using System.Collections.Generic;
using FakeXrmEasy.Core.Query.Aggregations;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Xunit;

namespace FakeXrmEasy.Core.Tests.Query.Aggregations
{
    public class EntityKeyGroupEqualsTests
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
                    DateTimeGrouping = XrmDateTimeGrouping.Year
                }
            };
        }
        
        [Fact]
        public void Should_return_false_when_other_object_is_not_an_entity_group_key()
        {
            var entityGroupKey = new EntityGroupKey(_entity, _attributeExpressions);
            
            Assert.False(entityGroupKey.Equals(_entity));
        }
        
        [Fact]
        public void Should_return_false_when_other_object_doesnt_have_the_same_attribute_keys()
        {
            _entity["name"] = "test";
            _otherEntity["othername"] = "test";
            
            var entityGroupKey = new EntityGroupKey(_entity, _attributeExpressions);
            var otherEntityGroupKey = new EntityGroupKey(_otherEntity, _attributeExpressions);
            
            Assert.False(entityGroupKey.Equals(otherEntityGroupKey));
        }
        
        [Fact]
        public void Should_only_project_attributes_present_in_group_by_attribute_expression()
        {
            _entity["name"] = "test";
            _entity["othernamenotgrouped"] = "test";
            
            var entityGroupKey = new EntityGroupKey(_entity, _attributeExpressions);
            
            Assert.True(entityGroupKey._attributes.ContainsKey("nameAlias"));
            Assert.True(entityGroupKey._attributes.ContainsKey("descAlias"));
            Assert.True(entityGroupKey._attributes.ContainsKey("createdOnAlias"));
            Assert.False(entityGroupKey._attributes.ContainsKey("othernamenotgrouped"));
            
            Assert.Equal("test", entityGroupKey._attributes["nameAlias"]);
            Assert.Null(entityGroupKey._attributes["descAlias"]);
            Assert.Null(entityGroupKey._attributes["createdOnAlias"]);
        }
        
        [Theory]
        [InlineData(null, "notnull")]
        [InlineData("notnull", null)]
        [InlineData("someValue","otherValue")]
        public void Should_return_false_when_other_object_doesnt_have_the_same_values(string value1, string value2)
        {
            _entity["name"] = value1;
            _otherEntity["name"] = value2;
            
            var entityGroupKey = new EntityGroupKey(_entity, _attributeExpressions);
            var otherEntityGroupKey = new EntityGroupKey(_otherEntity, _attributeExpressions);
            
            Assert.False(entityGroupKey.Equals(otherEntityGroupKey));
        }
        
        [Fact]
        public void Should_return_true_when_other_object_has_the_same_attribute_keys_and_values()
        {
            _entity["name"] = "test";
            _entity["desc"] = "desc";
            
            _otherEntity["name"] = "test";
            _otherEntity["desc"] = "desc";
            
            var entityGroupKey = new EntityGroupKey(_entity, _attributeExpressions);
            var otherEntityGroupKey = new EntityGroupKey(_otherEntity, _attributeExpressions);
            
            Assert.True(entityGroupKey.Equals(otherEntityGroupKey));
        }
    }
}