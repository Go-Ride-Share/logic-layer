using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using System.Net.Http.Headers;

namespace GoRideShare
{
    public class GetAllPosts
    {
        private readonly ILogger<GetAllPosts> _logger;
        private readonly IHttpRequestHandler _httpRequestHandler;
        private readonly string? _baseApiUrl;

        public GetAllPosts(ILogger<GetAllPosts> logger, IHttpRequestHandler httpRequestHandler)
        {
            _logger = logger;
            _httpRequestHandler = httpRequestHandler;
            _baseApiUrl = Environment.GetEnvironmentVariable("BASE_API_URL");
        }

        // This function is triggered by an HTTP GET request to retrieve all posts
        [Function("GetAllPosts")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest req)
        {

            string endpoint = $"{_baseApiUrl}/api/GetAllPosts";
            var (error, response) = await _httpRequestHandler.MakeHttpGetRequest(endpoint);

            if (!error)
            {
                var posts = JsonSerializer.Deserialize<List<PostDetails>>(response);
                if (posts == null || posts.Count == 0)
                {
                    _logger.LogError("No posts found in the response from the DB layer.");
                    return new ObjectResult("No posts found in the response from the DB layer.")
                    {
                        StatusCode = StatusCodes.Status500InternalServerError
                    };
                }
                return new OkObjectResult(posts);
            }
            else
            {
                _logger.LogError("Error connecting to the DB layer.");
                return new ObjectResult("Error connecting to the DB layer.")
                {
                    StatusCode = StatusCodes.Status500InternalServerError
                };
            }
        }
    }
}
