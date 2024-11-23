using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using System.Net.Http.Headers;
using Microsoft.Extensions.Primitives;

namespace GoRideShare
{
    public class GetPost
    {
        private readonly ILogger<GetPost> _logger;
        private readonly IHttpRequestHandler _httpRequestHandler;
        private readonly string? _baseApiUrl;

        public GetPost(ILogger<GetPost> logger, IHttpRequestHandler httpRequestHandler)
        {
            _logger = logger;
            _httpRequestHandler = httpRequestHandler;
            _baseApiUrl = Environment.GetEnvironmentVariable("BASE_API_URL");
        }

        // This function is triggered by an HTTP GET request to retrieve a single post
        [Function("GetPost")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "Posts")] HttpRequest req)
        {

            // Read the posterId from the query params
            string? post_id = null;
            if (req.Query.TryGetValue("post_id", out StringValues postIdParam))
            {
                Guid post_guid = Guid.Empty;
                if (!Guid.TryParse(postIdParam[0], out post_guid))
                {
                    _logger.LogError("Invalid post_id query param");
                    return new BadRequestObjectResult("ERROR: Invalid Query Parameter: post_id");
                } else {
                    post_id = post_guid.ToString();
                }
            } else {
                _logger.LogError("Missing Query Parameter: `user_id`");
                return new BadRequestObjectResult("ERROR: Missing Query Parameter: post_id");
            }

            string endpoint = $"{_baseApiUrl}/api/posts/?post_id={post_id}";
            var (error, response) = await _httpRequestHandler.MakeHttpGetRequest(endpoint);

            if (!error)
            {
                List<PostDetails> posts_list = JsonSerializer.Deserialize<List<PostDetails>>(response);
                if (posts_list == null || posts_list.Count != 1)
                {
                    _logger.LogError($"Error fetching post");
                    return new OkObjectResult("{}");
                }
                return new OkObjectResult(posts_list[0]);
            }
            else
            {
                _logger.LogError("Error connecting to the DB layer.");
                _logger.LogError(response);
                return new ObjectResult("Error connecting to the DB layer.")
                {
                    StatusCode = StatusCodes.Status500InternalServerError
                };
            }
        }
    }
}
