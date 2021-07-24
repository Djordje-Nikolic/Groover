using Microsoft.AspNetCore.Http;
using System.IO;
using System.Threading.Tasks;

namespace Groover.BL.Helpers
{
    public interface IImageProcessor
    {
        Task<byte[]> ProcessAsync(IFormFile imageFile, bool failOnNull = false);
        Task<byte[]> CheckAsync(byte[] imageBytes, bool failOnNull = false);
        Task<byte[]> CheckAsync(MemoryStream ms);
        Task<string> SaveImageAsync(byte[] imageBytes);
        void DeleteImage(string path);
        string GetDefaultGroupImage();
        string GetDefaultUserImage();
    }
}