using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage.Table;
using Microsoft.WindowsAzure.Storage;
using Repositories.Repositories;
using ServiceDataRepo.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceDataRepo.Repositories
{
    public class HealthCheckDataRepository
    {
        private CloudStorageAccount _storageAccount;
        private CloudTable _table;
        public HealthCheckDataRepository()
        {
            _storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("DataConnectionString"));
            CloudTableClient tableClient = new CloudTableClient(new Uri(_storageAccount.TableEndpoint.AbsoluteUri), _storageAccount.Credentials);
            _table = tableClient.GetTableReference("HealthCheckTable");
            _table.CreateIfNotExists();
        }

        public IQueryable<HealthCheckEntity> RetrieveAllHealthCheckEntities()
        {
            var results = from g in _table.CreateQuery<HealthCheckEntity>()
                          where g.PartitionKey == "StackOverflowService"
                          select g;
            return results;
        }

        public IQueryable<HealthCheckEntity> RetrieveAllNotificationkServiceHealthCheckEntities()
        {
            var results = from g in _table.CreateQuery<HealthCheckEntity>()
                          where g.PartitionKey == "NotificationService"
                          select g;
            return results;
        }
        public void AddHealthCheckEntity(HealthCheckEntity newHealthCheck)
        {
            TableOperation insertOperation = TableOperation.Insert(newHealthCheck);
            _table.Execute(insertOperation);
        }
        // izmeniti poslednja 2 chekca tako da uzimaju data samo u poslednja 3 sata
        // 
    }
}
