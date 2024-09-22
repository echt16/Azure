using lab3_1.Models.Database;
using lab3_1.Models.ViewModels;

namespace lab3_1.Models.Services.DatabaseServices
{
    public class DatabaseService
    {
        public StorageSystemDbContext Context { get; private set; }
        public DatabaseService(StorageSystemDbContext db)
        {
            Context = db;
            _ = LoadAll();
        }

        private async Task LoadAll()
        {
            await LoadRoles();
            await LoadStatuses();
            await LoadAzureStorage();
        }

        private async Task LoadAzureStorage()
        {
            Context.AzureStorages.Add(new AzureStorage()
            {
                ConnectionString = Environment.GetEnvironmentVariable("AZURE_STORAGE_CONNECTION_STRING"),
                Name = "AZURE STORAGE"
            });
            await Context.SaveChangesAsync();
        }
        private async Task LoadRoles()
        {
            Context.Roles.Add(new Role() { Name = "User" });
            Context.SaveChanges();
        }

        private async Task LoadStatuses()
        {
            Context.Statuses.AddRange([
                new Status()
                {
                    Name = "Loaded"
                },
                    new Status()
                {
                    Name = "Sent"
                }
            ]);
            Context.SaveChanges();
        }

        public async Task AddQueueIfNotExists(string queueName)
        {
            if (!Context.QueueClients.Any(client => client.Name == queueName))
            {
                int azId = Context.AzureStorages.First().Id;
                Context.QueueClients.Add(new QueueClient()
                {
                    AzureStorageId = azId,
                    Name = queueName
                });
                await Context.SaveChangesAsync();
            }
        }

        public async Task AddBlobContainerIfNotExists(string blobContainerName, int userId)
        {
            if (!Context.BlobContainers.Any(client => client.Name == blobContainerName))
            {
                int azId = Context.AzureStorages.First().Id;
                Context.BlobContainers.Add(new BlobContainer()
                {
                    AzureStorageId = azId,
                    Name = blobContainerName,
                    UserId = userId
                });
                await Context.SaveChangesAsync();
            }
        }

        public async Task AddMessageToQueue(string queueName, string message, string messageId, int fileId)
        {
            QueueClient client = Context.QueueClients.First(x => x.Name == queueName);
            QueueItem queueItem = new QueueItem()
            {
                CreatedAt = DateTime.UtcNow,
                FileId = fileId,
                MessageId = messageId,
                MessageText = message,
                QueueClientId = client.Id
            };
            Context.QueueItems.Add(queueItem);
            await Context.SaveChangesAsync();
        }

        public async Task<Database.File> AddFileToDb(string extension, string fileName, string fullPath, int statusId, int userId)
        {
            Database.File file = new Database.File()
            {
                Extension = extension,
                FileName = fileName,
                LocalFullPath = fullPath,
                StatusId = statusId,
                UserId = userId
            };
            Context.Add(file);
            await Context.SaveChangesAsync();
            return file;
        }

        public async Task<Database.File> TransferToQueueStorage(int fileId)
        {
            Database.File file = Context.Files.First(x => x.Id == fileId);
            Context.QueueItems.First(x => x.FileId == fileId).QueueClientId = Context.QueueClients.First(x => x.Name == "files-queue-sent").Id;
            await Context.SaveChangesAsync();
            file.StatusId = GetIdOdStatus("Sent");
            await Context.SaveChangesAsync();
            return file;
        }

        public async Task TransferToBlobStorage(int fileId, string containerName)
        {
            Database.File file = Context.Files.First(x => x.Id == fileId);
            Context.QueueItems.Remove(Context.QueueItems.First(x => x.FileId == fileId));
            await Context.SaveChangesAsync();
            int contId = Context.BlobContainers.First(x => x.Name == containerName).Id;
            Context.BlobFiles.Add(new BlobFile()
            {
                FileId = fileId,
                BlobContainerId = contId,
                CreatedAt = DateTime.UtcNow,
                Name = file.FileName
            });
            await Context.SaveChangesAsync();
        }

        public async Task ChangeMessageId(int fileId, string messageId)
        {
            Context.QueueItems.First(x => x.FileId == fileId).MessageId = messageId;
            await Context.SaveChangesAsync();
        }

        public int GetIdOdStatus(string statusName)
        {
            return Context.Statuses.First(x => x.Name == statusName).Id;
        }


        public bool ExistsUser(string login)
        {
            if (Context.LoginPasswords.Any(x => x.Username == login))
                return true;
            return false;
        }

        public async Task<AccountModel?> CheckAuthorization(string login, string password)
        {
            try
            {
                if (!Context.LoginPasswords.Any(x => x.Username == login && x.Password == password))
                    return null;
                LoginPassword lp = Context.LoginPasswords.First(x => x.Username == login && x.Password == password);
                User user = Context.Users.First(x => x.LoginPasswordId == lp.Id);
                Role role = Context.Roles.First(x => x.Id == user.RoleId);
                return new AccountModel()
                {
                    Id = user.Id,
                    Role = role.Name
                };
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<bool> AddUser(string login, string password, string firstname, string lastname)
        {
            try
            {

                LoginPassword lp = new LoginPassword()
                {
                    Username = login,
                    Password = password
                };
                if (ExistsUser(login))
                    throw new Exception("Login already exists");
                Context.LoginPasswords.Add(lp);
                await Context.SaveChangesAsync();
                int roleId = Context.Roles.First().Id;
                Context.Users.Add(new User()
                {
                    LoginPasswordId = lp.Id,
                    Firstname = firstname,
                    Lastname = lastname,
                    RoleId = roleId
                });
                await Context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}
