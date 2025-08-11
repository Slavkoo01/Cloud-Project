using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceDataRepo.Entities
{
    public class NotificationLogEntity : TableEntity
    {
        public NotificationLogEntity() { }

        public NotificationLogEntity(string answerId)
        {
            PartitionKey = "NotificationLog";
            RowKey = Guid.NewGuid().ToString();
            AnswerId = answerId;
            SentAt = DateTime.UtcNow;
        }

        public string AnswerId { get; set; }
        public int EmailsSentCount { get; set; }
        public DateTime SentAt { get; set; }
    }
}
