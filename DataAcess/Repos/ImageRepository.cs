using DataAcess.Repos.IRepos;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Models.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAcess.Repos
{
    public class ImageRepository : IImageRepository
    {
        private readonly ApplicationDbContext db;
        private readonly IWebHostEnvironment webHostEnvironment;
        private readonly IHttpContextAccessor contextAccessor;

        public ImageRepository(ApplicationDbContext db, IWebHostEnvironment webHostEnvironment ,IHttpContextAccessor contextAccessor)
        {
            this.db = db;
            this.webHostEnvironment = webHostEnvironment;
            this.contextAccessor = contextAccessor;
        }

        public async Task<Image> Upload(Image image)
        {
            var localFilepath = Path.Combine(webHostEnvironment.WebRootPath, "Images", $"{image.FileName}.{image.FileExtension}");

            using var fileStream = new FileStream(localFilepath, FileMode.Create);
            await image.File.CopyToAsync(fileStream);

            //                                          https                ://localhost:
            var urlFilepath = $"{contextAccessor.HttpContext.Request.Scheme}://{contextAccessor.HttpContext.Request.Host}" +
                //                5000
                $"{contextAccessor.HttpContext.Request.PathBase}" +
                // /Images       /imagename      .jpg
                $"/Images/{image.FileName}.{image.FileExtension}";

            image.FilePath = urlFilepath;
            await db.Image.AddAsync(image);
            await db.SaveChangesAsync();

            return image;
        }
    }
}
