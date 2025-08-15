using Microsoft.WindowsAzure.Storage.Table;
using Repositories.Repositories;
using ServiceDataRepo.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceDataRepo.Repositories
{
    public class UserTableRepository : BaseRepository<UserEntity>
    {
        public UserTableRepository() : base("UsersTable") {}

        public UserEntity GetUserByUsername(string username)
        {
            var usernameFilter = TableQuery.GenerateFilterCondition("Username", QueryComparisons.Equal, username);
            var query = new TableQuery<UserEntity>().Where(usernameFilter);

            var results = _table.ExecuteQuery(query);
            return results.FirstOrDefault();
        }
    }
}
