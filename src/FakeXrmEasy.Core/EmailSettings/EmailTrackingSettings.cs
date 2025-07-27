namespace FakeXrmEasy.Core.EmailSettings
{
    /// <summary>
    /// Provides configuration settings for handling of Email activities and their related messages
    /// </summary>
    public interface IEmailTrackingSettings
    {
        /// <summary>
        /// The Tracking Prefix for the email tracking token ('CRM' by default)
        /// </summary>
        string TrackingPrefix { get; set; }
        
        /// <summary>
        /// The number of digits to use for the generated token (3 by default)
        /// </summary>
        int NumberOfDigits { get; set; }
        
        /// <summary>
        /// Do not use. Stores information about the next tracking token number that will be generated
        /// </summary>
        int NextTrackingNumber { get; set; }
        
        /// <summary>
        /// Do not use. Stores the maximum tracking token number based on the number of digits property.
        /// </summary>
        int MaxTrackingNumber { get; set; }

        /// <summary>
        /// Gets the next available tracking token value and increments the counter
        /// </summary>
        /// <returns></returns>
        string GenerateNewTrackingTokenValue();
        
        /// <summary>
        /// True if tracking token will be generated, false otherwise. True by default
        /// </summary>
        bool IsEnabled { get; set; }
    }
    
    /// <summary>
    /// Sets the default values for email tracking
    /// </summary>
    public class EmailTrackingSettings: IEmailTrackingSettings
    {
        internal object _lock = new object();
        
        /// <summary>
        /// The Tracking Prefix for the email tracking token ('CRM' by default)
        /// </summary>
        public string TrackingPrefix { get; set; }
        
        /// <summary>
        /// The number of digits to use for the generated token (3 by default)
        /// </summary>
        public int NumberOfDigits { get; set; }
        
        /// <summary>
        /// Do not use. Stores information about the next tracking token number that will be generated
        /// </summary>
        public int NextTrackingNumber { get; set; }
        
        /// <summary>
        /// Do not use. Stores the maximum tracking token number based on the number of digits property.
        /// </summary>
        public int MaxTrackingNumber { get; set; }

        /// <summary>
        /// True if tracking token will be generated, false otherwise. True by default
        /// </summary>
        public bool IsEnabled { get; set; }
        
        /// <summary>
        /// Sets the default email tracking properties
        /// </summary>
        public EmailTrackingSettings()
        {
            TrackingPrefix = "CRM";
            NumberOfDigits = 3;
            NextTrackingNumber = 0;
            MaxTrackingNumber = 999;
            IsEnabled = true;
        }

        /// <summary>
        /// Gets the next available tracking token value and increments the counter
        /// </summary>
        /// <returns></returns>
        public string GenerateNewTrackingTokenValue()
        {
            lock (_lock)
            {
                if (NextTrackingNumber == 999)
                {
                    NextTrackingNumber = 0;
                }
                NextTrackingNumber++;
                return $"{TrackingPrefix}:0235{NextTrackingNumber.ToString($"D{NumberOfDigits.ToString()}")}";
            }
        }
    }
}