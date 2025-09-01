using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Microsoft.Azure; 
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Repositories.Repositories
{
    public class BaseRepository<T> where T : TableEntity, new()
    {
        public readonly CloudTable _table;
        private string connectionString = "DataConnectionString";
        private string configurationSetting = "UseDevelopmentStorage=true"; //I don't know man config manager doesnt work for me
        private CloudStorageAccount storageAccount;
        public BaseRepository(string tableName)
        {

            storageAccount =
            CloudStorageAccount.Parse(configurationSetting);
            CloudTableClient tableClient = new CloudTableClient(new
            Uri(storageAccount.TableEndpoint.AbsoluteUri), storageAccount.Credentials);
            
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
            Console.WriteLine(partitionKey);
            Console.WriteLine(rowKey);
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
