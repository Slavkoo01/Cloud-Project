using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Microsoft.Azure; 
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Repositories.TableRepositories
{
    public class BaseTableRepository<T> where T : TableEntity, new()
    {
        private readonly CloudTable _table;

        private string connectionString = "DataConnectionString";
        public BaseTableRepository(string tableName)
        {
            var storageAccount = CloudStorageAccount.Parse(
                CloudConfigurationManager.GetSetting(connectionString));

            var tableClient = storageAccount.CreateCloudTableClient();
            _table = tableClient.GetTableReference(tableName);
            _table.CreateIfNotExists();
        }

        public IQueryable<T> GetAll(string partitionKey = null)
        {
            var query = _table.CreateQuery<T>();

            if (!string.IsNullOrEmpty(partitionKey))
            {
                query = (TableQuery<T>)query.Where(e => e.PartitionKey == partitionKey);
            }

            return query;
        }

        public T GetById(string partitionKey, string rowKey)
        {
            var retrieveOperation = TableOperation.Retrieve<T>(partitionKey, rowKey);
            var result = _table.Execute(retrieveOperation);
            return result.Result as T;
        }

        public void Insert(T entity)
        {
            var insertOperation = TableOperation.Insert(entity);
            _table.Execute(insertOperation);
        }

        public void Update(T entity)
        {
            var updateOperation = TableOperation.Replace(entity);
            _table.Execute(updateOperation);
        }

        public void Delete(T entity)
        {
            var deleteOperation = TableOperation.Delete(entity);
            _table.Execute(deleteOperation);
        }
    }
}
