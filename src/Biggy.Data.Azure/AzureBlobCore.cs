using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using Biggy.Core;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;

namespace Biggy.Data.Azure
{
    public class AzureBlobCore : IAzureDataProvider
    {
        private readonly CloudBlobClient blobClient;
        private readonly string containerName;

        public AzureBlobCore(string connectionStringName)
        {
            var connectionString = ConfigurationManager.ConnectionStrings[connectionStringName].ConnectionString;
            var storageAccount = CloudStorageAccount.Parse(connectionString);
            this.blobClient = storageAccount.CreateCloudBlobClient();
            this.containerName = "biggy";
        }

        public IDataStore<T> CreateStoreFor<T>() where T : new()
        {
            return null;
        }

        public IEnumerable<T> GetAll<T>()
        {
            var blobName = AzureBlobCore.GetBlobName<T>();
            return null;
        }

        public void SaveAll<T>(IEnumerable<T> items)
        {
            var biggyContainer = this.blobClient.GetContainerReference(this.containerName);
            var blobName = AzureBlobCore.GetBlobName<T>();
            var blob = biggyContainer.GetBlobReferenceFromServer(blobName);

            using (var memoryStream = new MemoryStream())
            {
                using (var writer = new StreamWriter(memoryStream))
                {
                    var jsonWriter = new JsonTextWriter(writer);
                    var serializer = JsonSerializer.CreateDefault();
                    serializer.Serialize(jsonWriter, items);
                }

                blob.UploadFromStream(memoryStream);
            }
        }

        private static string GetBlobName<T>()
        {
            var type = typeof(T);
            return type.Name;
        }
    }
}