namespace FakeXrmEasy.Core.EmailSettings
{
    public interface IEmailTrackingSettings
    {
        string TrackingPrefix { get; set; }
        int NumberOfDigits { get; set; }
        int NextTrackingNumber { get; set; }
        int MaxTrackingNumber { get; set; }
    }
    
    /// <summary>
    /// Sets the default values for email tracking
    /// </summary>
    public class EmailTrackingSettings: IEmailTrackingSettings
    {
        public string TrackingPrefix { get; set; }
        public int NumberOfDigits { get; set; }
        public int NextTrackingNumber { get; set; }
        public int MaxTrackingNumber { get; set; }

        /// <summary>
        /// Sets the default email tracking properties
        /// </summary>
        public EmailTrackingSettings()
        {
            TrackingPrefix = "CRM";
            NumberOfDigits = 3;
            NextTrackingNumber = 0;
            MaxTrackingNumber = 999;
        }
    }
}