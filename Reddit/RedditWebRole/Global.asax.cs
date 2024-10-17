using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace RedditWebRole
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            InitBlobs();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }

        public void InitBlobs()
        {
            try
            {
                // read account configuration settings
                var storageAccount1 = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("UserImageBlobConnectionString"));
                // create blob container for images
                CloudBlobClient blobStorage1 = storageAccount1.CreateCloudBlobClient();
                CloudBlobContainer container1 = blobStorage1.GetContainerReference("user-images");
                container1.CreateIfNotExists();
                // configure container for public access
                var permissions1 = container1.GetPermissions();
                permissions1.PublicAccess = BlobContainerPublicAccessType.Container;
                container1.SetPermissions(permissions1);

                // read account configuration settings
                var storageAccount2 = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("PostImageBlobConnectionString"));
                // create blob container for images
                CloudBlobClient blobStorage2 = storageAccount2.CreateCloudBlobClient();
                CloudBlobContainer container2 = blobStorage2.GetContainerReference("post-images");
                container2.CreateIfNotExists();
                // configure container for public access
                var permissions2 = container2.GetPermissions();
                permissions2.PublicAccess = BlobContainerPublicAccessType.Container;
                container2.SetPermissions(permissions2);
            }
            catch (WebException wex)
            {
                Console.WriteLine("Error: " + wex);
            }
        }
    }
}
