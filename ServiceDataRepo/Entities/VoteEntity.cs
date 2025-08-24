using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceDataRepo.Entities
{
    public class VoteEntity: TableEntity
    {
        public VoteEntity() { }

        public VoteEntity(string answerId, string username, int value)
        {
            PartitionKey = answerId;  // Group all votes by answer
            RowKey = username;          // one vote per user per answer (in case of multiple it should just overwrite it or be exception which might be a problem :)) )
            AnswerId = answerId;
            Username = username;
            Value = value;        
        }

        public string Username { get; set; }

        public string AnswerId { get; set; }

        public int Value { get; set; } // -1 for downvote, 1 for upvote


    }
}
