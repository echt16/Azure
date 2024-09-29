namespace lab3_1.Models.Database
{
    public class AzureStorage
    {
        public int Id {  get; set; }
        public string ConnectionString {  get; set; }
        public string Name { get; set; }
        public int PartitionKey {  get; set; }
        public virtual List<BlobContainer> BlobContainers { get; set; }
        public virtual List<QueueClient> QueueClients { get; set; }
        public AzureStorage()
        {
            BlobContainers = new List<BlobContainer>();
            QueueClients = new List<QueueClient>();
        }
    }
}
