namespace lab3_1.Models.Database
{
    public class LoginPassword
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public virtual List<User> Users { get; set; }
        public LoginPassword()
        {
            Users = new List<User>();
        }
    }
}
