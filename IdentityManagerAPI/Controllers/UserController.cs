using DataAcess.Repos.IRepos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Models.Domain;
using Models.DTOs.image;
using System.Security.Claims;

namespace IdentityManagerAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IImageRepository imageRepo;
        private readonly IUserRepository userRepo;


        public UserController(IImageRepository imageRepo , IUserRepository userRepo)
        {
            this.imageRepo = imageRepo;
            this.userRepo = userRepo;
        }

        public IUserRepository UserRepo { get; }

        [HttpPost]
        [Authorize]
        [Route("uploadUserImage")]
        public async Task<IActionResult> UploadUserImage([FromForm] ImageUploadRequestDto request)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userIdString == null)
            {
                return BadRequest("User not found");
            }


            var user = await userRepo.GetUserByID(userIdString);


            ValidateFileUpload(request);
            if (ModelState.IsValid)
            {
                //conv DTO to domain
                var image = new Image
                {
                    File = request.File,
                    FileName = request.FileName,
                    FileExtension = Path.GetExtension(request.File.FileName),
                    FileSize = request.File.Length
                };

                //repo logic
                await imageRepo.Upload(image);
                user.ImageId = image.Id;
                await userRepo.UpdateAsync(user);
                return Ok(user);
            }
            return BadRequest(ModelState);
        }


        private void ValidateFileUpload(ImageUploadRequestDto request)
        {
            if (request.File == null)
            {
                ModelState.AddModelError("File", "File is required");
            }
            else if (request.File.Length == 0)
            {
                ModelState.AddModelError("File", "File is empty");
            }
            else if (request.File.Length > 10 * 1024 * 1024)
            {
                ModelState.AddModelError("File", "File is too large");
            }
            else if (request.File.ContentType != "image/jpeg" && request.File.ContentType != "image/png")
            {
                ModelState.AddModelError("File", "File is not an image");
            }

            if (!ModelState.IsValid)
            {
                throw new System.Exception("Invalid file upload request");
            }
        }

    }
}
