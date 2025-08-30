using Microsoft.WindowsAzure.Storage.Queue;
using ServiceDataRepo.Entities;
using ServiceDataRepo.Helpers;
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


        // ToggleAccept -> skuplja emailove -> poziva SendEmails (Notify Servis) -> ubacuje poruke u Queue -> WorkerRole šalje emailove.
        [HttpPost]
        [Route("{questionId}/{answerId}/toggle-accept")]
        public IHttpActionResult ToggleAccept(string questionId, string answerId)
        {
            //ovo ne ovdje -> po uslovu zad u queue ide samo answer ID,
            //NotificationService radi pretragu tabela i salje mejlove
 
            var existing = answerRepo.GetById(questionId, answerId);
            if (existing == null)
                return NotFound();

            var questionRepo = new QuestionTableRepository();
            var question = questionRepo.GetById("Question", questionId);
            if (question == null)
                return NotFound();

            if (existing.IsAccepted)
            {
                // Unaccept
                existing.IsAccepted = false;
                question.IsThemeOpen = true;
            }
            else
            {
                // Accept
                existing.IsAccepted = true;
                question.IsThemeOpen = false;

                CloudQueue queue = QueueHelper.GetQueueReference("admin-notifications-queue");
                queue.AddMessage(new CloudQueueMessage(answerId));

                /*
                 * Dakle ovaj dio ne ovdjeee

                // 1. Pronađi sve korisnike koji su odgovorili
                var allAnswers = answerRepo.GetAll(questionId).ToList();
                List<UserEntity> usersToNotify = new List<UserEntity>();

                foreach (var ans in allAnswers)
                {
                    var user = userRepo.GetById("User", ans.Username);
                    if (user != null)
                        usersToNotify.Add(user);
                }

                // 2. Izvuci emailove
                List<string> emails = new List<string>();
                foreach (var u in usersToNotify)
                {
                    if (!string.IsNullOrEmpty(u.Email) && !emails.Contains(u.Email))
                        emails.Add(u.Email);
                }

                // 3. Dodaj poruke u Queue za NotificationService
                if (emails.Any())
                {
                    CloudQueue queue = QueueHelper.GetQueueReference("admin-notifications-queue");

                    string subject = "Answer Accepted!";
                    string body = $"Your answer for question '{question.Title}' has been accepted.";

                    foreach (var email in emails)
                    {
                        string message = $"HEALTHCHECK;Subject:{subject};Email:{email};Body:{body}";
                        queue.AddMessage(new CloudQueueMessage(message));
                    }
                }
                */
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


        /* Slavko implementacija, u slucaju da moje ne radi kako treba
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
                var allAnswers = answerRepo.GetAll(questionId).ToList();
                List<UserEntity> usersToNotify = new List<UserEntity>();
                foreach (var ans in allAnswers)
                {
                    var user = userRepo.GetById("User", ans.Username);
                    usersToNotify.Add(user);
                }

                List<string> emails = new List<string>();
                foreach (var u in usersToNotify)
                {
                    if (u != null && !string.IsNullOrEmpty(u.Email) && !emails.Contains(u.Email))
                        emails.Add(u.Email);
                }

                string AnswerText = existing.Text;

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
        */

    }
}
