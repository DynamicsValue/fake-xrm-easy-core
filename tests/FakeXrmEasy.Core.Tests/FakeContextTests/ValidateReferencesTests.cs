using FakeXrmEasy.Abstractions;
using FakeXrmEasy.Abstractions.Integrity;
using FakeXrmEasy.Integrity;
using FakeXrmEasy.Middleware;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.ServiceModel;
using Xunit;
using FakeXrmEasy.Abstractions.Enums;
using Crm;
using FakeXrmEasy.Extensions;
using System.Collections.Generic;

#if !FAKE_XRM_EASY && !FAKE_XRM_EASY_2013 && !FAKE_XRM_EASY_2015
using Microsoft.Xrm.Sdk.Metadata;
#endif

namespace FakeXrmEasy.Core.Tests.FakeContextTests
{
    public class ValidateReferencesTests: FakeXrmEasyTestsBase
    {
        protected readonly IXrmFakedContext _contextWithIntegrity;
        protected readonly IOrganizationService _serviceWithIntegrity;
        
        public ValidateReferencesTests(): base()
        {
            _contextWithIntegrity = XrmFakedContextFactory.New(FakeXrmEasyLicense.RPL_1_5, new IntegrityOptions());
            _serviceWithIntegrity = _contextWithIntegrity.GetOrganizationService();
        }

        [Fact]
        public void When_context_is_initialised_validate_references_is_disabled_by_default()
        {
            var integrityOptions = _context.GetProperty<IIntegrityOptions>();
            Assert.False(integrityOptions.ValidateEntityReferences);
        }

        [Fact]
        public void An_entity_which_references_another_non_existent_entity_can_be_created_when_integrity_is_disabled()
        {
            Guid otherEntity = Guid.NewGuid();
            Entity entity = new Entity("entity");

            entity["otherEntity"] = new EntityReference("entity", otherEntity);

            Guid created = _service.Create(entity);

            var ex = Assert.Throws<FaultException<OrganizationServiceFault>>(() => _service.Retrieve("entity", otherEntity, new ColumnSet(true)));

            Assert.NotEqual(Guid.Empty, created);
            Assert.Equal($"{entity.LogicalName} With Id = {otherEntity:D} Does Not Exist", ex.Message);
        }

        [Fact]
        public void An_entity_which_references_another_non_existent_entity_can_not_be_created_when_validate_is_true()
        {
            Guid otherEntity = Guid.NewGuid();
            Entity entity = new Entity("entity");

            entity["otherEntity"] = new EntityReference("entity", otherEntity);

            var ex = Assert.Throws<FaultException<OrganizationServiceFault>>(() => _serviceWithIntegrity.Create(entity));

            Assert.Equal($"{entity.LogicalName} With Ids = {otherEntity:D} Do Not Exist", ex.Message);
        }

        [Fact]
        public void An_entity_which_references_another_existent_entity_can_be_created_when_integrity_is_enabled()
        {
            Entity otherEntity = new Entity("otherEntity");
            otherEntity.Id = Guid.NewGuid();
            _contextWithIntegrity.Initialize(otherEntity);

            Entity entity = new Entity("entity");
            entity["otherEntity"] = otherEntity.ToEntityReference();

            Guid created = _serviceWithIntegrity.Create(entity);

            Entity otherEntityInContext = _serviceWithIntegrity.Retrieve("otherEntity", otherEntity.Id, new ColumnSet(true));

            Assert.NotEqual(Guid.Empty, created);
            Assert.Equal(otherEntity.Id, otherEntityInContext.Id);
        }

        

        [Fact]
        public void An_entity_which_references_another_non_existent_entity_can_be_updated_when_integrity_is_disabled()
        {
            Entity entity = new Entity("entity");
            entity.Id = Guid.NewGuid();
            _context.Initialize(entity);

            Guid otherEntityId = Guid.NewGuid();
            entity["otherEntity"] = new EntityReference("entity", otherEntityId);

            _service.Update(entity);

            Entity updated = _service.Retrieve(entity.LogicalName, entity.Id, new ColumnSet(true));
            var ex = Assert.Throws<FaultException<OrganizationServiceFault>>(() => _service.Retrieve("entity", otherEntityId, new ColumnSet(true)));
            Assert.Equal(otherEntityId, updated.GetAttributeValue<EntityReference>("otherEntity").Id);
            Assert.Equal($"{entity.LogicalName} With Id = {otherEntityId:D} Does Not Exist", ex.Message);
        }

        [Fact]
        public void An_entity_which_references_another_non_existent_entity_can_not_be_updated_when_integrity_is_enabled()
        {
            Entity entity = new Entity("entity");
            entity.Id = Guid.NewGuid();
            _contextWithIntegrity.Initialize(entity);

            Guid otherEntityId = Guid.NewGuid();
            entity["otherEntity"] = new EntityReference("entity", otherEntityId);

            var ex = Assert.Throws<FaultException<OrganizationServiceFault>>(() => _serviceWithIntegrity.Update(entity));
            Assert.Equal($"{entity.LogicalName} With Ids = {otherEntityId:D} Do Not Exist", ex.Message);
        }

        [Fact]
        public void An_entity_which_references_another_existent_entity_can_be_updated_when_integrity_is_enabled()
        {
            
            Entity otherEntity = new Entity("otherEntity");
            otherEntity.Id = Guid.NewGuid();

            Entity entity = new Entity("entity");
            entity.Id = Guid.NewGuid();

            _contextWithIntegrity.Initialize(new Entity[] { otherEntity, entity });
            entity["otherEntity"] = otherEntity.ToEntityReference();

            _serviceWithIntegrity.Update(entity);

            Entity otherEntityInContext = _serviceWithIntegrity.Retrieve("otherEntity", otherEntity.Id, new ColumnSet(true));
            Entity updated = _serviceWithIntegrity.Retrieve(entity.LogicalName, entity.Id, new ColumnSet(true));

            Assert.Equal(otherEntity.Id, updated.GetAttributeValue<EntityReference>("otherEntity").Id);
            Assert.Equal(otherEntity.Id, otherEntityInContext.Id);
        }

        [Fact]
        public void Should_raise_exception_when_validating_a_null_entity()
        {
            Entity entity = new Entity("entity");

            Assert.Throws<InvalidOperationException>(() => (_contextWithIntegrity as XrmFakedContext).ValidateEntity(null));
        }
    }
}
