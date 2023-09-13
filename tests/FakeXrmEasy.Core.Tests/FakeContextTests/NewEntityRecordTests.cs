using System;
using Crm;
using FakeXrmEasy.Tests;
using Xunit;

namespace FakeXrmEasy.Core.Tests.FakeContextTests
{
    
    public class NewEntityRecordTests: FakeXrmEasyTestsBase
    {
        [Fact]
        public void Should_use_late_bound_if_no_early_bound_entity_has_been_used()
        {
            var contact = ((XrmFakedContext)_context).NewEntityRecord("contact");
            Assert.IsNotType<Contact>(contact);
        }
        
        [Fact]
        public void Should_use_early_bound_if_the_context_is_using_early_bound()
        {
            _context.Initialize(new Contact() { Id = Guid.NewGuid()});
            var contact = ((XrmFakedContext)_context).NewEntityRecord("contact");
            Assert.IsType<Contact>(contact);
        }
    }
}