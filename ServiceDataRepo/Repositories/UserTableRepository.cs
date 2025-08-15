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

    }
}
