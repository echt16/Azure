using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace lab3_1.Models.Services.AzureServices
{
    public class BlobService : StorageService
    {
        private static string BlobName { get; set; }
        private string privatBlobName;
        private int userId;

        public BlobService(int userId)
        {
            privatBlobName = BlobName + userId.ToString();
        }

        static BlobService()
        {
            BlobName = "files-blob-";
        }

        public int GetUserId()
        {
            return userId;
        }

        private async Task<BlobContainerClient> GetBlobContainer()
        {
            try
            {
                BlobServiceClient blobServiceClient = new BlobServiceClient(StorageKey);
                BlobContainerClient blobContainerClient = blobServiceClient.GetBlobContainerClient(privatBlobName);
                await blobContainerClient.CreateIfNotExistsAsync();
                return blobContainerClient;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Ошибка при открытии контейнера", ex);
            }
        }

        public async Task<string> LoadFile(FileStream fileStream, string fileName)
        {
            try
            {
                string contentType;
                string extension = Path.GetExtension(fileName).ToLower();

                switch (extension)
                {
                    case ".jpg":
                    case ".jpeg":
                        contentType = "image/jpeg";
                        break;
                    case ".png":
                        contentType = "image/png";
                        break;
                    case ".mp4":
                        contentType = "video/mp4";
                        break;
                    case ".avi":
                        contentType = "video/avi";
                        break;
                    case ".pdf":
                        contentType = "application/pdf";
                        break;
                    default:
                        throw new InvalidOperationException("Неподдерживаемый тип файла");
                }

                string uniqueFileName = fileName + Guid.NewGuid().ToString();
                BlobClient blobClient = (await GetBlobContainer()).GetBlobClient(uniqueFileName);

                await blobClient.UploadAsync(fileStream, new BlobHttpHeaders { ContentType = contentType });

                return uniqueFileName;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Ошибка при загрузке файла", ex);
            }
            finally
            {
                fileStream?.Close();
            }
        }

        public async Task<Stream> DownloadImage(string imageName)
        {
            try
            {
                BlobClient blobClient = (await GetBlobContainer()).GetBlobClient(imageName);
                BlobDownloadInfo downloadInfo = await blobClient.DownloadAsync();
                return downloadInfo.Content;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Ошибка при скачивании изображения", ex);
            }
        }

        public async Task<List<Stream>> GetBlobItems()
        {
            List<Stream> streams = new List<Stream>();

            try
            {
                BlobContainerClient blobContainerClient = await GetBlobContainer();

                await foreach (BlobItem block in blobContainerClient.GetBlobsAsync())
                {
                    try
                    {
                        BlobClient blobClient = blobContainerClient.GetBlobClient(block.Name);
                        BlobDownloadInfo downloadInfo = await blobClient.DownloadAsync();
                        streams.Add(downloadInfo.Content);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Ошибка при скачивании Blob '{block.Name}': {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Ошибка при получении списка Blob-элементов", ex);
            }

            return streams;
        }

        public async Task<List<string>> GetBlobItemsName()
        {
            List<string> names = new List<string>();

            try
            {
                BlobContainerClient blobContainerClient = await GetBlobContainer();

                await foreach (BlobItem block in blobContainerClient.GetBlobsAsync())
                {
                    try
                    {
                        names.Add(block.Name);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Ошибка при скачивании Blob '{block.Name}': {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Ошибка при получении списка Blob-элементов", ex);
            }

            return names;
        }

        public string GetBlobName()
        {
            return privatBlobName;
        }
    }
}
