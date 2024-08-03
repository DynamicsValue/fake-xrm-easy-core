namespace FakeXrmEasy.Core.FileStorage
{
    internal interface ICommitBlocksListProperties
    {
        string FileContinuationToken { get; set; }
        string FileName { get; set; }
        string MimeType { get; set; }
        string[] BlockListSequence { get; set; }
    }

    internal class CommitBlocksListProperties : ICommitBlocksListProperties
    {
        public string FileContinuationToken { get; set; }
        public string FileName { get; set; }
        public string MimeType { get; set; }
        public string[] BlockListSequence { get; set; }
    }
}