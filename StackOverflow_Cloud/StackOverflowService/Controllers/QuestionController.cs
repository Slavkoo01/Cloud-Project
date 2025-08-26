using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage;
using ServiceDataRepo.BlobRepositories;
using ServiceDataRepo.Entities;
using ServiceDataRepo.Repositories;
using StackOverflowService.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace StackOverflowService.Controllers
{
    [RoutePrefix("api/questions")]
    public class QuestionController : ApiController
    {
        private readonly QuestionTableRepository questionRepo = new QuestionTableRepository();
        private readonly ImageBlobStorageRepository blobContainer = new ImageBlobStorageRepository();
        private readonly UserTableRepository usersRepo = new UserTableRepository();
        private readonly VoteTableRepository voteRepo = new VoteTableRepository();
        private readonly AnswerTableRepository answerRepo = new AnswerTableRepository();

        [HttpGet]
        [Route("")]
        public IHttpActionResult GetAll(string search = null, string LoggedInUser = null, bool onlyMine = false)
        {
            var questions = questionRepo.GetAll().ToList();

            // Optional search
            if (!string.IsNullOrEmpty(search))
            {
                questions = questions
                    .Where(q => q.Title != null && q.Title.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0)
                    .ToList();
            }

            // Filter by logged-in user if requested
            if (onlyMine && !string.IsNullOrEmpty(LoggedInUser))
            {
                questions = questions
                    .Where(q => q.Username.Equals(LoggedInUser, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            // Order newest first
            questions = questions.OrderByDescending(q => q.CreatedAt).ToList();

            var questionDtos = new List<QuestionDto>();

            foreach (var q in questions)
            {
                // Get user who posted question
                var user = usersRepo.GetById("User", q.Username);

                // Build Question DTO
                var qDto = new QuestionDto
                {
                    Id = q.RowKey,
                    Title = q.Title,
                    Decription = q.Description,
                    ImageUrl = q.ImageUrl,
                    IsThemeOpen = q.IsThemeOpen,
                    CreatedAt = q.CreatedAt,
                    User = user == null ? null : new UserDto
                    {
                        Username = user.Username,
                        ProfileImageUrl = user.ProfileImageUrl
                    },
                    Answers = new List<AnswerDto>()
                };

                // Get answers for this question
                var answers = answerRepo.GetAll(q.RowKey).ToList();

                foreach (var a in answers)
                {
                    var aUser = usersRepo.GetById("User", a.Username);

                    // Count votes for this answer
                    var votes = voteRepo.GetAll(a.RowKey).ToList();
                    int voteCount = votes.Sum(v => v.Value);

                    // Has the logged-in user voted?
                    int userVote = 0;
                    if (!string.IsNullOrEmpty(LoggedInUser))
                    {
                        var userVoteEntity = votes.FirstOrDefault(v => v.RowKey == LoggedInUser);
                        if (userVoteEntity != null)
                            userVote = userVoteEntity.Value; // 1, -1
                    }

                    // Build Answer DTO
                    var aDto = new AnswerDto
                    {
                        Id = a.RowKey,
                        Text = a.Text,
                        CreatedAt = a.CreatedAt,
                        Votes = voteCount,
                        UserVote = userVote,
                        IsAccepted = a.IsAccepted,
                        User = aUser == null ? null : new UserDto
                        {
                            Username = aUser.Username,
                            ProfileImageUrl = aUser.ProfileImageUrl
                        }
                    };

                    qDto.Answers.Add(aDto);
                }

                questionDtos.Add(qDto);
            }

            return Ok(questionDtos);
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
        [Route("{Username}")]
        public IHttpActionResult Create(string Username, QuestionEntity req)
        {
            if (req == null || string.IsNullOrWhiteSpace(req.Title) || string.IsNullOrWhiteSpace(req.Description))
                return BadRequest("Title and Description are required");

            var user = usersRepo.GetById("User", Username);
            if (user == null)
                return Content(HttpStatusCode.NotFound, $"User with {Username} not found");

            req.Username = Username;
            req.PartitionKey = "Question";
            req.RowKey = Guid.NewGuid().ToString();
            req.CreatedAt = DateTime.UtcNow;
            req.IsThemeOpen = true;


            questionRepo.Insert(req);
            return Ok(new
            {
                QuestionId = req.RowKey
            });
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
            existing.ImageUrl = req.ImageUrl;
            

            questionRepo.Update(existing);

            return Ok(existing);
        }


        [HttpDelete]
        [Route("{id}")]
        public async Task<IHttpActionResult> Delete(string id)
        {
            var existing = questionRepo.GetById("Question", id);
            if (existing == null)
                return NotFound();

            var blobRepo = new ImageBlobStorageRepository();

            // Delete question image if it exists
            if (!string.IsNullOrEmpty(existing.ImageUrl))
            {
                try
                {
                    // Assuming ImageUrl is the full URL, extract the blob name
                    var blobName = Path.GetFileName(existing.ImageUrl);
                    await blobRepo.DeleteImageAsync(blobName);
                }
                catch (Exception ex)
                {
                    // Log error but don't prevent deletion
                    System.Diagnostics.Debug.WriteLine("Failed to delete image from blob: " + ex.Message);
                }
            }

            // Delete all answers
            var answers = answerRepo.GetAll(id).ToList();
            foreach (var a in answers)
            {
                // Delete votes for each answer
                var votes = voteRepo.GetAll(a.RowKey).ToList();
                foreach (var v in votes)
                    voteRepo.Delete(v);

                answerRepo.Delete(a);
            }

            questionRepo.Delete(existing);

            return Ok();
        }



        [HttpPost]
        [Route("{questionId}/upload-picture")]
        public async Task<IHttpActionResult> UploadPicture(string questionId)
        {
            var question = questionRepo.GetById("Question", questionId);
            if (question == null)
                return NotFound();

            if (!Request.Content.IsMimeMultipartContent())
                return BadRequest("Unsupported media type");

            var provider = new MultipartMemoryStreamProvider();
            await Request.Content.ReadAsMultipartAsync(provider);

            var file = provider.Contents.FirstOrDefault();
            if (file == null)
                return BadRequest("No file uploaded");

            var originalFileName = file.Headers.ContentDisposition.FileName?.Trim('"') ?? "upload.png";
            var blobName = $"{questionId}_{Guid.NewGuid()}{Path.GetExtension(originalFileName)}";

            using (var stream = await file.ReadAsStreamAsync())
            {
                var imageUrl = await blobContainer.UploadImageAsync(
                    blobName,
                    stream,
                    file.Headers.ContentType.MediaType
                );

       
                question.ImageUrl = imageUrl;
                questionRepo.Update(question);

                return Ok(new { ImageUrl = imageUrl });
            }
        }


    }
}
