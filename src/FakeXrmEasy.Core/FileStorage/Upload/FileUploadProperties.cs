using Microsoft.Xrm.Sdk;

namespace FakeXrmEasy.Core.FileStorage.Upload
{
    /// <summary>
    /// Properties needed to initialize a file upload
    /// </summary>
    internal class FileUploadProperties
    {
        public EntityReference Target { get; set; }
        public string FileAttributeName { get; set; }
        public string FileName { get; set; }

        internal FileUploadProperties()
        {
            
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="other"></param>
        internal FileUploadProperties(FileUploadProperties other)
        {
            if (other.Target != null)
            {
                Target = new EntityReference(other.Target.LogicalName, other.Target.Id);
            }

            FileAttributeName = other.FileAttributeName;
            FileName = other.FileName;
        }
    }
}