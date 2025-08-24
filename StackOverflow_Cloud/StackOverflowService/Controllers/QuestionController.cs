using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage;
using ServiceDataRepo.BlobRepositories;
using ServiceDataRepo.Entities;
using ServiceDataRepo.Repositories;
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
