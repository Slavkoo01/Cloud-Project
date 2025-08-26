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

        public QuestionEntity(string username)
        {
            PartitionKey = "Question";
            RowKey = Guid.NewGuid().ToString();
            Username = username;
            CreatedAt = DateTime.UtcNow;
        }

        public string Username { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; }

        public bool IsThemeOpen { get; set; } = true;
        public DateTime CreatedAt { get; set; }
    }

}
