using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceDataRepo.Entities
{
    public class HealthCheckEntity : TableEntity
    {
        public HealthCheckEntity() { }

        public HealthCheckEntity(string serviceName, string status)
        {
            PartitionKey = serviceName; // npr. "StackOverflowService" ili "NotificationService"
            RowKey = DateTime.UtcNow.Ticks.ToString();
            Status = status;
            CheckedAt = DateTime.UtcNow;
        }

        public string Status { get; set; } // "OK" or "NOT_OK"
        public DateTime CheckedAt { get; set; }
    }

   
}
