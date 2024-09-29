namespace lab3_1.Models.Database
{
    public class Status
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public virtual List<File> Files { get; set; }
        public int PartitionKey { get; set; }
        public Status()
        {
            Files = new List<File>();
        }
    }
}
