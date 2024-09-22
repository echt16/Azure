using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;
using Newtonsoft.Json;

namespace lab3.Models
{
    public class QueueService
    {
        private static string connectionSrting;

        static QueueService()
        {
            connectionSrting = Environment.GetEnvironmentVariable("AZURE_STORAGE_CONNECTION_STRING");
        }

        public static async Task DeleteQueueAsync(string queueName)
        {
            QueueServiceClient serviceClient = new QueueServiceClient(connectionSrting);
            QueueClient queue = serviceClient.GetQueueClient(queueName);
            await queue.CreateIfNotExistsAsync();
            await queue.DeleteAsync();
        }

        public static async Task SendMessageAsync(string queueName, string message)
        {
            QueueServiceClient serviceClient = new QueueServiceClient(connectionSrting);
            QueueClient queue = serviceClient.GetQueueClient(queueName);
            await queue.CreateIfNotExistsAsync();
            SendReceipt receipt = await queue.SendMessageAsync(message);
        }

        public static async Task SendMessageAsync(string queueName, string message, int TTL)
        {
            QueueServiceClient serviceClient = new QueueServiceClient(connectionSrting);
            QueueClient queue = serviceClient.GetQueueClient(queueName);
            await queue.CreateIfNotExistsAsync();
            await queue.SendMessageAsync(message, timeToLive: TimeSpan.FromSeconds(TTL));
        }

        public static async Task<List<Lot>> PeekMessagesAsync(string queueName, int count)
        {
            List<Lot> lots = new List<Lot>();
            QueueServiceClient serviceClient = new QueueServiceClient(connectionSrting);
            QueueClient queue = serviceClient.GetQueueClient(queueName);
            await queue.CreateIfNotExistsAsync();
            foreach (PeekedMessage message in (await queue.PeekMessagesAsync(maxMessages: count)).Value)
            {
                Lot lot = JsonConvert.DeserializeObject<Lot>(message.MessageText);
                lot.Id = message.MessageId;
                lots.Add(lot);
            }
            return lots;
        }

        public static async Task DeleteMessageAsync(string queueName, string messageId)
        {
            QueueServiceClient serviceClient = new QueueServiceClient(connectionSrting);
            QueueClient queue = serviceClient.GetQueueClient(queueName);
            await queue.CreateIfNotExistsAsync();
            foreach (QueueMessage message in (await queue.ReceiveMessagesAsync(maxMessages: 10)).Value)
            {
                if (message.MessageId == messageId)
                    await queue.DeleteMessageAsync(message.MessageId, message.PopReceipt);
            }
        }
    }
}
