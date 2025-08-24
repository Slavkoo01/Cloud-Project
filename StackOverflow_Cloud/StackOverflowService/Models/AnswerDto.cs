using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace StackOverflowService.Models
{
    public class AnswerDto
    {
        public string Id { get; set; }
        public string Text { get; set; }
        public UserDto User { get; set; } 
        public int Votes { get; set; }

        /// <summary>
        /// 1 = upvoted, -1 = downvoted, 0 = none
        /// </summary>
        /// 
        public int UserVote { get; set; }

        public bool IsAccepted { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}