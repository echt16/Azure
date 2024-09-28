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
             Database.EnsureCreatedAsync();
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // LoginPassword
            modelBuilder.Entity<LoginPassword>()
                .HasKey(lp => lp.Id);

            modelBuilder.Entity<LoginPassword>()
                .HasMany(lp => lp.Users)
                .WithOne(u => u.LoginPassword)
                .OnDelete(DeleteBehavior.Cascade);

            // File
            modelBuilder.Entity<File>()
                .HasKey(f => f.Id);

            modelBuilder.Entity<File>()
                .HasOne(f => f.User)
                .WithMany(u => u.Files)
                .HasForeignKey(f => f.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<File>()
                .HasOne(f => f.Status)
                .WithMany(s => s.Files)
                .HasForeignKey(f => f.StatusId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<File>()
                .HasMany(f => f.BlobFiles)
                .WithOne(bf => bf.File)
                .HasForeignKey(bf => bf.FileId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<File>()
                .HasMany(f => f.QueueItems)
                .WithOne(qi => qi.File)
                .HasForeignKey(qi => qi.FileId)
                .OnDelete(DeleteBehavior.Cascade);

            // BlobFile
            modelBuilder.Entity<BlobFile>()
                .HasKey(bf => bf.Id);

            modelBuilder.Entity<BlobFile>()
                .HasOne(bf => bf.BlobContainer)
                .WithMany(bc => bc.BlobFiles)
                .HasForeignKey(bf => bf.BlobContainerId)
                .OnDelete(DeleteBehavior.Cascade);

            // BlobContainer
            modelBuilder.Entity<BlobContainer>()
                .HasKey(bc => bc.Id);

            modelBuilder.Entity<BlobContainer>()
                .HasOne(bc => bc.AzureStorage)
                .WithMany(a => a.BlobContainers)
                .HasForeignKey(bc => bc.AzureStorageId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<BlobContainer>()
                .HasOne(bc => bc.User)
                .WithMany(u => u.BlobContainers)
                .HasForeignKey(bc => bc.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // AzureStorage
            modelBuilder.Entity<AzureStorage>()
                .HasKey(a => a.Id);

            modelBuilder.Entity<AzureStorage>()
                .HasMany(a => a.BlobContainers)
                .WithOne(bc => bc.AzureStorage)
                .HasForeignKey(bc => bc.AzureStorageId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<AzureStorage>()
                .HasMany(a => a.QueueClients)
                .WithOne(qc => qc.AzureStorage)
                .HasForeignKey(qc => qc.AzureStorageId)
                .OnDelete(DeleteBehavior.Cascade);

            // QueueClient
            modelBuilder.Entity<QueueClient>()
                .HasKey(qc => qc.Id);

            modelBuilder.Entity<QueueClient>()
                .HasMany(qc => qc.QueueItems)
                .WithOne(qi => qi.QueueClient)
                .HasForeignKey(qi => qi.QueueClientId)
                .OnDelete(DeleteBehavior.Cascade);

            // QueueItem
            modelBuilder.Entity<QueueItem>()
                .HasKey(qi => qi.Id);

            modelBuilder.Entity<QueueItem>()
                .HasOne(qi => qi.File)
                .WithMany(f => f.QueueItems)
                .HasForeignKey(qi => qi.FileId)
                .OnDelete(DeleteBehavior.Cascade);

            // Role
            modelBuilder.Entity<Role>()
                .HasKey(r => r.Id);

            modelBuilder.Entity<Role>()
                .HasMany(r => r.Users)
                .WithOne(u => u.Role)
                .HasForeignKey(u => u.RoleId)
                .OnDelete(DeleteBehavior.Cascade);

            // Status
            modelBuilder.Entity<Status>()
                .HasKey(s => s.Id);

            modelBuilder.Entity<Status>()
                .HasMany(s => s.Files)
                .WithOne(f => f.Status)
                .HasForeignKey(f => f.StatusId)
                .OnDelete(DeleteBehavior.Cascade);

            // User
            modelBuilder.Entity<User>()
                .HasKey(u => u.Id);

            modelBuilder.Entity<User>()
                .HasOne(u => u.Role)
                .WithMany(r => r.Users)
                .HasForeignKey(u => u.RoleId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<User>()
                .HasOne(u => u.LoginPassword)
                .WithMany(lp => lp.Users)
                .HasForeignKey(u => u.LoginPasswordId)
                .OnDelete(DeleteBehavior.Cascade);
        }


    }
}
