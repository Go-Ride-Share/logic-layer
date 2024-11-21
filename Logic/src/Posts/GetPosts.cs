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
            // If validation result is not null, return the bad request result
            var validationResult = Utilities.ValidateHeaders(req.Headers, out string userId, out string db_token);
            if (validationResult != null)
            {
                return validationResult;
            }
            // Read the posterId from the query params
            if (!req.Query.TryGetValue("userId", out var posterId))
            {
                return new BadRequestObjectResult("Missing the following query param: \'userId\'");
            }

            string endpoint = $"{_baseApiUrl}/api/GetPosts?userId={posterId}";
            var (error, response) = await _httpRequestHandler.MakeHttpGetRequest(endpoint, db_token, userId);

            if (!error)
            {
                var posts = JsonSerializer.Deserialize<List<PostDetails>>(response);
                if (posts == null || posts.Count == 0)
                {
                    _logger.LogInformation("No posts found in the response from the DB layer.");
                    return new OkObjectResult("[]");
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
