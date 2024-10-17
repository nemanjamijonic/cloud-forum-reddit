using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Drawing;
using System.IO;

namespace Common.Blobs
{
    public class UserImageBlob
    {
        private CloudBlobClient blobClient;

        public UserImageBlob()
        {
            var storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("UserImageBlobConnectionString"));
            blobClient = storageAccount.CreateCloudBlobClient();
        }

        public Image DownloadImage(string containerName, string blobName)
        {
            var container = blobClient.GetContainerReference(containerName);
            var blob = container.GetBlockBlobReference(blobName);

            using (var memoryStream = new MemoryStream())
            {
                blob.DownloadToStream(memoryStream);
                return Image.FromStream(memoryStream);
            }
        }

        public string UploadImage(Image image, string containerName, string blobName)
        {
            try
            {
                var container = blobClient.GetContainerReference(containerName);
                container.CreateIfNotExists();
                var blob = container.GetBlockBlobReference(blobName);

                using (var memoryStream = new MemoryStream())
                {
                    image.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Bmp);
                    memoryStream.Position = 0;
                    blob.Properties.ContentType = "image/bmp";
                    blob.UploadFromStream(memoryStream);
                    return blob.Uri.ToString();
                }
            }
            catch (Exception ex)
            {
                // Log error
                Console.WriteLine($"Error uploading image: {ex.Message}");
                throw;
            }
        }

    }
}
