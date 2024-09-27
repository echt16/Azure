using System;

namespace lab3_1.Models.Services.AzureServices
{
    public class StorageService
    {
        protected static string StorageKey { get; set; }

        static StorageService()
        {
            StorageKey = Environment.GetEnvironmentVariable("AZURE_STORAGE_CONNECTION_STRING") ?? "";
        }
    }
}
