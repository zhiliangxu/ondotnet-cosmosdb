using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.Net.Http.Headers;

namespace episode1
{
    public class ItemController : Controller
    {
        private readonly Container container;
        private readonly IContainerProxy containerProxy;
        public ItemController(
            CosmosClient client,
            IContainerProxy containerProxy)
        {
            this.container = client.GetContainer("OnDotNet", "episode1");
            this.containerProxy = containerProxy;
        }

        [Route("/item/read/{id}")]
        [HttpGet]
        public async Task<IActionResult> ReadItemAsync(string id)
        {
            ResponseMessage response = await this.container.ReadItemStreamAsync(id, new PartitionKey(id));
            foreach (string header in response.Headers)
            {
                Response.Headers.Add(header, response.Headers[header]);
            }

            if (!response.IsSuccessStatusCode)
            {
                return StatusCode((int) response.StatusCode, response.ErrorMessage);
            }

            return new FileStreamResult(response.Content, new MediaTypeHeaderValue("application/json"));
        }

        [Route("/item/save/{id}")]
        [HttpPost]
        public async Task<IActionResult> SaveItemAsync(string id)
        {
            ResponseMessage response = await this.containerProxy.GetMainContainer().CreateItemStreamAsync(HttpContext.Request.Body, new PartitionKey(id));
            foreach (string header in response.Headers)
            {
                Response.Headers.Add(header, response.Headers[header]);
            }

            if (!response.IsSuccessStatusCode)
            {
                return StatusCode((int) response.StatusCode, response.ErrorMessage);
            }
            
            return StatusCode(201);
        }

        [Route("/item/readtype/{id}")]
        [HttpGet]
        public async Task<IActionResult> ReadTypedItemAsync(string id)
        {
            try
            {
                Model response = await container.ReadItemAsync<Model>(id, new PartitionKey(id));
                return new OkObjectResult(response.DescriptiveTitle);
            }
            catch (CosmosException exception)
            {
                return StatusCode((int) exception.StatusCode, exception.Message);
            }
        }
    }
}