using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Http;
using System.Web.Routing;
using HealthStatusService;
using System.Web.Mvc;
using System.Web.Optimization;

namespace StackOverflowService
{
    public class WebApiApplication : System.Web.HttpApplication
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
                var setting= CloudConfigurationManager.GetSetting("DataConnectionString");
                if (string.IsNullOrEmpty(setting))
                {
                    throw new Exception("DataConnectionString is null! Proveri Web.config.");
                }
                var storageAccount = CloudStorageAccount.Parse(setting);

                // create blob container for images
                CloudBlobClient blobStorage = storageAccount.CreateCloudBlobClient();
                CloudBlobContainer container = blobStorage.GetContainerReference("profilephoto");
                container.CreateIfNotExists();
                // configure container for public access
                var permissions = container.GetPermissions();
                permissions.PublicAccess = BlobContainerPublicAccessType.Container;
                container.SetPermissions(permissions);
            }
            catch (WebException)
            {
            }
        }
    }
}
