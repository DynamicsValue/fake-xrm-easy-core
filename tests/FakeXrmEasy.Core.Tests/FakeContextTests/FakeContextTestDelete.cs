
/* Previous Moved to FakeXrmEasy.Tests.Middleware.Crud.FakeMessageExecutors.DeleteRequestTests */
/* This file now contains tests against the DeleteEntity method */

using System;
using Crm;
using Microsoft.Xrm.Sdk;
using Xunit;


namespace FakeXrmEasy.Core.Tests
{
    public class FakeXrmEasyTestsDelete : FakeXrmEasyTestsBase
    {
        [Fact]
        public void Should_return_error_if_entity_logical_name_does_not_exist_and_not_using_proxy_types() 
        {
            Assert.Throws<InvalidOperationException>(() =>_context.DeleteEntity(new EntityReference("contact", Guid.NewGuid()))); 
        }

        [Fact]
        public void Should_return_error_if_entity_logical_name_does_not_exist_and_reflected_type_is_not_found() 
        {
            var contact = new Contact() 
            { 
                Id = Guid.NewGuid(),
                FirstName = "Steve",
                LastName = "Vai"
            };
            _context.Initialize(contact);
            Assert.Throws<InvalidOperationException>(() =>_context.DeleteEntity(new EntityReference("otherEntity", Guid.NewGuid()))); 
        }
        
    }
}