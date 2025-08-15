using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;
using Microsoft.WindowsAzure.Storage.Queue.Protocol;
using Newtonsoft.Json;
using System;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using QueueClient = Azure.Storage.Queues.QueueClient;
using QueueMessage = Azure.Storage.Queues.Models.QueueMessage;

namespace ServiceDataRepo.QueueServices
{
    public class NotificationQueueService
    {
        private readonly QueueClient _queueClient;

        private string connectionString = "DataConncetionString";
        private string queueName = "notification";
        public NotificationQueueService(string queueName)
        {
            _queueClient = new QueueClient(connectionString, queueName);
            _queueClient.CreateIfNotExists();
        }

      
        public class NotificationMessage
        {
            public string AnswerId { get; set; }
        }


        public async Task EnqueueNotificationAsync(string answerId)
        {
            var message = new NotificationMessage { AnswerId = answerId };
            string json = System.Text.Json.JsonSerializer.Serialize(message);
            string base64Message = Convert.ToBase64String(Encoding.UTF8.GetBytes(json));

            await _queueClient.SendMessageAsync(base64Message);
        }


        public async Task<NotificationMessage> DequeueNotificationAsync()
        {
            QueueMessage[] messages = await _queueClient.ReceiveMessagesAsync(1);

            if (messages.Length == 0)
                return null;

            var msg = messages[0];
            string json = Encoding.UTF8.GetString(Convert.FromBase64String(msg.MessageText));
            var notificationMessage = System.Text.Json.JsonSerializer.Deserialize<NotificationMessage>(json);


            await _queueClient.DeleteMessageAsync(msg.MessageId, msg.PopReceipt);

            return notificationMessage;
        }
    }
}
