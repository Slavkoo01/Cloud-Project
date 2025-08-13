using Repositories.TableRepositories;
using ServiceDataRepo.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceDataRepo.TableRepositories
{
    public class HealthCheckTableRepository : BaseTableRepository<HealthCheckEntity>
    {
        public HealthCheckTableRepository(string connectionString)
            : base("HealthChecks") { }
    }

}
