using System;
using Microsoft.Azure.Functions.Worker;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;

namespace EShop.Orders
{
    public static class OrderItemReserver
    {
        [Function("OrderItemReserver")]
        public static async Task Run([ServiceBusTrigger("Orders")] string order, FunctionContext context)
        {
            var connectionString = Environment.GetEnvironmentVariable("STORAGECONNECTION");
            var containerName = Environment.GetEnvironmentVariable("CONTAINERNAME");
            var retryNumber = 3;
            var tried = 0;
            var logger = context.GetLogger<string>();

            var id = JsonSerializer.Deserialize<Order>(order).id;

            do
            {
                try
                {
                    var blobClient = new BlobServiceClient(connectionString);
                    var containerClient = blobClient.GetBlobContainerClient(containerName);
                    await containerClient.UploadBlobAsync(
                        $"{id}.json",
                        new MemoryStream(Encoding.UTF8.GetBytes(order))
                    );
                    break;
                }
                catch (Exception ex)
                {
                    tried++;
                    logger.LogError(ex.Message);
                    if (tried == retryNumber)
                    {
                        throw ex;
                    }
                }
            }while(tried<retryNumber);
        }
    }
}
