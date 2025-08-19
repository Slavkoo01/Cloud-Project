using ServiceDataRepo.Entities;
using ServiceDataRepo.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace StackOverflowService.Controllers
{
    [RoutePrefix("api/answers")]
    public class AnswerController : ApiController
    {
        private readonly AnswerTableRepository answerRepo = new AnswerTableRepository();

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
            return Ok(existing);
        }

        [HttpDelete]
        [Route("{questionId}/{answerId}")]
        public IHttpActionResult Delete(string questionId, string answerId)
        {
            var existing = answerRepo.GetById(questionId, answerId);
            if (existing == null)
                return NotFound();

            answerRepo.Delete(existing);
            return Ok();
        }

        [HttpPost]
        [Route("{questionId}/{answerId}/accept")]
        public IHttpActionResult AcceptAnswer(string questionId, string answerId)
        {
            var existing = answerRepo.GetById(questionId, answerId);
            if (existing == null)
                return NotFound();

            existing.IsAccepted = true;
            answerRepo.Update(existing);

            // TODO: send message to Notification queue here

            return Ok(existing);
        }
    }
}
