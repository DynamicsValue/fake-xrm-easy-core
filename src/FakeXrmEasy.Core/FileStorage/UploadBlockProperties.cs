namespace FakeXrmEasy.Core.FileStorage
{
    internal interface IUploadBlockProperties
    {
        string BlockId { get; set; }
        byte[] BlockContents { get; set; }
        string FileContinuationToken { get; set; }
    }
    internal class UploadBlockProperties: IUploadBlockProperties
    {
        public string BlockId { get; set; }
        public byte[] BlockContents { get; set; }
        public string FileContinuationToken { get; set; }
    }
}