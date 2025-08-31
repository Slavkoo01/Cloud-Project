using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceDataRepo.Entities
{
    public class NotificationEmailSentEntity : TableEntity
    {
        public NotificationEmailSentEntity() { }

        public NotificationEmailSentEntity(Guid answerId, int emailCount)
        {
            PartitionKey = "NotificationEmailSent";
            RowKey = DateTime.UtcNow.Ticks.ToString();
            AnswerId = answerId;
            EmailCount = emailCount;
            SentAt = DateTime.UtcNow;
        }

        public Guid AnswerId { get; set; }
        public int EmailCount { get; set; }
        public DateTime SentAt { get; set; }
    }
}
