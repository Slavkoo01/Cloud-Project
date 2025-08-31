
using Microsoft.Azure;
using ServiceDataRepo.BlobRepositories;
using ServiceDataRepo.Entities;
using ServiceDataRepo.Repositories;
using StackOverflowService.Service;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace YourNamespace.Controllers
{
    [RoutePrefix("api/auth")]
    public class AuthController : ApiController
    {
        private readonly UserTableRepository usersRepo = new UserTableRepository();
        private readonly ImageBlobStorageRepository blobContainer = new ImageBlobStorageRepository();


        public AuthController() { }

        [HttpPost]
        [Route("register")]
        public IHttpActionResult Register(UserEntity req)
        {

            if (req == null)
            {
                return BadRequest("Request body is missing.");
            }


            var missingFields = new List<string>();

            if (string.IsNullOrWhiteSpace(req.Email)) missingFields.Add(nameof(req.Email));
            if (string.IsNullOrWhiteSpace(req.Name)) missingFields.Add(nameof(req.Name));
            if (string.IsNullOrWhiteSpace(req.LastName)) missingFields.Add(nameof(req.LastName));
            if (string.IsNullOrWhiteSpace(req.Password)) missingFields.Add(nameof(req.Password));
            if (string.IsNullOrWhiteSpace(req.Country)) missingFields.Add(nameof(req.Country));
            if (string.IsNullOrWhiteSpace(req.City)) missingFields.Add(nameof(req.City));
            if (string.IsNullOrWhiteSpace(req.Address)) missingFields.Add(nameof(req.Address));
            if (string.IsNullOrWhiteSpace(req.Gender)) missingFields.Add(nameof(req.Gender));

            if (missingFields.Any())
            {
                return BadRequest("Missing required fields: " + string.Join(", ", missingFields));
            }
            var users = usersRepo.GetAll().ToList();

            var existing = users.FirstOrDefault(u => u.Email == req.Email);
            if (existing != null)
            {
                return Content(HttpStatusCode.Conflict, "Email already exists");
            }
            existing = users.FirstOrDefault(u => u.Username == req.Username);
            if (existing != null)
            {
                return Content(HttpStatusCode.Conflict, "Username already exists");
            }

            var salt = HashPassword.GenSalt();
            var hash = HashPassword.Hash(req.Password, salt);

            usersRepo.Insert(new UserEntity(req.Username)
            {
                Name = req.Name,
                LastName = req.LastName,
                Gender = req.Gender,
                Country = req.Country,
                City = req.City,
                Address = req.Address,
                Email = req.Email,
                Salt = Convert.ToBase64String(salt),
                Password = hash,
                ProfileImageUrl = req.ProfileImageUrl,
            });

            return Ok("User registered successfully");
        }

        [HttpPost]
        [Route("login")]
        public IHttpActionResult Login(UserEntity req)
        {
            var user = usersRepo.GetAll().ToList().FirstOrDefault(
                    u => u.Email == req.Email &&
                    u.Password == HashPassword.Hash(req.Password, Convert.FromBase64String(u.Salt)));

            if (user == null)
            {
                return Content(HttpStatusCode.Unauthorized, new { message = "Invalid credentials" });
            }

            return Ok(new
            {
                Name = user.Name,
                LastName = user.LastName,
                Gender = user.Gender,
                Country = user.Country,
                City = user.City,
                Address = user.Address,
                Email = user.Email,
                Username = user.Username,
                ProfileImageUrl = user.ProfileImageUrl

            });
        }

        [HttpPost]
        [Route("register/{username}/upload-picture")]
        public async Task<IHttpActionResult> UploadPicture(string username)
        {
            var user = usersRepo.GetById("User", username);
            if (user == null)
                return NotFound();

            if (!Request.Content.IsMimeMultipartContent())
                return BadRequest("Unsupported media type");

            var provider = new MultipartMemoryStreamProvider();
            await Request.Content.ReadAsMultipartAsync(provider);

            var file = provider.Contents.FirstOrDefault();
            if (file == null)
                return BadRequest("No file uploaded");

            var originalFileName = file.Headers.ContentDisposition.FileName?.Trim('"') ?? "upload.png";
            var blobName = $"{username}_{Guid.NewGuid()}{Path.GetExtension(originalFileName)}";

            using (var stream = await file.ReadAsStreamAsync())
            {
                var imageUrl = await blobContainer.UploadImageAsync(
                    blobName,
                    stream,
                    file.Headers.ContentType.MediaType
                );


                user.ProfileImageUrl = imageUrl;
                usersRepo.Update(user);

                return Ok(new { Message = "Profile picture uploaded successfully", ImageUrl = imageUrl });

            }
        }
    }
}
