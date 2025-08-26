using ServiceDataRepo.Entities;
using ServiceDataRepo.Repositories;
using StackOverflowService.Models;
using System.Web.Http;

namespace StackOverflowService.Controllers
{
    [RoutePrefix("api/votes")]
    public class VoteController : ApiController
    {
        private readonly AnswerTableRepository answerRepo = new AnswerTableRepository();
        private readonly VoteTableRepository voteRepo = new VoteTableRepository();

        [HttpPost]
        [Route("{questionId}/{answerId}/")]
        public IHttpActionResult Vote(string questionId, string answerId, [FromBody] VoteDto voteDto)
        {
            var answer = answerRepo.GetById(questionId, answerId);
            if (answer == null)
                return NotFound();

            // check if user already voted on this answer
            var existingVote = voteRepo.GetById(answerId, voteDto.Username);

            if (existingVote == null)
            {
                // no previous vote -> create new one
                answer.VoteCount += voteDto.Vote;
                answerRepo.Update(answer);

                VoteEntity newVote = new VoteEntity
                {
                    PartitionKey = answerId,
                    RowKey = voteDto.Username,
                    Value = voteDto.Vote
                };

                voteRepo.Insert(newVote);
            }
            else
            {
                // user already voted -> adjust count
                if (existingVote.Value != voteDto.Vote)
                {
                    // remove old vote and apply new one
                    answer.VoteCount -= existingVote.Value;
                    answer.VoteCount += voteDto.Vote;
                    answerRepo.Update(answer);

                    existingVote.Value = voteDto.Vote;
                    voteRepo.Update(existingVote);
                }
                // else -> same vote as before, do nothing
            }

            return Ok(new
            {
                Id = answer.RowKey,
                VoteCount = answer.VoteCount,
                UserVote = voteDto.Vote
            });
        }
    }
}
