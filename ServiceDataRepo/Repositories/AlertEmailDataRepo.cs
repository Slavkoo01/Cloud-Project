using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage.Table;
using Microsoft.WindowsAzure.Storage;
using ServiceDataRepo.Entities;

namespace ServiceDataRepo.Repositories
{
    public class AlertEmailDataRepo
    {
        // metode po uzoru na zad sa zadnjih vj
        private CloudStorageAccount _storageAccount;
        private CloudTable _table;
        public AlertEmailDataRepo()
        {
            _storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("DataConnectionString"));
            CloudTableClient tableClient = new CloudTableClient(new Uri(_storageAccount.TableEndpoint.AbsoluteUri), _storageAccount.Credentials);
            _table = tableClient.GetTableReference("AlertEmailTable");
            _table.CreateIfNotExists();
        }

        public IQueryable<AlertEmailEntity> RetrieveAllAlertEmails()
        {
            var results = from g in _table.CreateQuery<AlertEmailEntity>()
                          where g.PartitionKey == "AlertEmail"
                          select g;
            return results;
        }

        public void UpdateAlertEmail(string oldEmail, AlertEmailEntity email)
        {
            AlertEmailEntity emailToUpdate = RetrieveAllAlertEmails().Where(s => s.Email == oldEmail).FirstOrDefault();

            if (emailToUpdate != null)
            {
                emailToUpdate.Email = email.Email;
                TableOperation updateOperation = TableOperation.Replace(emailToUpdate);
                _table.Execute(updateOperation);
            }  
        }

        public void AddAlertEmail(AlertEmailEntity newAlertEmail)
        {
            TableOperation insertOperation = TableOperation.Insert(newAlertEmail);
            _table.Execute(insertOperation);
        }

        public void RemoveAlertEmail(string id)
        {
            AlertEmailEntity email = RetrieveAllAlertEmails().Where(s => s.Email == id).FirstOrDefault();

            if (email != null)
            {
                TableOperation deleteOperation = TableOperation.Delete(email);
                _table.Execute(deleteOperation);
            }
        }
    }
}
