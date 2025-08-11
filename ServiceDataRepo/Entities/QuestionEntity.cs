using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceDataRepo.Entities
{
    public class QuestionEntity : TableEntity
    {
        public QuestionEntity() { }

        public QuestionEntity(string userId)
        {
            PartitionKey = "Question";
            RowKey = Guid.NewGuid().ToString();
            UserId = userId;
            CreatedAt = DateTime.UtcNow;
        }

        public string UserId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; }
        public DateTime CreatedAt { get; set; }
    }

}
