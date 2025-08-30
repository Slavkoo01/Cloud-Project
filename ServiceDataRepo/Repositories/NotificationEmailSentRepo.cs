using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using ServiceDataRepo.Entities;
using System;
using System.Linq;

namespace ServiceDataRepo.Repositories
{
    public class NotificationEmailSentRepo
    {
        private CloudStorageAccount _storageAccount;
        private CloudTable _table;

        public NotificationEmailSentRepo()
        {
            _storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("DataConnectionString"));
            CloudTableClient tableClient = new CloudTableClient(new Uri(_storageAccount.TableEndpoint.AbsoluteUri), _storageAccount.Credentials);
            _table = tableClient.GetTableReference("NotificationEmailSentTable");
            _table.CreateIfNotExists();
        }

        public IQueryable<NotificationEmailSentEntity> RetrieveAllNotificationEmailsSent()
        {
            var results = from g in _table.CreateQuery<NotificationEmailSentEntity>()
                          where g.PartitionKey == "NotificationEmailSent"
                          select g;
            return results;
        }

        public void AddNotificationEmailSent(NotificationEmailSentEntity entity)
        {
            TableOperation insertOperation = TableOperation.Insert(entity);
            _table.Execute(insertOperation);
        }

        public IQueryable<NotificationEmailSentEntity> GetNotificationsByAnswerId(Guid answerId)
        {
            var results = from g in _table.CreateQuery<NotificationEmailSentEntity>()
                          where g.PartitionKey == "NotificationEmailSent"
                             && g.AnswerId == answerId
                          select g;
            return results;
        }
    }
}