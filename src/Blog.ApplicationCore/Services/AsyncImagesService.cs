using Blog.ApplicationCore.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Threading.Tasks;

namespace Blog.ApplicationCore.Services
{
    public class AsyncImagesService : IAsyncImagesService
    {
        private readonly IWebHostEnvironment _env;

        public AsyncImagesService(IWebHostEnvironment env)
        {
            _env = env;
        }

        public async Task<string> SauvegarderImageAsync(IFormFile image)
        {
            var path = _env.WebRootPath + @"\images\" + image.FileName;

            using (var stream = new FileStream(path, FileMode.Create))
            {
                await image.CopyToAsync(stream);
            }

            //On retourne le nom de l'image
            return image.FileName;
        }

       
    }



    
}
