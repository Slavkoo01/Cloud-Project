using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceDataRepo.BlobRepositories
{
    public class ImageBlobStorageService
    {
        private readonly BlobContainerClient _containerClient;

        private string connectionString = "DataConnectionString";

        private string containerName = "images";
        public ImageBlobStorageService()
        {
            var blobServiceClient = new BlobServiceClient(connectionString);
            _containerClient = blobServiceClient.GetBlobContainerClient(containerName);
            _containerClient.CreateIfNotExists(PublicAccessType.Blob); // publicly acessable via URL
        }

        public async Task<string> UploadImageAsync(string blobName, Stream imageStream, string contentType)
        {
            var blobClient = _containerClient.GetBlobClient(blobName);
            await blobClient.UploadAsync(imageStream, new BlobHttpHeaders { ContentType = contentType });
            return blobClient.Uri.ToString(); 
        }

        public async Task<Stream> DownloadImageAsync(string blobName)
        {
            var blobClient = _containerClient.GetBlobClient(blobName);
            var downloadInfo = await blobClient.DownloadAsync();
            return downloadInfo.Value.Content;
        }

        public async Task DeleteImageAsync(string blobName)
        {
            var blobClient = _containerClient.GetBlobClient(blobName);
            await blobClient.DeleteIfExistsAsync();
        }
    }
}
