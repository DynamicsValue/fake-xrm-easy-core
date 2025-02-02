using Microsoft.Xrm.Sdk;

namespace FakeXrmEasy.Core.FileStorage.Download
{
    internal class FileDownloadProperties
    {
        internal EntityReference Target { get; set; }
        internal string FileAttributeName { get; set; }

        internal FileDownloadProperties()
        {
            
        }
        
        internal FileDownloadProperties(FileDownloadProperties other)
        {
            if (other.Target != null)
            {
                Target = new EntityReference(other.Target.LogicalName, other.Target.Id);
            }

            FileAttributeName = other.FileAttributeName;
        }
    }
}