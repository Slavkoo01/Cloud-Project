using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace StackOverflowService.Models
{
    public class QuestionDto
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Decription { get; set; }
        public string ImageUrl { get; set; }

        public bool IsThemeOpen { get; set; }
        public UserDto User { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<AnswerDto> Answers { get; set; } = new List<AnswerDto>();
    }
}