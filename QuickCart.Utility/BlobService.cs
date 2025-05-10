using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace QuickCart.Utility
{
    public class BlobService : IBlobService
    {
        private readonly string _connectionString;
        private readonly string _containerName;

        public BlobService(IConfiguration configuration)
        {
            _connectionString = configuration.GetSection("AzureBlobStorage:ConnectionString").Value;
            _containerName = configuration.GetSection("AzureBlobStorage:ContainerName").Value;
        }

        public async Task<string> UploadFileAsync(IFormFile file, string fileName)
        {
            var blobContainer = new BlobContainerClient(_connectionString, _containerName);
            await blobContainer.CreateIfNotExistsAsync();
            var blobClient = blobContainer.GetBlobClient(fileName);

            using (var stream = file.OpenReadStream())
            {
                await blobClient.UploadAsync(stream, true);
            }

            return blobClient.Uri.ToString(); // URL للملف على Azure
        }

        public async Task<bool> DeleteFileFromBlob(string blobName)
        {
            var blobContainer = new BlobContainerClient(_connectionString, _containerName);
            var blobClient = blobContainer.GetBlobClient(blobName);
            return await blobClient.DeleteIfExistsAsync();
        }
    }
}
