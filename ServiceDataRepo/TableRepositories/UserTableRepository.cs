using Repositories.TableRepositories;
using ServiceDataRepo.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceDataRepo.TableRepositories
{
    public class UserTableRepository : BaseTableRepository<UserEntity>
    {
        public UserTableRepository()
            : base("Users") { }
    }
}
