using Repositories.TableRepositories;
using ServiceDataRepo.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceDataRepo.TableRepositories
{
    public class AnswerTableRepository : BaseTableRepository<AnswerEntity>
    {
        public AnswerTableRepository(string connectionString)
            : base("Answers") { }
    }
}
