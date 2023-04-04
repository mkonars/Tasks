using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Net.Http;
using System.Reflection.Metadata;
using System.Threading.Tasks;
using Azure.Data.Tables;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Table = Microsoft.Azure.WebJobs.TableAttribute;

namespace Task1
{
    public class GetRandomResponses
    {
        private readonly HttpClient _httpClient;

        private const string TableName = "RandomResponses";
        private const string BlobContainer = "randomresponses";

        public GetRandomResponses(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient();
        }

        [FunctionName("FetchRandomResponses")]
        [return: Table(TableName)]
        public async Task<RandomResponse> Run(
            [TimerTrigger("0 * * * * *")] TimerInfo myTimer,
            ILogger log, IBinder binder
            )
        {
            log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");

            var response = await _httpClient.GetAsync("https://api.publicapis.org/random?auth=null");
            var content = await response.Content.ReadAsStringAsync();

            var rowKey = $"{DateTime.UtcNow.ToString("yyyyMMddhhmmss")}.json";

            using (var writer = await binder.BindAsync<TextWriter>(
              new BlobAttribute($"{BlobContainer}/{rowKey}")))
            {
                writer.Write(content);
            }

            return new RandomResponse
            {
                PartitionKey = DateTime.UtcNow.ToString("yyyyMMdd"),
                RowKey = rowKey,
                Success = response.IsSuccessStatusCode
            };
        }

        // sample call: http://localhost:7175/api/RandomResponses?from=2023-01-01&to=2023-05-31
        [FunctionName("GetRandomResponses")]
        public static async Task<IActionResult> Query(
            [HttpTrigger("get", Route = "RandomResponses")] HttpRequest req,
            [Table(TableName)] TableClient tableClient)
        {
            if (!DateTime.TryParse(req.Query["from"], out var from))
                return new BadRequestObjectResult("Error parsing 'from' parameter");

            if (!DateTime.TryParse(req.Query["to"], out var to))
                return new BadRequestObjectResult("Error parsing 'to' parameter");

            var queryResults = tableClient.QueryAsync<RandomResponse>(x => x.Timestamp >= from && x.Timestamp <= to);

            var result = new List<RandomResponse>();

            await foreach (RandomResponse entity in queryResults)
                result.Add(entity);

            return new OkObjectResult(result);
        }

        // sample call: http://localhost:7175/api/RandomResponse/20230404055800.json
        [FunctionName("GetRandomResponse")]
        public static IActionResult GetById(
            [HttpTrigger("GET", Route = "RandomResponse/{id}")] HttpRequest req, string id,
            [Blob($"{BlobContainer}/{{id}}", FileAccess.Read)] Stream blobStream)
        {
            if (blobStream == null)
                return new NotFoundObjectResult($"Id not found: {id}");

            var reader = new StreamReader(blobStream);
            string response = reader.ReadToEnd();

            return new OkObjectResult(response);
        }
    }
}
