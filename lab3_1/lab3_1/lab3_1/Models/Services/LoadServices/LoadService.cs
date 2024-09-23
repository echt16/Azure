using lab3_1.Models.Services.AzureServices;
using lab3_1.Models.Services.DatabaseServices;
using lab3_1.Models.Database;
using Newtonsoft.Json;
using System.Drawing;
using System.Drawing.Imaging;
using Xabe.FFmpeg;
using lab3_1.Models.ViewModels;

namespace lab3_1.Models.Services.LoadServices
{
    public class LoadService
    {
        private int UserId { get; set; }
        private string LocalDirectory { get; set; }
        private string LoadQueueName { get; set; }
        private string SentQueueName { get; set; }
        private string PrivateBlobName { get; set; }
        private BlobService BlobService { get; set; }
        private QueueService LoadQueueService { get; set; }
        private QueueService SentQueueService { get; set; }
        private DatabaseService DatabaseService { get; set; }

        public LoadService(int userId, DatabaseService databaseService)
        {
            BlobService = new BlobService(userId);
            LoadQueueService = new QueueService("loaded");
            SentQueueService = new QueueService("sent");

            DatabaseService = databaseService;

            _ = DatabaseService.AddBlobContainerIfNotExists(BlobService.GetBlobName(), userId);
            _ = DatabaseService.AddQueueIfNotExists(LoadQueueService.GetQueueName());
            _ = DatabaseService.AddQueueIfNotExists(SentQueueService.GetQueueName());

            LoadQueueName = LoadQueueService.GetQueueName();
            SentQueueName = SentQueueService.GetQueueName();
            PrivateBlobName = BlobService.GetBlobName();

            LocalDirectory = "./data";

            if (!Directory.Exists(LocalDirectory))
            {
                Directory.CreateDirectory(LocalDirectory);
            }

            UserId = userId;
        }

        public async Task LoadToLocalPath(FileStream file, string filePath)
        {
            string fileName = LoadQueueName + Guid.NewGuid().ToString() + Path.GetExtension(filePath);
            string fullPath = Path.Combine(LocalDirectory, fileName);
            using (FileStream fs = new FileStream(fullPath, FileMode.CreateNew, FileAccess.Write))
            {
                await file.CopyToAsync(fs);
            }

            int statusId = DatabaseService.GetIdOfStatus("Loaded");

            Database.File f = await DatabaseService.AddFileToDb(Path.GetExtension(fullPath), fileName, fullPath, statusId, UserId);

            string messageText = JsonConvert.SerializeObject(f);
            string messageId = await LoadQueueService.SendMessage(messageText, 24);

            await DatabaseService.AddMessageToQueue(LoadQueueName, messageText, messageId, f.Id);
        }

        public async Task SentQueueProcessing()
        {
            while (true)
            {
                List<MessageModelView> list = await SentQueueService.GetMessages(10);
                foreach (var file in list)
                {
                    Database.File tmp = JsonConvert.DeserializeObject<Database.File>(file.MessageText);

                    using (FileStream fs = new FileStream(tmp.LocalFullPath, FileMode.Open, FileAccess.Read))
                    {
                        await BlobService.LoadFile(fs, tmp.LocalFullPath);
                    }

                    await SentQueueService.DeleteMessage(file.MessageId);

                    await DatabaseService.TransferToBlobStorage(tmp.Id, BlobService.GetBlobName());
                }
            }
        }

        public async Task SendAllToStorage()
        {
            List<MessageModelView> list = await LoadQueueService.GetMessages(100);
            foreach (var file in list)
            {
                Database.File tmp = JsonConvert.DeserializeObject<Database.File>(file.MessageText);

                await LoadQueueService.DeleteMessage(file.MessageId);

                Database.File nFile = await DatabaseService.TransferToQueueStorage(tmp.Id);

                string id = await SentQueueService.SendMessage(JsonConvert.SerializeObject(nFile), 24);

                await DatabaseService.ChangeMessageId(nFile.Id, id);
            }
        }

        private async Task<string> CreateThumbnail(Database.File file)
        {
            string inputPath = file.FileName;
            string thumbnailPath = Path.Combine(LocalDirectory, "thumbnails", Path.GetFileNameWithoutExtension(file.FileName) + "_thumb.jpg");

            using (Image image = Image.FromFile(inputPath))
            {
                Image thumbnail = image.GetThumbnailImage(100, 100, () => false, IntPtr.Zero);
                thumbnail.Save(thumbnailPath, ImageFormat.Jpeg);
            }

            return thumbnailPath;
        }

        private async Task<string> ConvertVideo(Database.File file, string extensionTo)
        {
            string inputPath = Path.Combine(LocalDirectory, file.FileName);
            string outputPath = Path.Combine(LocalDirectory, Path.GetFileNameWithoutExtension(file.FileName) + extensionTo);

            await FFmpeg.Conversions.FromSnippet.Convert(inputPath, outputPath);

            return outputPath;
        }
    }
}
