using Groover.BL.Models;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Threading.Tasks;

namespace Groover.BL.Helpers
{
    public interface IAvatarImageProcessor : IImageProcessor
    {
        Task<byte[]> ProcessAsync(IFormFile imageFile, bool failOnNull = false);
        Task<string> SaveImageAsync(byte[] imageBytes);
        void DeleteImage(string path);
        string GetDefaultGroupImage();
        string GetDefaultUserImage();
    }
}