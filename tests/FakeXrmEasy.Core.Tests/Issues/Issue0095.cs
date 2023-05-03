using FakeXrmEasy.Tests;
using Xunit;

namespace FakeXrmEasy.Core.Tests.Issues
{
    public class Issue0095 : FakeXrmEasyTestsBase
    {
        [Theory]
        [InlineData("sdkmessage")]
        [InlineData("sdkmessagefilter")]
        [InlineData("sdkmessageprocessingstep")]
        [InlineData("plugintype")]
        public void should_always_find_pipeline_types(string logicalName)
        {
            // Act
            var reflectedType = _context.FindReflectedType(logicalName);

            // Assert
            Assert.NotNull(reflectedType);
        }

        [Fact]
        public void should_not_contain_unnecessary_types()
        {
            // Act
            var reflectedType = _context.FindReflectedType("account");

            // Assert
            Assert.Null(reflectedType);
        }
    }
}
