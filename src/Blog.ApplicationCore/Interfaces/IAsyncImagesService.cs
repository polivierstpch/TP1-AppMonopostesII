using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Blog.ApplicationCore.Interfaces
{
    public interface IAsyncImagesService
    {
        public Task<string> SauvegarderImageAsync(IFormFile image);
    }
}
