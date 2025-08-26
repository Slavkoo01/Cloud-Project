using ServiceDataRepo.Entities;
using ServiceDataRepo.Repositories;
using StackOverflowService.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Web.Http;

namespace StackOverflowService.Controllers
{
    [RoutePrefix("api/answers")]
    public class AnswerController : ApiController
    {
        private readonly AnswerTableRepository answerRepo = new AnswerTableRepository();
        private readonly VoteTableRepository voteRepo = new VoteTableRepository();
        private readonly QuestionTableRepository questionRepo = new QuestionTableRepository();
        private readonly UserTableRepository userRepo = new UserTableRepository();

        [HttpGet]
        [Route("by-question/{questionId}")]
        public IHttpActionResult GetByQuestion(string questionId)
        {
            var answers = answerRepo.GetAll(questionId).ToList();
            return Ok(answers);
        }

        [HttpPost]
        [Route("{questionId}")]
        public IHttpActionResult Create(string questionId, AnswerEntity req)
        {
            if (req == null || string.IsNullOrWhiteSpace(req.Text))
                return BadRequest("Answer text required");

            req.PartitionKey = questionId;
            req.RowKey = Guid.NewGuid().ToString();
            req.CreatedAt = DateTime.UtcNow;
            req.VoteCount = 0;
            req.IsAccepted = false;

            answerRepo.Insert(req);
            return Ok(req);
        }

        [HttpPut]
        [Route("{questionId}/{answerId}")]
        public IHttpActionResult Update(string questionId, string answerId, AnswerEntity req)
        {
            var existing = answerRepo.GetById(questionId, answerId);
            if (existing == null)
                return NotFound();

            existing.Text = req.Text;
            answerRepo.Update(existing);

          
            return Ok(new {
                Id = existing.RowKey,
                Text = existing.Text
            });
        }

        [HttpDelete]
        [Route("{questionId}/{answerId}")]
        public IHttpActionResult Delete(string questionId, string answerId)
        {
            var existing = answerRepo.GetById(questionId, answerId);
            if (existing == null)
                return NotFound();

            
            var votes = voteRepo.GetAll(answerId).ToList();
            foreach (var v in votes)
                voteRepo.Delete(v);

            var question = new QuestionTableRepository().GetById("Question", questionId);
            // If accepted -> reopen question
            if (existing.IsAccepted)
            {
                if (question != null)
                {
                    question.IsThemeOpen = true;
                    questionRepo.Update(question);
                }
            }

            answerRepo.Delete(existing);

            return Ok(new
            {
                IsThemeOpen = question.IsThemeOpen 
            });

        }

        [HttpPost]
        [Route("{questionId}/{answerId}/toggle-accept")]
        public IHttpActionResult ToggleAccept(string questionId, string answerId)
        {
            var existing = answerRepo.GetById(questionId, answerId);
            if (existing == null)
                return NotFound();

            // get related question
            var questionRepo = new QuestionTableRepository();
            var question = questionRepo.GetById("Question", questionId);
            if (question == null)
                return NotFound();

            if (existing.IsAccepted)
            {
                //TODO when answer is accepted it needs to send email to all of the people who answerd before 
                //you will search all ansers based on question ID and get their email

                // Unaccept
                existing.IsAccepted = false;
                question.IsThemeOpen = true;
            }
            else
            {
                // Accept
                existing.IsAccepted = true;

                // Close the discussion
                question.IsThemeOpen = false;

            }
           

            answerRepo.Update(existing);
            questionRepo.Update(question);

            return Ok(new
            {
                AnswerId = existing.RowKey,
                IsAccepted = existing.IsAccepted,
                IsThemeOpen = question.IsThemeOpen
            });
        }


    }
}
