using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace QuickCart.Utility
{
    public interface IStorageService : IBlobService
    {
        string GetPresignedUrl(string fileId, double durationInMinutes = 240);
        Task<Stream> GetFileStreamAsync(string fileId);
    }
}
