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
    }
}