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
        private readonly Utilities _utilities;

        public GetPosts(ILogger<GetPosts> logger, IHttpRequestHandler httpRequestHandler, Utilities utilities)
        {
            _logger = logger;
            _utilities = utilities;
            _httpRequestHandler = httpRequestHandler;
            _baseApiUrl = Environment.GetEnvironmentVariable("BASE_API_URL");
        }

        // This function is triggered by an HTTP GET request to retrive a users posts
        [Function("PostsGet")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "Posts/{user_id}")] HttpRequest req, string user_id)
        {
            // If validation result is not null, return the bad request result
            var validationResult = _utilities.ValidateHeadersAndTokens(req.Headers, out string requsterUserId, out string db_token);
            if (validationResult != null)
            {
                return validationResult;
            }
            // Read the posterId from the path params
            if (string.IsNullOrEmpty(user_id?.Trim()))
            {
                _logger.LogError("Invalid Path Parameter: `user_id` not passed");
                return new BadRequestObjectResult("Invalid Path Parameter: `user_id` not passed");
            } else {
                _logger.LogInformation($"user_id: {user_id}");
            }

            string endpoint = $"{_baseApiUrl}/api/posts/{user_id}";
            var (error, response) = await _httpRequestHandler.MakeHttpGetRequest(endpoint, db_token, requsterUserId);

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
