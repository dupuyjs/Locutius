using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using System.IO;
using System.Threading.Tasks;

namespace Locutius.Common.Helpers
{
    public class AzureStorageHelper
    {
        public static async Task UploadTranscriptFileAsync(string localFilePath, string storageConnectionString, string storageContainerName, string blobName)
        {
            var blobContainerClient = new BlobContainerClient(storageConnectionString, storageContainerName);
            var blobClient = blobContainerClient.GetBlobClient(blobName);

            var headers = new BlobHttpHeaders() { ContentType = "application/json" };

            using (FileStream uploadFileStream = File.OpenRead(localFilePath))
            {
                await blobClient.UploadAsync(uploadFileStream, headers).ConfigureAwait(false);
                uploadFileStream.Close();
            };
        }

        public static async Task UploadAudioFileAsync(string localFilePath, string storageConnectionString, string storageContainerName, string blobName)
        {
            var blobContainerClient = new BlobContainerClient(storageConnectionString, storageContainerName);
            var blobClient = blobContainerClient.GetBlobClient(blobName);

            var headers = new BlobHttpHeaders() { ContentType = "audio/wav" };

            using (FileStream uploadFileStream = File.OpenRead(localFilePath))
            {
                await blobClient.UploadAsync(uploadFileStream, headers).ConfigureAwait(false);
                uploadFileStream.Close();
            };
        }
    }
}
