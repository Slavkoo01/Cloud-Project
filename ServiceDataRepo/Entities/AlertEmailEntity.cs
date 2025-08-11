using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceDataRepo.Entities
{
    public class AlertEmailEntity : TableEntity
    {
        public AlertEmailEntity() { }

        public AlertEmailEntity(string email)
        {
            PartitionKey = "AlertEmail";
            RowKey = Guid.NewGuid().ToString();
            Email = email;
        }

        public string Email { get; set; }
    }
}
