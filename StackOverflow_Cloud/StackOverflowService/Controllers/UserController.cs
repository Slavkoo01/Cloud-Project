using ServiceDataRepo.BlobRepositories;
using ServiceDataRepo.Entities;
using ServiceDataRepo.Repositories;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace StackOverflowService.Controllers
{
    [RoutePrefix("api/users")]
    public class UserController : ApiController
    {
        private readonly UserTableRepository userRepo = new UserTableRepository();
        private readonly ImageBlobStorageRepository blobContainer = new ImageBlobStorageRepository();

        public UserController() { }

       
        [HttpGet]
        [Route("{Username}")]
        
        public IHttpActionResult GetProfile(string Username)
        {
            var user = userRepo.GetById("User", Username);
            if (user == null)
                return Content(HttpStatusCode.NotFound, $"User with {Username} not found");

            return Ok(user);
        }

        /// <summary>
        /// Update user profile (without password change)
        /// </summary>
        [HttpPut]
        [Route("{Username}")]
        public IHttpActionResult UpdateProfile(string Username, [FromBody] UserEntity updatedUser)
        {
            var existingUser = userRepo.GetById("User", Username);
            if (existingUser == null)
                return Content(HttpStatusCode.NotFound, $"User with {Username} not found");

            existingUser.Name = updatedUser.Name;
            existingUser.LastName = updatedUser.LastName;
            existingUser.Gender = updatedUser.Gender;
            existingUser.Country = updatedUser.Country;
            existingUser.City = updatedUser.City;
            existingUser.Address = updatedUser.Address;

            userRepo.Update(existingUser);
            return Ok(existingUser);
        }
        [HttpPost]
        [Route("{Username}/upload-picture")]
        public async Task<IHttpActionResult> UploadProfilePicture(string Username)
        {
           
            var existingUser = userRepo.GetById("User", Username);
            if (existingUser == null)
                return Content(HttpStatusCode.NotFound, $"User with {Username} not found");

           
            var file = Request.Content.IsMimeMultipartContent()
                ? await Request.Content.ReadAsMultipartAsync()
                : null;

            if (file == null)
                return BadRequest("No file uploaded");

            var uploadedFile = file.Contents.FirstOrDefault();
            if (uploadedFile == null)
                return BadRequest("No file uploaded");

           
            var fileName = uploadedFile.Headers.ContentDisposition.FileName.Trim('\"');
            var blobName = $"{Username}_{Guid.NewGuid()}{Path.GetExtension(fileName)}";

           
            using (var stream = await uploadedFile.ReadAsStreamAsync())
            {
                var imageUrl = await blobContainer.UploadImageAsync(blobName, stream, uploadedFile.Headers.ContentType.MediaType);

                
                existingUser.ProfileImageUrl = imageUrl;
                userRepo.Update(existingUser);

                
                return Ok(new { ImageUrl = imageUrl });
            }
        }

    }
}
