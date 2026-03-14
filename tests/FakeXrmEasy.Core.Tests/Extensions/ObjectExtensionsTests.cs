using System;
using FakeXrmEasy.Extensions;
using Microsoft.Xrm.Sdk.Metadata;
using Xunit;

namespace FakeXrmEasy.Core.Tests.Extensions
{
    public class ObjectExtensionsTests
    {
        [Fact]
        public void Should_throw_exception_when_setting_an_unknown_field()
        {
            var metadata = new EntityMetadata();
            Assert.Throws<ArgumentOutOfRangeException>(() => metadata.SetFieldValue("non-existing", 1));
        }
    }
}