using System;
using Crm;
using Xunit;

namespace FakeXrmEasy.Tests
{
    public class FakeXrmEasyTestsAddEntity : FakeXrmEasyTestsBase
    {
        [Fact]
        public void Should_set_default_caller_id_when_adding_an_new_entity()
        {
            var contact = new Contact() { Id = Guid.NewGuid() };
            
            _context.AddEntityWithDefaults(contact);

            Assert.NotNull(_context.CallerProperties.CallerId);
        }
    }

}