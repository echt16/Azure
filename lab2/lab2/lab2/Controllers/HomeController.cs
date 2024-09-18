using lab2.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using System.Threading.Tasks;
using System;
using System.IO;

namespace lab2.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            //string? con = Environment.GetEnvironmentVariable("AZURE_STORAGE_CONNECTION_STRING");
            //BlobServiceClient blobServiceClient = new BlobServiceClient(con);
            //string name = "container-for-images" + Guid.NewGuid().ToString();
            //string localPath = "./data/";
            //string fileName = "container-for-images" + Guid.NewGuid().ToString() + ".txt";
            //string localFilePath = Path.Combine(localPath, fileName);
            //await System.IO.File.WriteAllTextAsync(localFilePath, "Hello World");
            //BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(name);

            //if (!await containerClient.ExistsAsync())
            //{
            //    await containerClient.CreateAsync();
            //}

            //BlobClient blobClient = containerClient.GetBlobClient(fileName);

            //string text = "Uploading to Blob storage as blob:\n\t {0}\n" + blobClient.Uri;

            //FileStream fs = System.IO.File.OpenRead(localFilePath);
            //await blobClient.UploadAsync(fs);
            //fs.Close();


            //string downloadPath = localFilePath.Replace(".txt", "DOWNLOAD.txt");

            //text = "\nDownloading blob to\n\t{0}\n" + downloadPath;

            //BlobDownloadInfo dwnld = await blobClient.DownloadAsync();


            //FileStream fs1 = System.IO.File.OpenWrite(downloadPath);

            //await dwnld.Content.CopyToAsync(fs1);

            //fs1.Close();

            //List<string> list = new List<string>() { text };

            return View();
        }

        //[HttpPost]
        //public async Task<IActionResult> addImage(string imagePath)
        //{


        //    if (imagePath == null)
        //    {
        //        return BadRequest("No file uploaded.");
        //    }

        //    if (imagePath.Length == 0)
        //    {
        //        return BadRequest("Empty file.");
        //    }

        //    string connection = Environment.GetEnvironmentVariable("AZURE_STORAGE_CONNECTION_STRING");
        //    BlobServiceClient blobServiceClient = new BlobServiceClient(connection);

        //    string name = "container-for-images";
        //    string fileName = Guid.NewGuid().ToString();

        //    BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(name);
        //    await containerClient.CreateIfNotExistsAsync();

        //    BlobClient blobClient = containerClient.GetBlobClient(fileName);

        //    using (FileStream stream = new FileStream(image, FileMode.Open, FileAccess.Read))
        //    {
        //        await blobClient.UploadAsync(stream);
        //    }

        //    return RedirectToAction("Index");
        //}

        [HttpPost]
        public async Task<IActionResult> addImage(IFormFile image)
        {


            if (image == null)
            {
                return BadRequest("No file uploaded.");
            }

            if (image.Length == 0)
            {
                return BadRequest("Empty file.");
            }

            string connection = Environment.GetEnvironmentVariable("AZURE_STORAGE_CONNECTION_STRING");
            BlobServiceClient blobServiceClient = new BlobServiceClient(connection);

            string name = "container-for-images";
            string fileName = Guid.NewGuid().ToString();

            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(name);
            await containerClient.CreateIfNotExistsAsync();

            BlobClient blobClient = containerClient.GetBlobClient(fileName);

            using (var stream = image.OpenReadStream())
            {
                await blobClient.UploadAsync(stream);
            }

            return RedirectToAction("Index");
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
