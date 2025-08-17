
using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage;
using ServiceDataRepo.Entities;
using ServiceDataRepo.Repositories;
using StackOverflowService.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Web;
using System.Web.Http;

namespace YourNamespace.Controllers
{
    [RoutePrefix("api/auth")]
    public class AuthController : ApiController
    {
        private readonly UserTableRepository usersRepo = new UserTableRepository();

        public AuthController() { }


        /*
        [HttpPost]
        [Route("register")]
        public IHttpActionResult Register(UserEntity req)
        {
            var httpRequest = HttpContext.Current.Request;

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


            usersRepo.Insert(new UserEntity(req.Username)
            {
                Name = req.Name,
                LastName = req.LastName,
                Gender = req.Gender,
                Country = req.Country,
                City = req.City,
                Address = req.Address,
                Email = req.Email,
                Password = req.Password 
            });

            return Ok("User registered successfully");
        }
        */

        [HttpPost]
        [Route("register")]
        public IHttpActionResult Register()
        {
            var httpRequest = HttpContext.Current.Request;

            try
            {
                // 1️⃣ Validacija form polja
                var firstName = httpRequest.Form["firstName"];
                var lastName = httpRequest.Form["lastName"];
                var gender = httpRequest.Form["Gender"];
                var country = httpRequest.Form["Country"];
                var city = httpRequest.Form["City"];
                var address = httpRequest.Form["Address"];
                var email = httpRequest.Form["Email"];
                var password = httpRequest.Form["Password"];

                var missingFields = new List<string>();
                if (string.IsNullOrWhiteSpace(firstName)) missingFields.Add(nameof(firstName));
                if (string.IsNullOrWhiteSpace(lastName)) missingFields.Add(nameof(lastName));
                if (string.IsNullOrWhiteSpace(gender)) missingFields.Add(nameof(gender));
                if (string.IsNullOrWhiteSpace(country)) missingFields.Add(nameof(country));
                if (string.IsNullOrWhiteSpace(city)) missingFields.Add(nameof(city));
                if (string.IsNullOrWhiteSpace(address)) missingFields.Add(nameof(address));
                if (string.IsNullOrWhiteSpace(email)) missingFields.Add(nameof(email));
                if (string.IsNullOrWhiteSpace(password)) missingFields.Add(nameof(password));

                if (missingFields.Any())
                {
                    return BadRequest("Missing required fields: " + string.Join(", ", missingFields));
                }

                string imageUrl = null;
                try
                {
                    var file = httpRequest.Files["Image"];
                    if (file != null && file.ContentLength > 0)
                    {
                        var storageAccount = CloudStorageAccount.Parse(
                            CloudConfigurationManager.GetSetting("DataConnectionString")
                        );
                        var blobClient = storageAccount.CreateCloudBlobClient();
                        var container = blobClient.GetContainerReference("profilephoto");
                        container.CreateIfNotExists();

                        var blobName = Guid.NewGuid().ToString() + System.IO.Path.GetExtension(file.FileName);
                        var blockBlob = container.GetBlockBlobReference(blobName);
                        blockBlob.UploadFromStream(file.InputStream);
                        imageUrl = blockBlob.Uri.ToString();
                    }
                }
                catch (Exception ex)
                {
                    return Content(HttpStatusCode.InternalServerError, "Error uploading image: " + ex.Message);
                }

                string hashedPassword;
                try
                {
                    string salt = Guid.NewGuid().ToString();
                    hashedPassword = PasswordHelper.HashPassword(password, salt);
                }
                catch (Exception ex)
                {
                    return Content(HttpStatusCode.InternalServerError, "Error hashing password: " + ex.Message);
                }

                var user = new UserEntity(email)
                {
                    Name = firstName,
                    LastName = lastName,
                    Gender = gender,
                    Country = country,
                    City = city,
                    Address = address,
                    Email = email,
                    Password = hashedPassword,
                    ImageUrl = imageUrl
                };

                try
                {
                    usersRepo.Insert(user);
                }
                catch (Exception ex)
                {
                    return Content(HttpStatusCode.InternalServerError, "Error inserting user: " + ex.Message);
                }

                return Ok("User registered successfully");
            }
            catch (Exception ex)
            {
                return Content(HttpStatusCode.InternalServerError, "Unexpected error: " + ex.Message);
            }
        }



        [HttpPost]
        [Route("login")]
        public IHttpActionResult Login(UserEntity req)
        {
            var user = usersRepo.GetAll().ToList().FirstOrDefault(u => u.Email == req.Email && u.Password == req.Password);
            if (user == null)
            {
                return Content(HttpStatusCode.Unauthorized, "Invalid credentials");
            }

            return Ok($"Welcome {user}");
        }
    }
}
