//Moved to fake-xrm-easy-plugins repo / XrmFakedPluginContextPropertiesTests.cs

using Xunit;

namespace FakeXrmEasy.Core.Tests 
{ 
    public class XrmFakedTracingServiceTests : FakeXrmEasyTestsBase
    {
        [Fact]
        public void The_TracingService_Should_Be_Retrievable_Without_Calling_Execute_Before()
        {
            //Get tracing service
            var fakeTracingService = _context.GetTracingService();

            Assert.NotNull(fakeTracingService);
        }

        [Fact]
        public void Retrieving_The_TracingService_Twice_Should_Return_The_Same_Instance()
        {
            //Get tracing service
            var fakeTracingService1 = _context.GetTracingService();
            fakeTracingService1.Trace("foobar");

            var fakeTracingService2 = _context.GetTracingService();

            Assert.NotNull(fakeTracingService1);
            Assert.NotNull(fakeTracingService2);

            Assert.Contains("foobar", fakeTracingService2.DumpTrace());
        }
    }
}