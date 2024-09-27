using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;
using lab3_1.Models.ViewModels;

namespace lab3_1.Models.Services.AzureServices
{
    public class QueueService : StorageService
    {
        private static string QueueName { get; set; }
        private string queueName;

        public QueueService(string section)
        {
            queueName = QueueName + section;
        }

        static QueueService()
        {
            QueueName = "files-queue-";
        }

        public void SetQueueName(string name)
        {
            QueueName = name;
        }

        private async Task<QueueClient> GetQueue()
        {
            try
            {
                QueueServiceClient serviceClient = new QueueServiceClient(StorageKey);
                QueueClient queueClient = serviceClient.GetQueueClient(queueName);
                await queueClient.CreateIfNotExistsAsync();
                return queueClient;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Ошибка при получении клиента очереди", ex);
            }
        }

        public async Task<string> SendMessage(string message)
        {
            try
            {
                QueueClient queueClient = await GetQueue();
                SendReceipt receipt = await queueClient.SendMessageAsync(message);
                return receipt.MessageId;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Ошибка при отправке сообщения в очередь", ex);
            }
        }

        public async Task<string> SendMessage(string message, int time)
        {
            try
            {
                QueueClient queueClient = await GetQueue();
                SendReceipt sr = await queueClient.SendMessageAsync(message, TimeSpan.FromHours(time));
                return sr.MessageId;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Ошибка при отправке сообщения с задержкой {time} секунд", ex);
            }
        }

        public async Task DeleteQueue()
        {
            try
            {
                QueueClient queueClient = await GetQueue();
                await queueClient.DeleteAsync();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Ошибка при удалении очереди", ex);
            }
        }

        public async Task<List<MessageModelView>> GetMessages(int count)
        {
            try
            {
                List<MessageModelView> messages = new List<MessageModelView>();
                QueueClient queueClient = await GetQueue();

                count = queueClient.MaxPeekableMessages < count ? queueClient.MaxPeekableMessages : count;

                foreach (PeekedMessage message in (await queueClient.PeekMessagesAsync(count)).Value)
                {
                    messages.Add(new MessageModelView()
                    {
                        MessageId = message.MessageId,
                        MessageText = message.MessageText
                    });
                }

                return messages;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Ошибка при получении {count} сообщений из очереди", ex);
            }
        }

        public async Task DeleteMessage(string messageId)
        {
            try
            {
                QueueClient queueClient = await GetQueue();

                foreach (QueueMessage message in (await queueClient.ReceiveMessagesAsync()).Value)
                {
                    if (message.MessageId == messageId)
                    {
                        await queueClient.DeleteMessageAsync(message.MessageId, message.PopReceipt);
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Ошибка при удалении сообщения с ID: {messageId}", ex);
            }
        }

        public string GetQueueName()
        {
            return queueName;
        }
    }
}
