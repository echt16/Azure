namespace lab3_1.Models.Database
{
    public class BlobFile
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime CreatedAt { get; set; }
        public int BlobContainerId { get; set; }
        public BlobContainer BlobContainer { get; set; }
        public int FileId {  get; set; }
        public File File { get; set; }
    }
}
