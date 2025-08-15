
using Microsoft.Azure;
using ServiceDataRepo.Entities;
using ServiceDataRepo.Repositories;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Http;

namespace YourNamespace.Controllers
{
    [RoutePrefix("api/auth")]
    public class AuthController : ApiController
    {
        private readonly UserTableRepository usersRepo = new UserTableRepository();

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
