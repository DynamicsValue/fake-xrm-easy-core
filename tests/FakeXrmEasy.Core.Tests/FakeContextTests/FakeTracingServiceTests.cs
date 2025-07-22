using Xunit;
using System;

namespace FakeXrmEasy.Core.Tests.FakeContextTests
{
    public class FakeTracingServiceTests
    {
        private readonly XrmFakedTracingService _tracingService; 
        public FakeTracingServiceTests()
        {
            _tracingService = new XrmFakedTracingService();
        }
        
        [Fact]
        public void When_a_trace_is_dumped_it_should_return_right_traces()
        {
            var trace1 = "This is one trace";
            var trace2 = "This is a second trace";

            _tracingService.Trace(trace1);
            _tracingService.Trace(trace2);

            Assert.Equal(_tracingService.DumpTrace(), trace1 + Environment.NewLine + trace2 + Environment.NewLine);
        }
        
        [Fact]
        public void Should_write_trace_with_formatting_and_without_args()
        {
            var previousTraceTime = DateTime.UtcNow.AddMilliseconds(-300);
            var utcNow = DateTime.UtcNow;

            var deltaMiliseconds = utcNow.Subtract(previousTraceTime).TotalMilliseconds;
            var message = "Fake Message";
            var zeroMessage = "{0}";

            string[] args = { "Some message" };
                
            _tracingService.Trace($"[+{deltaMiliseconds:N0}ms - {zeroMessage}]");
             
        }
    }
}