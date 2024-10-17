using Microsoft.Azure;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Blobs
{
    public class PostImageBlob
    {
        // read account configuration settings
        CloudStorageAccount storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("PostImageBlobConnectionString"));

        // create blob container for images
        CloudBlobClient postImageBlobStorage;

        public PostImageBlob()
        {
            postImageBlobStorage = storageAccount.CreateCloudBlobClient();
        }

        public Image DownloadImage(String containerName, String blobName)
        {
            CloudBlobContainer container = postImageBlobStorage.GetContainerReference(containerName);
            CloudBlockBlob blob = container.GetBlockBlobReference(blobName);

            using (MemoryStream ms = new MemoryStream())
            {
                blob.DownloadToStream(ms);
                return new Bitmap(ms);
            }
        }

        public string UploadImage(Image image, String containerName, String blobName)
        {
            CloudBlobContainer container = postImageBlobStorage.GetContainerReference(containerName);
            CloudBlockBlob blob = container.GetBlockBlobReference(blobName);

            using (MemoryStream memoryStream = new MemoryStream())
            {
                image.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Bmp);
                memoryStream.Position = 0;
                blob.Properties.ContentType = "image/bmp";
                blob.UploadFromStream(memoryStream);
                return blob.Uri.ToString();
            }
        }

    }
}
