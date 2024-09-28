namespace lab3_1.Models.Database
{
    public class User
    {
        public int Id { get; set; }
        public string Firstname { get; set; }
        public string Lastname { get; set; }
        public int RoleId {  get; set; }
        public Role Role { get; set; }
        public int LoginPasswordId { get; set; }
        public LoginPassword LoginPassword { get; set; }
        public virtual List<File> Files { get; set; }
        public virtual List<BlobContainer> BlobContainers { get; set; }

        public User()
        {
            Files = new List<File>();
        }
    }
}
