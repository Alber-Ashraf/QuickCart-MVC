using System;
using System.IO;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace QuickCart.Utility
{
    public class BackblazeStorageService : IStorageService, IBlobService
    {
        private readonly IAmazonS3 _s3Client;
        private readonly string _bucketName;
        private readonly string _friendlyUrlBase;

        public BackblazeStorageService(IOptions<BackblazeSettings> settings, IAmazonS3 s3Client)
        {
            _s3Client = s3Client ?? throw new ArgumentNullException(nameof(s3Client));

            var config = settings?.Value ?? new BackblazeSettings();
            _bucketName = config.BucketName;
            var region = string.IsNullOrWhiteSpace(config.Region) ? "us-east-005" : config.Region;

            // S3-compatible Backblaze friendly URL base
            if (!string.IsNullOrWhiteSpace(config.Endpoint) && !string.IsNullOrWhiteSpace(_bucketName))
            {
                var endpointUri = new Uri(config.Endpoint);
                _friendlyUrlBase = $"https://{_bucketName}.{endpointUri.Host}";
            }
            else
            {
                _friendlyUrlBase = $"https://{_bucketName}.s3.{region}.backblazeb2.com";
            }
        }

        public async Task<string> UploadFileAsync(IFormFile file, string fileName)
        {
            if (file == null || file.Length == 0)
            {
                throw new ArgumentException("Cannot upload an empty or null file.", nameof(file));
            }

            using var stream = file.OpenReadStream();
            var uploadRequest = new PutObjectRequest
            {
                InputStream = stream,
                Key = fileName,
                BucketName = _bucketName,
                ContentType = string.IsNullOrWhiteSpace(file.ContentType) ? "application/octet-stream" : file.ContentType,
                CannedACL = S3CannedACL.PublicRead
            };

            await _s3Client.PutObjectAsync(uploadRequest);

            return $"{_friendlyUrlBase}/{fileName}";
        }

        public async Task<bool> DeleteFileFromBlob(string blobName)
        {
            if (string.IsNullOrWhiteSpace(blobName))
            {
                return false;
            }

            try
            {
                var deleteRequest = new DeleteObjectRequest
                {
                    BucketName = _bucketName,
                    Key = blobName
                };

                var response = await _s3Client.DeleteObjectAsync(deleteRequest);
                return (int)response.HttpStatusCode >= 200 && (int)response.HttpStatusCode < 300;
            }
            catch
            {
                return false;
            }
        }

        public string GetPresignedUrl(string fileId, double durationInMinutes = 240)
        {
            var request = new GetPreSignedUrlRequest
            {
                BucketName = _bucketName,
                Key = fileId,
                Verb = HttpVerb.GET,
                Expires = DateTime.UtcNow.AddMinutes(durationInMinutes)
            };

            return _s3Client.GetPreSignedURL(request);
        }

        public async Task<Stream> GetFileStreamAsync(string fileId)
        {
            var request = new GetObjectRequest
            {
                BucketName = _bucketName,
                Key = fileId
            };

            var response = await _s3Client.GetObjectAsync(request);
            return response.ResponseStream;
        }
    }
}
