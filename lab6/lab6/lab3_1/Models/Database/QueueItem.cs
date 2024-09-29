namespace lab3_1.Models.Database
{
    public class QueueItem
    {
        public int Id { get; set; }
        public string MessageId { get; set; }
        public string MessageText { get; set; }
        public DateTime CreatedAt { get; set; }
        public int QueueClientId { get; set; }
        public QueueClient QueueClient { get; set; }
        public int FileId {  get; set; }
        public File File { get; set; }
        public int PartitionKey { get; set; }
    }
}
