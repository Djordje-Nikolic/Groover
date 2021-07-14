using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Groover.BL.Helpers
{
    public interface IImageProcessor
    {
        Task<byte[]> Process(IFormFile imageFile);
    }
}