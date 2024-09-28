using lab3_1.Models.Database;
using lab3_1.Models.ViewModels;
using Microsoft.Azure.Cosmos;
using Microsoft.EntityFrameworkCore;

namespace lab3_1.Models.Services.DatabaseServices
{
    public class DatabaseService
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public DatabaseService(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        public async Task InitializeAsync()
        {
            await LoadAll();
        }

        private async Task LoadAll()
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<StorageSystemDbContext>();
                try
                {
                    await LoadRoles(context);
                    await LoadStatuses(context);
                    await LoadAzureStorage(context);
                }
                catch (CosmosException ex)
                {
                    Console.WriteLine($"Cosmos DB Error: {ex.StatusCode} - {ex.Message}");
                }
            }
        }

        private async Task LoadRoles(StorageSystemDbContext context)
        {
            if (!(await context.Roles.AnyAsync(x => x.Name == "User")))
            {
                await context.Roles.AddAsync(new Role() { Name = "User" });
                await context.SaveChangesAsync();
            }
        }

        private async Task LoadAzureStorage(StorageSystemDbContext context)
        {
            if (!(await context.AzureStorages.AnyAsync(x => x.Name == "AZURE STORAGE")))
            {
                await context.AzureStorages.AddAsync(new AzureStorage()
                {
                    ConnectionString = Environment.GetEnvironmentVariable("AZURE_STORAGE_CONNECTION_STRING"),
                    Name = "AZURE STORAGE"
                });
                await context.SaveChangesAsync();
            }
        }

        private async Task LoadStatuses(StorageSystemDbContext context)
        {
            if (!(await context.Statuses.AnyAsync(x => x.Name == "Loaded")))
            {
                await context.Statuses.AddRangeAsync(new[]
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
                try
                {
                    if (!await context.QueueClients.AnyAsync(client => client.Name == queueName))
                    {
                        int azId = await context.AzureStorages.Select(x => x.Id).FirstAsync();
                        context.QueueClients.Add(new QueueClient()
                        {
                            AzureStorageId = azId,
                            Name = queueName
                        });
                        await context.SaveChangesAsync();
                    }
                }
                catch (CosmosException ex)
                {
                    Console.WriteLine($"Error adding Queue: {ex.StatusCode} - {ex.Message}");
                }
            }
        }

        public async Task AddBlobContainerIfNotExists(string blobContainerName, int userId)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<StorageSystemDbContext>();
                try
                {
                    if (!await context.BlobContainers.AnyAsync(client => client.Name == blobContainerName))
                    {
                        int azId = await context.AzureStorages.Select(x => x.Id).FirstAsync();
                        context.BlobContainers.Add(new BlobContainer()
                        {
                            AzureStorageId = azId,
                            Name = blobContainerName,
                            UserId = userId
                        });
                        await context.SaveChangesAsync();
                    }
                }
                catch (CosmosException ex)
                {
                    Console.WriteLine($"Error adding Blob Container: {ex.StatusCode} - {ex.Message}");
                }
            }
        }

        public async Task AddMessageToQueue(string queueName, string message, string messageId, int fileId)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<StorageSystemDbContext>();
                try
                {
                    QueueClient client = await context.QueueClients.FirstAsync(x => x.Name == queueName);
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
                catch (CosmosException ex)
                {
                    Console.WriteLine($"Error adding Message to Queue: {ex.StatusCode} - {ex.Message}");
                }
            }
        }

        public async Task<Database.File> AddFileToDb(string extension, string fileName, string fullPath, int statusId, int userId)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<StorageSystemDbContext>();
                try
                {
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
                catch (CosmosException ex)
                {
                    Console.WriteLine($"Error adding File to DB: {ex.StatusCode} - {ex.Message}");
                    return null;
                }
            }
        }

        public async Task<Database.File> TransferToQueueStorage(int fileId)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<StorageSystemDbContext>();
                try
                {
                    Database.File file = await context.Files.FirstAsync(x => x.Id == fileId);
                    context.QueueItems.First(x => x.FileId == fileId).QueueClientId = await context.QueueClients.Where(x => x.Name == "files-queue-sent").Select(x => x.Id).FirstAsync();
                    await context.SaveChangesAsync();
                    file.StatusId = await GetIdOfStatus("Sent");
                    await context.SaveChangesAsync();
                    return file;
                }
                catch (CosmosException ex)
                {
                    Console.WriteLine($"Error transferring file to Queue Storage: {ex.StatusCode} - {ex.Message}");
                    return null;
                }
            }
        }

        public async Task TransferToBlobStorage(int fileId, string containerName)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<StorageSystemDbContext>();
                try
                {
                    Database.File file = await context.Files.FirstAsync(x => x.Id == fileId);
                    context.QueueItems.Remove(await context.QueueItems.FirstAsync(x => x.FileId == fileId));
                    await context.SaveChangesAsync();
                    int contId = await context.BlobContainers.Where(x => x.Name == containerName).Select(x => x.Id).FirstAsync();
                    context.BlobFiles.Add(new BlobFile()
                    {
                        FileId = fileId,
                        BlobContainerId = contId,
                        CreatedAt = DateTime.UtcNow,
                        Name = file.FileName
                    });
                    await context.SaveChangesAsync();
                }
                catch (CosmosException ex)
                {
                    Console.WriteLine($"Error transferring file to Blob Storage: {ex.StatusCode} - {ex.Message}");
                }
            }
        }

        public async Task ChangeMessageId(int fileId, string messageId)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<StorageSystemDbContext>();
                try
                {
                    context.QueueItems.First(x => x.FileId == fileId).MessageId = messageId;
                    await context.SaveChangesAsync();
                }
                catch (CosmosException ex)
                {
                    Console.WriteLine($"Error changing Message ID: {ex.StatusCode} - {ex.Message}");
                }
            }
        }

        public async Task<int> GetIdOfStatus(string statusName)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<StorageSystemDbContext>();
                try
                {
                    return await context.Statuses.Where(x => x.Name == statusName).Select(x => x.Id).FirstAsync();
                }
                catch (CosmosException ex)
                {
                    Console.WriteLine($"Error getting Status ID: {ex.StatusCode} - {ex.Message}");
                    return 0;
                }
            }
        }

        public async Task<bool> ExistsUser(string login)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<StorageSystemDbContext>();
                try
                {
                    return await context.LoginPasswords.AnyAsync(x => x.Username == login);
                }
                catch (CosmosException ex)
                {
                    Console.WriteLine($"Error checking if user exists: {ex.StatusCode} - {ex.Message}");
                    return false;
                }
            }
        }

        public async Task<AccountModel?> CheckAuthorization(string login, string password)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<StorageSystemDbContext>();
                try
                {
                    if (!await context.LoginPasswords.AnyAsync(x => x.Username == login && x.Password == password))
                        return null;
                    LoginPassword lp = await context.LoginPasswords.FirstAsync(x => x.Username == login && x.Password == password);
                    Database.User user = await context.Users.FirstAsync(x => x.LoginPasswordId == lp.Id);
                    Role role = await context.Roles.FirstAsync(x => x.Id == user.RoleId);
                    return new AccountModel()
                    {
                        Id = user.Id,
                        Role = role.Name
                    };
                }
                catch (CosmosException ex)
                {
                    Console.WriteLine($"Error during authorization: {ex.StatusCode} - {ex.Message}");
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
                    if (await ExistsUser(login))
                        throw new Exception("Login already exists");
                    context.LoginPasswords.Add(lp);
                    await context.SaveChangesAsync();
                    int roleId = await context.Roles.Select(x => x.Id).FirstAsync();
                    context.Users.Add(new Database.User()
                    {
                        LoginPasswordId = lp.Id,
                        Firstname = firstname,
                        Lastname = lastname,
                        RoleId = roleId
                    });
                    await context.SaveChangesAsync();
                    return true;
                }
                catch (CosmosException ex)
                {
                    Console.WriteLine($"Error adding user: {ex.StatusCode} - {ex.Message}");
                    return false;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                    return false;
                }
            }
        }
    }
}
