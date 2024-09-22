namespace lab3_1.Models.Database
{
    public class BlobContainer
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public virtual List<BlobFile> BlobFiles { get; set; }
        public int AzureStorageId { get; set; }
        public AzureStorage AzureStorage { get; set; }
        public int UserId {  get; set; }
        public User User { get; set; }
        public BlobContainer()
        {
            BlobFiles = new List<BlobFile>();
        }
    }
}
