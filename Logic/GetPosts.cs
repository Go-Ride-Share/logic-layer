using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using System.Net.Http.Headers;

namespace GoRideShare
{
    public class GetPosts
    {
        private readonly ILogger<GetPosts> _logger;
        private readonly IHttpRequestHandler _httpRequestHandler;
        private readonly string? _baseApiUrl;

        public GetPosts(ILogger<GetPosts> logger, IHttpRequestHandler httpRequestHandler)
        {
            _logger = logger;
            _httpRequestHandler = httpRequestHandler;
            _baseApiUrl = Environment.GetEnvironmentVariable("BASE_API_URL");
        }

        // This function is triggered by an HTTP GET request to retrive a users posts
        [Function("GetPosts")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest req)
        {
            // Read the user ID and the db token from the headers
            if (!req.Headers.TryGetValue("X-User-ID", out var userId))
            {
                return new BadRequestObjectResult("Missing the following header: \'X-User-ID\'.");
            }
            if (!req.Headers.TryGetValue("X-Db-Token", out var db_token))
            {
                return new BadRequestObjectResult("Missing the following header \'X-Db-Token\'.");
            }
            // Read the posterId from the query params
            if (!req.Query.TryGetValue("userId", out var posterId))
            {
                return new BadRequestObjectResult("Missing the following query param: \'userId\'");
            }

            string endpoint = $"{_baseApiUrl}/api/GetPosts?userId={posterId}";
            var (error, response) = await _httpRequestHandler.MakeHttpGetRequest(endpoint, db_token, userId.ToString());

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
                return new ObjectResult("Error connecting to the DB layer.")
                {
                    StatusCode = StatusCodes.Status500InternalServerError
                };
            }
        }
    }
}
