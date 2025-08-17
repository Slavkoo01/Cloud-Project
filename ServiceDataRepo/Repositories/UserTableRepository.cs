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

        public void AddUser(UserEntity newUser)
        {
            TableOperation insertOperation = TableOperation.Insert(newUser);
            _table.Execute(insertOperation);
        }

    }
}
