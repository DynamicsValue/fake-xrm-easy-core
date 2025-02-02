using Crm;
using FakeXrmEasy.Extensions;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using System;
using System.Linq;
using System.Reflection;
using Xunit;

namespace FakeXrmEasy.Core.Tests.Middleware.Crud.FakeMessageExecutors.UpsertRequestTests
{
#if !FAKE_XRM_EASY && !FAKE_XRM_EASY_2013 && !FAKE_XRM_EASY_2015
    public class UpsertRequestTests : FakeXrmEasyTestsBase
    {
        [Fact]
        public void Upsert_Creates_Record_When_It_Does_Not_Exist()
        {
            _context.EnableProxyTypes(Assembly.GetExecutingAssembly());

            var contact = new Contact()
            {
                Id = Guid.NewGuid(),
                FirstName = "FakeXrm",
                LastName = "Easy"
            };

            var request = new UpsertRequest()
            {
                Target = contact
            };

            var response = (UpsertResponse)_service.Execute(request);

            var contactCreated = _context.CreateQuery<Contact>().FirstOrDefault();

            Assert.True(response.RecordCreated);
            Assert.NotNull(contactCreated);
        }

        [Fact]
        public void Upsert_Updates_Record_When_It_Exists()
        {
            var contact = new Contact()
            {
                Id = Guid.NewGuid(),
                FirstName = "FakeXrm"
            };
            _context.Initialize(new[] { contact });

            contact = new Contact()
            {
                Id = contact.Id,
                FirstName = "FakeXrm2",
                LastName = "Easy"
            };

            var request = new UpsertRequest()
            {
                Target = contact
            };


            var response = (UpsertResponse)_service.Execute(request);
            var contactUpdated = _context.CreateQuery<Contact>().FirstOrDefault();

            Assert.False(response.RecordCreated);
            Assert.Equal("FakeXrm2", contactUpdated.FirstName);
        }
    }
#endif
}
