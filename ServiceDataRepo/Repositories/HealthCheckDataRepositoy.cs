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

        // StackOverflowService health checks (samo poslednja 3h)
        public IQueryable<HealthCheckEntity> RetrieveAllHealthCheckEntities()
        {
            var threeHoursAgo = DateTime.UtcNow.AddHours(-3);

            var results = from g in _table.CreateQuery<HealthCheckEntity>()
                          where g.PartitionKey == "StackOverflowService"
                             && g.CheckedAt >= threeHoursAgo
                          select g;
            return results;
        }

        // NotificationService health checks (samo poslednja 3h)
        public IQueryable<HealthCheckEntity> RetrieveAllNotificationkServiceHealthCheckEntities()
        {
            var threeHoursAgo = DateTime.UtcNow.AddHours(-3);

            var results = from g in _table.CreateQuery<HealthCheckEntity>()
                          where g.PartitionKey == "NotificationService"
                             && g.CheckedAt >= threeHoursAgo
                          select g;
            return results;
        }

        public void AddHealthCheckEntity(HealthCheckEntity newHealthCheck)
        {
            TableOperation insertOperation = TableOperation.Insert(newHealthCheck);
            _table.Execute(insertOperation);
        }
    }
}
