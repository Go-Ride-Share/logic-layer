using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using System.Net.Http.Headers;

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
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "Posts/{post_id?}")] HttpRequest req, string? post_id)
        {

            // Read the posterId from the query params
            if ( post_id != null && !Guid.TryParse(post_id, out Guid _))
            {
                _logger.LogError("Invalid Query Parameter: `post_id` must be a Guid");
                return new BadRequestObjectResult("Invalid Query Parameter: `post_id` must be a Guid");
            } else {
                _logger.LogInformation($"post_id: {post_id}");
            }

            string endpoint = $"{_baseApiUrl}/api/post/{post_id}";
            var (error, response) = await _httpRequestHandler.MakeHttpGetRequest(endpoint);

            if (!error)
            {
                var post = JsonSerializer.Deserialize<PostDetails>(response);
                if (post == null)
                {
                    _logger.LogError("Error fetching post");
                    return new OkObjectResult("{}");
                }
                return new OkObjectResult(post);
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
