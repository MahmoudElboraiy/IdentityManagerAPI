using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.DTOs.image
{
    public class ImageUpIoadRequestDto
    {
        public IFormFile File { get; set; }
        public string FileName { get; set; }

    }
}
