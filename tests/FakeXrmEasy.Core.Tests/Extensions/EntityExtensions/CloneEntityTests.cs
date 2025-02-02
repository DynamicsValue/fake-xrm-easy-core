using System;
using System.Collections.Generic;
using Crm;
using FakeXrmEasy.Extensions;
using Microsoft.Xrm.Sdk;
using Xunit;

namespace FakeXrmEasy.Core.Tests.Extensions
{
    public class CloneEntityTests: FakeXrmEasyTestsBase
    {
        private readonly Entity _earlyBoundSource;
        private readonly Entity _lateBoundSource;
        private Entity _clone;
        
        public CloneEntityTests()
        {
            _earlyBoundSource = new Account() { Id = Guid.NewGuid() };
            _lateBoundSource = new Entity("account") { Id = Guid.NewGuid() };
        }

        [Fact]
        public void Should_clone_main_late_bound_object_properties()
        {
            var clone = _lateBoundSource.Clone(_context);
            
            ///Different object reference
            Assert.NotEqual(clone, _lateBoundSource);
            
            Assert.Equal(_lateBoundSource.LogicalName, clone.LogicalName);
            Assert.Equal(_lateBoundSource.Id, clone.Id);
        }
        
        [Fact]
        public void Should_clone_main_early_bound_object_properties_and_keep_original_type()
        {
            var clone = _earlyBoundSource.Clone(_earlyBoundSource.GetType(), _context);
            
            ///Different object reference
            Assert.NotEqual(clone, _earlyBoundSource);
            
            Assert.Equal(_earlyBoundSource.LogicalName, clone.LogicalName);
            Assert.Equal(_earlyBoundSource.Id, clone.Id);

            Assert.Equal(_earlyBoundSource.GetType(), clone.GetType());
        }

        #if FAKE_XRM_EASY_9
        [Fact]
        public void Should_clone_related_entities()
        {
            var relatedEntity1 = new Contact() { Id = Guid.NewGuid() };
            var relatedEntity2 = new Contact() { Id = Guid.NewGuid() };
            
            var relationShip = new Relationship("account_contacts");
            
            _earlyBoundSource.RelatedEntities = new RelatedEntityCollection()
            {
                new KeyValuePair<Relationship, EntityCollection>(
                    relationShip, 
                    new EntityCollection(new List<Entity>()
                {
                    relatedEntity1, relatedEntity2
                }))
            };
            
            var clone = _earlyBoundSource.Clone(_earlyBoundSource.GetType(), _context);
            Assert.NotEqual(clone, _earlyBoundSource);

            Assert.Single(clone.RelatedEntities);
            
            var clonedRelatedEntities = clone.RelatedEntities;
            Assert.True(clonedRelatedEntities.ContainsKey(relationShip));

            var clonedEntityCollection = clonedRelatedEntities[relationShip];
            Assert.Equal(2, clonedEntityCollection.Entities.Count);

            var clonedRelatedEntity1 = clonedEntityCollection.Entities[0];
            var clonedRelatedEntity2 = clonedEntityCollection.Entities[1];
            
            Assert.NotEqual(relatedEntity1, clonedRelatedEntity1);
            Assert.Equal(relatedEntity1.Id, clonedRelatedEntity1.Id);
            Assert.Equal(relatedEntity1.LogicalName, clonedRelatedEntity1.LogicalName);
            
            Assert.NotEqual(relatedEntity2, clonedRelatedEntity2);
            Assert.Equal(relatedEntity2.Id, clonedRelatedEntity2.Id);
            Assert.Equal(relatedEntity2.LogicalName, clonedRelatedEntity2.LogicalName);
        }
        #endif
        
    }
}