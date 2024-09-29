namespace lab3_1.Models.Database
{
    public class QueueClient
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int AzureStorageId { get; set; }
        public AzureStorage AzureStorage { get; set; }
        public virtual List<QueueItem> QueueItems { get; set; }
        public int PartitionKey { get; set; }
        public QueueClient()
        {
            QueueItems = new List<QueueItem>();
        }
    }
}
