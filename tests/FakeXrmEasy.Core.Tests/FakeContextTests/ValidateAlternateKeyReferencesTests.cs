#if !FAKE_XRM_EASY && !FAKE_XRM_EASY_2013 && !FAKE_XRM_EASY_2015

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
using FakeXrmEasy.Extensions;
using System.Collections.Generic;
using DataverseEntities;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Account = Crm.Account;


namespace FakeXrmEasy.Core.Tests.FakeContextTests
{
    public class ValidateAlternateKeyReferencesTests: FakeXrmEasyTestsBase
    {
        private readonly IXrmFakedContext _contextWithIntegrity;
        private readonly IOrganizationService _serviceWithIntegrity;

        private readonly EntityMetadata _accountMetadata;
        private readonly Entity _account;
        private readonly Entity _account2;
        
        public ValidateAlternateKeyReferencesTests(): base()
        {
            _contextWithIntegrity = XrmFakedContextFactory.New(FakeXrmEasyLicense.RPL_1_5, new IntegrityOptions());
            _serviceWithIntegrity = _contextWithIntegrity.GetOrganizationService();
            
            _accountMetadata = new EntityMetadata()
            {
                LogicalName = Account.EntityLogicalName
            };
            var alternateKeyMetadata = new EntityKeyMetadata()
            {
                KeyAttributes = new string[] { "alternateKey" }
            };
            _accountMetadata.SetFieldValue("_keys", new EntityKeyMetadata[]
            {
                alternateKeyMetadata
            });
            
            _account = new Entity(Account.EntityLogicalName)
            {
                Id = Guid.NewGuid(),
                ["alternateKey"] = "keyValue"
            };
            
            _account2 = new Entity(Account.EntityLogicalName)
            {
                Id = Guid.NewGuid(),
                ["alternateKey"] = "keyValue2"
            };
        }
        [Fact]
        public void An_entity_which_references_another_existent_entity_by_alternate_key_can_be_created_when_integrity_is_enabled()
        {
            _contextWithIntegrity.InitializeMetadata(_accountMetadata);
            _contextWithIntegrity.Initialize(new List<Entity>() { _account });

            Entity otherEntity = new Entity("otherEntity");
            otherEntity.Id = Guid.NewGuid();
            otherEntity["new_accountId"] = new EntityReference("account", "alternateKey","keyValue") ;
            Guid created = _serviceWithIntegrity.Create(otherEntity);

            Entity otherEntityInContext = _serviceWithIntegrity.Retrieve("otherEntity", otherEntity.Id, new ColumnSet(true));

            Assert.NotEqual(Guid.Empty, created);
            Assert.Equal(((EntityReference)otherEntityInContext["new_accountId"]).Id, _account.Id);
        }
        
        [Fact]
        public void Should_throw_exception_if_create_is_called_with_a_non_existing_alternate_key()
        {
            var dummy_attribute_name = "dv_dummy_attribute";

            var nonExistingGuid = Guid.NewGuid();

            var entity = new dv_test()
            {
                dv_accountid = new EntityReference(Account.EntityLogicalName, dummy_attribute_name, "Microsoft")
            };
            
            var request = new CreateRequest()
            {
                Target = entity
            };

            var ex = XAssert.ThrowsFaultCode(ErrorCodes.InvalidEntityKeyOperation, () => _serviceWithIntegrity.Execute(request));
            Assert.Equal($"Invalid EntityKey Operation performed : Entity {Account.EntityLogicalName} does not contain an attribute named {dummy_attribute_name}", ex.Detail.Message);
        }

        [Fact]
        public void An_entity_which_references_another_existent_entity_by_alternate_key_can_be_initialised_when_integrity_is_enabled()
        {
            _contextWithIntegrity.InitializeMetadata(_accountMetadata);

            Entity otherEntity = new Entity("otherEntity");
            otherEntity.Id = Guid.NewGuid();
            otherEntity["new_accountId"] = new EntityReference("account", "alternateKey", "keyValue");

            _contextWithIntegrity.Initialize(new List<Entity>() { _account, otherEntity });

            Entity otherEntityInContext = _serviceWithIntegrity.Retrieve("otherEntity", otherEntity.Id, new ColumnSet(true));

            Assert.Equal(((EntityReference)otherEntityInContext["new_accountId"]).Id, _account.Id);
        }

        [Fact]
        public void An_entity_which_references_another_existent_entity_by_alternate_key_can_be_updated_when_integrity_is_enabled()
        {
            _contextWithIntegrity.InitializeMetadata(_accountMetadata);

            Entity otherEntity = new Entity("otherEntity");
            otherEntity.Id = Guid.NewGuid();
            otherEntity["new_accountId"] = new EntityReference("account", "alternateKey", "keyValue");

            _contextWithIntegrity.Initialize(new List<Entity>() { _account, _account2, otherEntity });

            var entityToUpdate = new Entity("otherEntity")
            {
                Id = otherEntity.Id,
                ["new_accountId"] = new EntityReference("account", "alternateKey", "keyValue2")
            };
            _serviceWithIntegrity.Update(entityToUpdate);

            Entity otherEntityInContext = _serviceWithIntegrity.Retrieve("otherEntity", otherEntity.Id, new ColumnSet(true));

            Assert.Equal(((EntityReference)otherEntityInContext["new_accountId"]).Id, _account2.Id);
        }
    }
}

#endif
