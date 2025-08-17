using ServiceDataRepo.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace StackOverflowService.Controllers
{
    [RoutePrefix("api/votes")]
    public class VoteController : ApiController
    {
        private readonly AnswerTableRepository answerRepo = new AnswerTableRepository();

        [HttpPost]
        [Route("{questionId}/{answerId}")]
        public IHttpActionResult Vote(string questionId, string answerId, [FromBody] bool isUpvote)
        {
            var answer = answerRepo.GetById(questionId, answerId);
            if (answer == null)
                return NotFound();

            answer.VoteCount += isUpvote ? 1 : -1;
            answerRepo.Update(answer);

            return Ok(answer);
        }
    }
}
