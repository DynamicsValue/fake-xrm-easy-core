namespace FakeXrmEasy.Core.FileStorage.Upload
{
    internal class CommitFileUploadSessionProperties
    {
        public string FileUploadSessionId { get; set; }
        public string FileName { get; set; }
        public string MimeType { get; set; }
        public string[] BlockIdsListSequence { get; set; }
    }
}