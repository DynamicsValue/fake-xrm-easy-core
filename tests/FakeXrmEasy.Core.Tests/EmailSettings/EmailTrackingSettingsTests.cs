using FakeXrmEasy.Core.EmailSettings;
using Xunit;

namespace FakeXrmEasy.Core.Tests.EmailSettings
{
    public class EmailTrackingSettingsTests: FakeXrmEasyTestsBase
    {
        [Fact]
        public void Should_populate_default_email_tracking_settings()
        {
            var emailTrackingSettings = _context.GetProperty<IEmailTrackingSettings>();
            Assert.NotNull(emailTrackingSettings);
        }
        
        [Fact]
        public void Should_generate_first_tracking_number()
        {
            var emailTrackingSettings = _context.GetProperty<IEmailTrackingSettings>();
            var trackingToken = emailTrackingSettings.GenerateNewTrackingTokenValue();
            Assert.Equal($"CRM: 0235001", trackingToken);
        }
        
        [Fact]
        public void Should_generate_next_tracking_number() 
        {
            var emailTrackingSettings = _context.GetProperty<IEmailTrackingSettings>();
            emailTrackingSettings.NextTrackingNumber = 101;
            var trackingToken = emailTrackingSettings.GenerateNewTrackingTokenValue();
            Assert.Equal($"CRM: 0235102", trackingToken);
        }
        
        [Fact]
        public void Should_generate_last_tracking_number() 
        {
            var emailTrackingSettings = _context.GetProperty<IEmailTrackingSettings>();
            emailTrackingSettings.NextTrackingNumber = 998;
            var trackingToken = emailTrackingSettings.GenerateNewTrackingTokenValue();
            Assert.Equal($"CRM: 0235999", trackingToken);
        }
        
        [Fact]
        public void Should_generate_next_tracking_number_and_start_over_when_max_is_reached() 
        {
            var emailTrackingSettings = _context.GetProperty<IEmailTrackingSettings>();
            emailTrackingSettings.NextTrackingNumber = 999;
            var trackingToken = emailTrackingSettings.GenerateNewTrackingTokenValue();
            Assert.Equal($"CRM: 0235001", trackingToken);
        }
    }
}