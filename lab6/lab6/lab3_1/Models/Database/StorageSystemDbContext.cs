using Microsoft.EntityFrameworkCore;
namespace lab3_1.Models.Database
{
    public class StorageSystemDbContext : DbContext
    {

        public DbSet<AzureStorage> AzureStorages { get; set; }
        public DbSet<BlobContainer> BlobContainers { get; set; }
        public DbSet<BlobFile> BlobFiles { get; set; }
        public DbSet<File> Files { get; set; }
        public DbSet<LoginPassword> LoginPasswords { get; set; }
        public DbSet<QueueClient> QueueClients { get; set; }
        public DbSet<QueueItem> QueueItems { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Status> Statuses { get; set; }
        public DbSet<User> Users { get; set; }

        public StorageSystemDbContext(DbContextOptions<StorageSystemDbContext> options) : base(options)
        {
            Database.EnsureCreated();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<BlobFile>()
                .HasOne(bf => bf.File)
                .WithMany(f => f.BlobFiles)
                .HasForeignKey(bf => bf.FileId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
