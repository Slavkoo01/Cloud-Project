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
    [RoutePrefix("api/questions")]
    public class QuestionController : ApiController
    {
        private readonly QuestionTableRepository questionRepo = new QuestionTableRepository();

        [HttpGet]
        [Route("")]
        public IHttpActionResult GetAll(string search = null)
        {
            var questions = questionRepo.GetAll().ToList();

            if (!string.IsNullOrEmpty(search))
            {
                questions = questions
                    .Where(q => q.Title != null && q.Title.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0)
                    .ToList();
            }
            questions = questions.OrderByDescending(q => q.CreatedAt).ToList();

            return Ok(questions);
        }


        [HttpGet]
        [Route("{id}")]
        public IHttpActionResult GetById(string id)
        {
            var q = questionRepo.GetById("Question", id);
            if (q == null)
                return NotFound();
            return Ok(q);
        }

        [HttpPost]
        [Route("")]
        public IHttpActionResult Create(QuestionEntity req)
        {
            if (req == null || string.IsNullOrWhiteSpace(req.Title) || string.IsNullOrWhiteSpace(req.Description))
                return BadRequest("Title and Description are required");

            req.PartitionKey = "Question";
            req.RowKey = Guid.NewGuid().ToString();
            req.CreatedAt = DateTime.UtcNow;
  

            questionRepo.Insert(req);
            return Ok(req);
        }

        [HttpPut]
        [Route("{id}")]
        public IHttpActionResult Update(string id, QuestionEntity req)
        {
            var existing = questionRepo.GetById("Question", id);
            if (existing == null)
                return NotFound();

            existing.Title = req.Title;
            existing.Description = req.Description;
            questionRepo.Update(existing);

            return Ok(existing);
        }

        [HttpDelete]
        [Route("{id}")]
        public IHttpActionResult Delete(string id)
        {
            var existing = questionRepo.GetById("Question", id);
            if (existing == null)
                return NotFound();

            questionRepo.Delete(existing);
            return Ok();
        }
    }
}
