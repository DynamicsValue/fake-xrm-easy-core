#if !FAKE_XRM_EASY && !FAKE_XRM_EASY_2013 && !FAKE_XRM_EASY_2015

using FakeXrmEasy.Extensions;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using System;
using System.Linq;
using System.Reflection;
using DataverseEntities;
using Microsoft.Xrm.Sdk;
using Xunit;
using Contact = Crm.Contact;

namespace FakeXrmEasy.Core.Tests.Middleware.Crud.FakeMessageExecutors.UpsertRequestTests
{
    public class UpsertRequestAlternateKeyTests : FakeXrmEasyTestsBase
    {
        private const string KEY = "C0001";
        
        public UpsertRequestAlternateKeyTests()
        {
            var assembly = Assembly.GetAssembly(typeof(dv_test));
            _context.EnableProxyTypes(assembly);
            _context.InitializeMetadata(assembly);
            
            var metadata = _context.GetEntityMetadataByName("dv_test");
            metadata.SetFieldValue("_keys", new EntityKeyMetadata[]
            {
                new EntityKeyMetadata()
                {
                    KeyAttributes = new string[]{"dv_code"}
                }
            });
            _context.SetEntityMetadata(metadata);
        }
        
        [Fact]
        public void Upsert_Creates_Record_When_It_Does_Not_Exist_Using_Alternate_Key()
        {
            var alternateKeyValue = Guid.NewGuid().ToString();

            var record = new dv_test();
            record.KeyAttributes.Add("dv_code", alternateKeyValue);

            var request = new UpsertRequest()
            {
                Target = record
            };

            var response = (UpsertResponse)_service.Execute(request);
            
            Assert.True(response.RecordCreated);
        }

        [Fact]
        public void Upsert_Updates_Record_When_It_Exists_Using_Alternate_Key()
        {
            var existingRecord = new dv_test()
            {
                Id = Guid.NewGuid(),
                dv_code = KEY
            };

            _context.Initialize(existingRecord);
            
            var record = new dv_test();
            record.KeyAttributes.Add("dv_code", KEY);

            var request = new UpsertRequest()
            {
                Target = record
            };

            var response = (UpsertResponse)_service.Execute(request);
            
            Assert.False(response.RecordCreated);
        }        
    }
}
#endif