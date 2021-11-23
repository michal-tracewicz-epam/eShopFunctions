using System;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace EShop.Orders
{
    public static class DeliveryOrderProcessor
    {
        [Function("DeliveryOrderProcessor")]
        public static async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "DeliveryOrderProcessor/{id:int}")] HttpRequestData req,
            FunctionContext executionContex, int id)
        {
            var logger = executionContex.GetLogger("Orders");

            var endpoint = Environment.GetEnvironmentVariable("ENDPOINT");
            var primaryKey = Environment.GetEnvironmentVariable("PK");
            var dbId = Environment.GetEnvironmentVariable("DB");
            var containerId = Environment.GetEnvironmentVariable("CONTAINERID");

            var client = new CosmosClient(endpoint, primaryKey);
            var db = client.GetDatabase(dbId);
            var conteiner = db.GetContainer(containerId);

            try
            {
                var order = await JsonSerializer.DeserializeAsync<Order>(
                    req.Body,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                );
                var result = await conteiner.CreateItemAsync(order, new PartitionKey(order.BuyerId));
                logger.LogTrace($"Upsert sucessfull! {result.StatusCode}");
            }
            catch (Exception e)
            {
                logger.LogError(id, e, "Error adding to Cosmos!");
            }

            var response = req.CreateResponse(HttpStatusCode.Created);
            response.Headers.Add("Content-Type", "text/plain; charset=utf-8");
            response.WriteString($"Order placed!");
            return response;
        }
    }
}
