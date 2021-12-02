using FakeXrmEasy.Extensions;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Metadata;
using System;
using Xunit;
using FakeXrmEasy.Abstractions;

namespace FakeXrmEasy.Tests.FakeContextTests
{
    public class XrmFakedRelationshipTests: FakeXrmEasyTestsBase
    {
        [Fact]
        public void When_creating_relationship_with_first_constructor_properties_are_set_correctly()
        {
            var rel = new XrmFakedRelationship("entity1Attribute", "entity2Attribute", "entity1LogicalName", "entity2LogicalName");

            Assert.Equal("entity1LogicalName", rel.Entity1LogicalName);
            Assert.Equal("entity2LogicalName", rel.Entity2LogicalName);
            Assert.Equal("entity1Attribute", rel.Entity1Attribute);
            Assert.Equal("entity2Attribute", rel.Entity2Attribute);
            Assert.Equal(XrmFakedRelationship.FakeRelationshipType.OneToMany, rel.RelationshipType);
        }

        [Fact]
        public void When_creating_relationship_with_second_constructor_properties_are_set_correctly()
        {
            var rel = new XrmFakedRelationship("intersectName", "entity1Attribute", "entity2Attribute", "entity1LogicalName", "entity2LogicalName");

            Assert.Equal("entity1LogicalName", rel.Entity1LogicalName);
            Assert.Equal("entity2LogicalName", rel.Entity2LogicalName);
            Assert.Equal("entity1Attribute", rel.Entity1Attribute);
            Assert.Equal("entity2Attribute", rel.Entity2Attribute);
            Assert.Equal("intersectName", rel.IntersectEntity);
            Assert.Equal(XrmFakedRelationship.FakeRelationshipType.ManyToMany, rel.RelationshipType);
        }

        [Fact]
        public void Self_referential_relationships_can_be_created()
        {
            var exampleMetadata = new EntityMetadata()
            {
                LogicalName = "test_entity"
            };

            var nameAttribute = new StringAttributeMetadata()
            {
                LogicalName = "name",
                RequiredLevel = new AttributeRequiredLevelManagedProperty(AttributeRequiredLevel.ApplicationRequired)
            };

            var idAttribute = new AttributeMetadata()
            {
                LogicalName = "test_entityid",
                RequiredLevel = new AttributeRequiredLevelManagedProperty(AttributeRequiredLevel.ApplicationRequired),
            };
            exampleMetadata.SetAttributeCollection(new AttributeMetadata[] { idAttribute, nameAttribute });

            _context.InitializeMetadata(new[] { exampleMetadata });

            _context.AddRelationship("test_entity_entity", new XrmFakedRelationship
            {
                IntersectEntity = "test_entity_entity",
                Entity1LogicalName = exampleMetadata.LogicalName,
                Entity1Attribute = idAttribute.LogicalName,
                Entity2LogicalName = exampleMetadata.LogicalName,
                Entity2Attribute = idAttribute.LogicalName,
                RelationshipType = XrmFakedRelationship.FakeRelationshipType.ManyToMany
            });

            var record1 = new Entity(exampleMetadata.LogicalName)
            {
                Id = Guid.NewGuid(),
                [nameAttribute.LogicalName] = "First Record"
            };

            var record2 = new Entity(exampleMetadata.LogicalName)
            {
                Id = Guid.NewGuid(),
                [nameAttribute.LogicalName] = "Second Record"
            };

            _context.Initialize(new[] { record1, record2 });


            

            var relationship = new Relationship("test_entity_entity");

            var ex = Record.Exception(() => _service.Associate(
                exampleMetadata.LogicalName,
                record1.Id,
                relationship,
                new EntityReferenceCollection(
                    new[] { record2.ToEntityReference() }
                    )
                ));

            Assert.Null(ex);
        }

        [Fact]
        public void Relationships_between_two_different_entities_can_be_created()
        {
            var exampleMetadata = new EntityMetadata()
            {
                LogicalName = "test_entity",
            };

            var nameAttribute = new StringAttributeMetadata()
            {
                LogicalName = "name",
                RequiredLevel = new AttributeRequiredLevelManagedProperty(AttributeRequiredLevel.ApplicationRequired)
            };

            var idAttribute = new AttributeMetadata()
            {
                LogicalName = "test_entityid",
                RequiredLevel = new AttributeRequiredLevelManagedProperty(AttributeRequiredLevel.ApplicationRequired),
            };
            exampleMetadata.SetAttributeCollection(new AttributeMetadata[] { idAttribute, nameAttribute });


            var otherMetadata = new EntityMetadata()
            {
                LogicalName = "test_other",
            };

            var otherNameAttribute = new StringAttributeMetadata()
            {
                LogicalName = "name",
                RequiredLevel = new AttributeRequiredLevelManagedProperty(AttributeRequiredLevel.ApplicationRequired)
            };

            var otherIdAttribute = new AttributeMetadata()
            {
                LogicalName = "test_otherid",
                RequiredLevel = new AttributeRequiredLevelManagedProperty(AttributeRequiredLevel.ApplicationRequired),
            };
            otherMetadata.SetAttributeCollection(new AttributeMetadata[] { otherIdAttribute, otherNameAttribute });

            _context.InitializeMetadata(new[] { exampleMetadata });

            _context.AddRelationship("test_entity_other", new XrmFakedRelationship
            {
                IntersectEntity = "test_entity_other",
                Entity1LogicalName = exampleMetadata.LogicalName,
                Entity1Attribute = idAttribute.LogicalName,
                Entity2LogicalName = otherMetadata.LogicalName,
                Entity2Attribute = otherIdAttribute.LogicalName,
                RelationshipType = XrmFakedRelationship.FakeRelationshipType.ManyToMany
            });

            var record1 = new Entity(exampleMetadata.LogicalName)
            {
                Id = Guid.NewGuid(),
                [nameAttribute.LogicalName] = "First Record"
            };

            var record2 = new Entity(otherMetadata.LogicalName)
            {
                Id = Guid.NewGuid(),
                [nameAttribute.LogicalName] = "Second Record"
            };

            _context.Initialize(new[] { record1, record2 });

            var relationship = new Relationship("test_entity_other");

            var ex = Record.Exception(() => _service.Associate(
                   exampleMetadata.LogicalName,
                   record1.Id,
                   relationship,
                   new EntityReferenceCollection(
                       new[] { record2.ToEntityReference() }
                       )
                   ));

            Assert.Null(ex);
        }
    }
}