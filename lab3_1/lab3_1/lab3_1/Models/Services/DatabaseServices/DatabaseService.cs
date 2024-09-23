using lab3_1.Models.Database;
using lab3_1.Models.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace lab3_1.Models.Services.DatabaseServices
{
    public class DatabaseService
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public DatabaseService(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
            LoadAll().Wait(); // Можно вызвать загрузку данных в конструкторе
        }

        private async Task LoadAll()
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<StorageSystemDbContext>();
                await LoadRoles(context);
                await LoadStatuses(context);
                await LoadAzureStorage(context);
            }
        }

        private async Task LoadRoles(StorageSystemDbContext context)
        {
            if (!context.Roles.Any(x => x.Name == "User"))
            {
                context.Roles.Add(new Role() { Name = "User" });
                await context.SaveChangesAsync();
            }
        }

        private async Task LoadAzureStorage(StorageSystemDbContext context)
        {
            if (!context.AzureStorages.Any(x => x.Name == "AZURE STORAGE"))
            {
                context.AzureStorages.Add(new AzureStorage()
                {
                    ConnectionString = Environment.GetEnvironmentVariable("AZURE_STORAGE_CONNECTION_STRING"),
                    Name = "AZURE STORAGE"
                });
                await context.SaveChangesAsync();
            }
        }

        private async Task LoadStatuses(StorageSystemDbContext context)
        {
            if (!context.Statuses.Any(x => x.Name == "Loaded"))
            {
                context.Statuses.AddRange(new[]
                {
                    new Status { Name = "Loaded" },
                    new Status { Name = "Sent" }
                });
                await context.SaveChangesAsync();
            }
        }

        public async Task AddQueueIfNotExists(string queueName)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<StorageSystemDbContext>();
                if (!context.QueueClients.Any(client => client.Name == queueName))
                {
                    int azId = context.AzureStorages.First().Id;
                    context.QueueClients.Add(new QueueClient()
                    {
                        AzureStorageId = azId,
                        Name = queueName
                    });
                    await context.SaveChangesAsync();
                }
            }
        }

        public async Task AddBlobContainerIfNotExists(string blobContainerName, int userId)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<StorageSystemDbContext>();
                if (!context.BlobContainers.Any(client => client.Name == blobContainerName))
                {
                    int azId = context.AzureStorages.First().Id;
                    context.BlobContainers.Add(new BlobContainer()
                    {
                        AzureStorageId = azId,
                        Name = blobContainerName,
                        UserId = userId
                    });
                    await context.SaveChangesAsync();
                }
            }
        }

        public async Task AddMessageToQueue(string queueName, string message, string messageId, int fileId)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<StorageSystemDbContext>();
                QueueClient client = context.QueueClients.First(x => x.Name == queueName);
                QueueItem queueItem = new QueueItem()
                {
                    CreatedAt = DateTime.UtcNow,
                    FileId = fileId,
                    MessageId = messageId,
                    MessageText = message,
                    QueueClientId = client.Id
                };
                context.QueueItems.Add(queueItem);
                await context.SaveChangesAsync();
            }
        }

        public async Task<Database.File> AddFileToDb(string extension, string fileName, string fullPath, int statusId, int userId)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<StorageSystemDbContext>();
                Database.File file = new Database.File()
                {
                    Extension = extension,
                    FileName = fileName,
                    LocalFullPath = fullPath,
                    StatusId = statusId,
                    UserId = userId
                };
                context.Add(file);
                await context.SaveChangesAsync();
                return file;
            }
        }

        public async Task<Database.File> TransferToQueueStorage(int fileId)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<StorageSystemDbContext>();
                Database.File file = context.Files.First(x => x.Id == fileId);
                context.QueueItems.First(x => x.FileId == fileId).QueueClientId = context.QueueClients.First(x => x.Name == "files-queue-sent").Id;
                await context.SaveChangesAsync();
                file.StatusId = GetIdOfStatus("Sent");
                await context.SaveChangesAsync();
                return file;
            }
        }

        public async Task TransferToBlobStorage(int fileId, string containerName)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<StorageSystemDbContext>();
                Database.File file = context.Files.First(x => x.Id == fileId);
                context.QueueItems.Remove(context.QueueItems.First(x => x.FileId == fileId));
                await context.SaveChangesAsync();
                int contId = context.BlobContainers.First(x => x.Name == containerName).Id;
                context.BlobFiles.Add(new BlobFile()
                {
                    FileId = fileId,
                    BlobContainerId = contId,
                    CreatedAt = DateTime.UtcNow,
                    Name = file.FileName
                });
                await context.SaveChangesAsync();
            }
        }

        public async Task ChangeMessageId(int fileId, string messageId)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<StorageSystemDbContext>();
                context.QueueItems.First(x => x.FileId == fileId).MessageId = messageId;
                await context.SaveChangesAsync();
            }
        }

        public int GetIdOfStatus(string statusName)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<StorageSystemDbContext>();
                try
                {
                    return context.Statuses.First(x => x.Name == statusName).Id;
                }
                catch
                {
                    return 0;
                }
            }
        }

        public bool ExistsUser(string login)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<StorageSystemDbContext>();
                return context.LoginPasswords.Any(x => x.Username == login);
            }
        }

        public async Task<AccountModel?> CheckAuthorization(string login, string password)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<StorageSystemDbContext>();
                try
                {
                    if (!context.LoginPasswords.Any(x => x.Username == login && x.Password == password))
                        return null;
                    LoginPassword lp = context.LoginPasswords.First(x => x.Username == login && x.Password == password);
                    User user = context.Users.First(x => x.LoginPasswordId == lp.Id);
                    Role role = context.Roles.First(x => x.Id == user.RoleId);
                    return new AccountModel()
                    {
                        Id = user.Id,
                        Role = role.Name
                    };
                }
                catch (Exception)
                {
                    return null;
                }
            }
        }

        public async Task<bool> AddUser(string login, string password, string firstname, string lastname)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<StorageSystemDbContext>();
                try
                {
                    LoginPassword lp = new LoginPassword()
                    {
                        Username = login,
                        Password = password
                    };
                    if (ExistsUser(login))
                        throw new Exception("Login already exists");
                    context.LoginPasswords.Add(lp);
                    await context.SaveChangesAsync();
                    int roleId = context.Roles.First().Id;
                    context.Users.Add(new User()
                    {
                        LoginPasswordId = lp.Id,
                        Firstname = firstname,
                        Lastname = lastname,
                        RoleId = roleId
                    });
                    await context.SaveChangesAsync();
                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }
    }
}
