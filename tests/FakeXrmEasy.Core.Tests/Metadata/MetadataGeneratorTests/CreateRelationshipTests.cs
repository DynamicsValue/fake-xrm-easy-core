using System;
using System.Linq;
using DataverseEntities;
using FakeXrmEasy.Metadata;
using Microsoft.Xrm.Sdk.Metadata;
using Xunit;

namespace FakeXrmEasy.Core.Tests.Metadata
{
    public class CreateRelationshipTests: FakeXrmEasyTestsBase
    {
        /*
         
         N:1 looks like ....
        /// <summary>
		/// N:1 account_master_account
		/// </summary>
		[Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("masterid")]
		[Microsoft.Xrm.Sdk.RelationshipSchemaNameAttribute("account_master_account", Microsoft.Xrm.Sdk.EntityRole.Referencing)]
		public Crm.Account Referencingaccount_master_account
		{
			get
			{
				return this.GetRelatedEntity<Crm.Account>("account_master_account", Microsoft.Xrm.Sdk.EntityRole.Referencing);
			}
		}
		
		/// <summary>
		/// N:1 contact_customer_accounts
		/// </summary>
		[Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("parentcustomerid")]
		[Microsoft.Xrm.Sdk.RelationshipSchemaNameAttribute("contact_customer_accounts")]
		public Crm.Account contact_customer_accounts
		
		
		1:N looks like ....
		/// <summary>
		/// 1:N ActivityMimeAttachment_AsyncOperations
		/// </summary>
		[Microsoft.Xrm.Sdk.RelationshipSchemaNameAttribute("ActivityMimeAttachment_AsyncOperations")]
		public System.Collections.Generic.IEnumerable<Crm.AsyncOperation> ActivityMimeAttachment_AsyncOperations
		{
		
		
		N:N looks like:
		/// <summary>
		/// N:N accountleads_association
		/// </summary>
		[Microsoft.Xrm.Sdk.RelationshipSchemaNameAttribute("accountleads_association")]
		public System.Collections.Generic.IEnumerable<Crm.Lead> accountleads_association
		{	
	
         */
        
        private const string SELF_REFERENTIAL_RELATIONSHIP_NAME = "account_master_account";
        
        [Fact]
        public void Should_return_self_referential_relationship_from_one_early_bound_type()
        {
            var entityMetadata = MetadataGenerator.FromType(typeof(Account), _context);
            
            var oneToMany =
                entityMetadata.OneToManyRelationships.FirstOrDefault(rel => rel.SchemaName.Equals(SELF_REFERENTIAL_RELATIONSHIP_NAME));
            
            Assert.NotNull(oneToMany);
            
            Assert.Equal(Account.EntityLogicalName, oneToMany.ReferencingEntity);
            Assert.Equal("masterid", oneToMany.ReferencingAttribute);
            Assert.Equal(Account.EntityLogicalName, oneToMany.ReferencedEntity);
            Assert.Equal("accountid", oneToMany.ReferencedAttribute);
            
            var manyToOne =
                entityMetadata.ManyToOneRelationships.FirstOrDefault(rel => rel.SchemaName.Equals(SELF_REFERENTIAL_RELATIONSHIP_NAME));
            
            Assert.NotNull(manyToOne);
            Assert.Equal(Account.EntityLogicalName, manyToOne.ReferencingEntity);
            Assert.Equal(Account.EntityLogicalName, manyToOne.ReferencedEntity);
            Assert.Equal("accountid", manyToOne.ReferencedAttribute);
            Assert.Equal("masterid", manyToOne.ReferencingAttribute);
            
        }
        
        [Fact]
        public void Should_return_entity_metadata_with_one_to_many_and_many_to_one_relationships()
        {
	        var accountEntityMetadata = MetadataGenerator.FromType(typeof(Account), _context);
	        var contactEntityMetadata = MetadataGenerator.FromType(typeof(Contact), _context);

	        AssertMasterRelationship(accountEntityMetadata);
	        AssertContactsRelationship(accountEntityMetadata, contactEntityMetadata);
        }
        
        [Fact]
        public void Should_return_entity_metadata_with_many_to_many_relationships()
        {
	        var testEntityMetadata = MetadataGenerator.FromType(typeof(dv_test), _context);
	        var contactEntityMetadata = MetadataGenerator.FromType(typeof(Contact), _context);
	        
	        AssertManyToManyRelationship(testEntityMetadata, contactEntityMetadata);
        }
        
        private void AssertMasterRelationship(EntityMetadata entityMetadata)
		{
		    var account_master_oneToMany = entityMetadata.OneToManyRelationships.FirstOrDefault(r => r.SchemaName == "account_master_account");

		    Assert.NotNull(account_master_oneToMany);
		    Assert.Equal("accountid", account_master_oneToMany.ReferencedAttribute);
		    Assert.Equal("account", account_master_oneToMany.ReferencedEntity);
		    Assert.Equal("account", account_master_oneToMany.ReferencingEntity);
		    Assert.Equal("masterid", account_master_oneToMany.ReferencingAttribute);

		    var account_master_manyToOne = entityMetadata.ManyToOneRelationships.FirstOrDefault(r => r.SchemaName == "account_master_account");
		    Assert.NotNull(account_master_manyToOne);
		    Assert.Equal("accountid", account_master_manyToOne.ReferencedAttribute);
		    Assert.Equal("account", account_master_manyToOne.ReferencedEntity);
		    Assert.Equal("account", account_master_manyToOne.ReferencingEntity);
		    Assert.Equal("masterid", account_master_manyToOne.ReferencingAttribute);
		}

		private void AssertContactsRelationship(EntityMetadata accountEntityMetadata, EntityMetadata contactEntityMetadata)
		{
		    var oneToMany = accountEntityMetadata.OneToManyRelationships.FirstOrDefault(r => r.SchemaName == "contact_customer_accounts");

		    Assert.NotNull(oneToMany);
		    Assert.Equal("accountid", oneToMany.ReferencedAttribute);
		    Assert.Equal("account", oneToMany.ReferencedEntity);
		    Assert.Equal("contact", oneToMany.ReferencingEntity);
		    Assert.Equal("parentcustomerid", oneToMany.ReferencingAttribute);

		    var manyToOne = contactEntityMetadata.ManyToOneRelationships.FirstOrDefault(r => r.SchemaName == "contact_customer_accounts");
		    Assert.NotNull(manyToOne);
		    Assert.Equal("accountid", manyToOne.ReferencedAttribute);
		    Assert.Equal("account", manyToOne.ReferencedEntity);
		    Assert.Equal("contact", manyToOne.ReferencingEntity);
		    Assert.Equal("parentcustomerid", manyToOne.ReferencingAttribute);

		    Assert.Null(accountEntityMetadata.ManyToOneRelationships.FirstOrDefault(r => r.SchemaName == "contact_customer_accounts"));
		    Assert.Null(contactEntityMetadata.OneToManyRelationships.FirstOrDefault(r => r.SchemaName == "contact_customer_accounts"));
		}
        
		private void AssertManyToManyRelationship(EntityMetadata contactEntityMetadata, EntityMetadata testEntityMetadata)
		{
			var manyToMany = contactEntityMetadata.ManyToManyRelationships.FirstOrDefault(r => r.SchemaName == "dv_test_Contact_Contact");

			Assert.NotNull(manyToMany);
			Assert.Equal("dv_testid", manyToMany.Entity1IntersectAttribute);
			Assert.Equal(dv_test.EntityLogicalName, manyToMany.Entity1LogicalName);
			Assert.Equal(Contact.EntityLogicalName, manyToMany.Entity2LogicalName);
			Assert.Equal("contactid", manyToMany.Entity2IntersectAttribute);

			var otherManyToMany = testEntityMetadata.ManyToManyRelationships.FirstOrDefault(r => r.SchemaName == "dv_test_Contact_Contact");
			Assert.NotNull(otherManyToMany);
			Assert.Equal("dv_testid", manyToMany.Entity1IntersectAttribute);
			Assert.Equal(dv_test.EntityLogicalName, manyToMany.Entity1LogicalName);
			Assert.Equal(Contact.EntityLogicalName, manyToMany.Entity2LogicalName);
			Assert.Equal("contactid", manyToMany.Entity2IntersectAttribute);
		}
    }
}