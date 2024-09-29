namespace lab3_1.Models.Database
{
    public class File
    {
        public int Id { get; set; }
        public string? LocalFullPath { get; set; }
        public string FileName { get; set; }
        public string Extension { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }
        public int StatusId { get; set; }
        public Status Status { get; set; }
        public virtual List<BlobFile> BlobFiles { get; set; }
        public virtual List<QueueItem> QueueItems { get; set; }
        public int PartitionKey { get; set; }
        public File()
        {
            BlobFiles = new List<BlobFile>();
            QueueItems = new List<QueueItem>();
        }
    }
}
