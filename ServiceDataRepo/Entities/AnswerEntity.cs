using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceDataRepo.Entities
{
    public class AnswerEntity : TableEntity
    {
        public AnswerEntity() { }

        public AnswerEntity(string questionId, string userId)
        {
            PartitionKey = questionId; //To Group answeres by the question (it made sense idk like pull everything from the same partition)
            RowKey = Guid.NewGuid().ToString();
            QuestionId = questionId;
            UserId = userId;
            CreatedAt = DateTime.UtcNow;
            VoteCount = 0;
            IsAccepted = false;
        }

        public string QuestionId { get; set; }
        public string UserId { get; set; }
        public string Text { get; set; }
        public int VoteCount { get; set; }
        public bool IsAccepted { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
