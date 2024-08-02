namespace FakeXrmEasy.Core.FileStorage
{
    public interface IFileStorageSettings
    {
        /// <summary>
        /// Sets or gets the default maximum file size for file uploads
        /// </summary>
        int MaxSizeInKB { get; set; }
        
        /// <summary>
        /// Sets or gets the default maximum file size for image uploads
        /// </summary>
        int ImageMaxSizeInKB { get; set; }
    }
    public class FileStorageSettings: IFileStorageSettings
    {
        
        /// <summary>
        /// The default max size (i.e 32 MB)
        /// </summary>
        public const int DEFAULT_MAX_FILE_SIZE_IN_KB = 32768;

        /// <summary>
        /// The maximum file size supported by the platform
        /// https://learn.microsoft.com/en-us/dotnet/api/microsoft.xrm.sdk.metadata.fileattributemetadata.maxsizeinkb?view=dataverse-sdk-latest#microsoft-xrm-sdk-metadata-fileattributemetadata-maxsizeinkb
        /// </summary>
        public const int MAX_SUPPORTED_FILE_SIZE_IN_KB = 10485760;
        
        /// <summary>
        /// The maximum file size supported for Image column types
        /// https://learn.microsoft.com/en-us/power-apps/developer/data-platform/files-images-overview?tabs=sdk
        /// </summary>
        public const int MAX_SUPPORTED_IMAGE_FILE_SIZE_IN_KB = 30 * 1024;
        
        /// <summary>
        /// Sets or gets the current maximum file size for file uploads that use file storage
        /// </summary>
        public int MaxSizeInKB { get; set; }

        /// <summary>
        /// Sets or gets the default maximum file size for image uploads
        /// </summary>
        public int ImageMaxSizeInKB { get; set; }
        
        /// <summary>
        /// Default constructor
        /// </summary>
        public FileStorageSettings()
        {
            MaxSizeInKB = DEFAULT_MAX_FILE_SIZE_IN_KB;
            ImageMaxSizeInKB = MAX_SUPPORTED_IMAGE_FILE_SIZE_IN_KB;
        }
    }
}