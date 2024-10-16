using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using Microsoft.AspNetCore.Http;

namespace GoRideShare
{
    public class SavePost
    {
        private readonly ILogger<SavePost> _logger;
        private readonly string? _baseApiUrl;
        private readonly IHttpRequestHandler _httpRequestHandler;

        public SavePost(ILogger<SavePost> logger, IHttpRequestHandler httpRequestHandler)
        {
            _logger = logger;
            _httpRequestHandler = httpRequestHandler;
            _baseApiUrl = Environment.GetEnvironmentVariable("BASE_API_URL");
        }

        // This function is triggered by an HTTP POST request to create a new post
        [Function("SavePost")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequest req)
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

            // Read the request body to get the user's registration information
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            PostDetails? newPost = null;
            try
            {
                newPost = JsonSerializer.Deserialize<PostDetails>(requestBody);
            }
            catch (JsonException ex)
            {
                _logger.LogError($"JSON deserialization failed: {ex.Message}");
            }
            _logger.LogInformation($"Raw Request Body: {JsonSerializer.Serialize(requestBody)}");

            // Validate if essential post data is present
            if (newPost == null)
            {
                return new BadRequestObjectResult("Incomplete post data.");
            }
            else if (string.IsNullOrEmpty(newPost.Name) ||
                string.IsNullOrEmpty(newPost.Description) ||
                90 < newPost.OriginLat || newPost.OriginLat < -90 ||
                90 < newPost.DestinationLat || newPost.DestinationLat < -90 ||
                180 < newPost.OriginLng || newPost.OriginLng < -180 ||
                180 < newPost.DestinationLng || newPost.DestinationLng < -180)
            {
                return new BadRequestObjectResult("Invalid post data.");
            }
            newPost.PosterId = userId;

            // Create the HttpRequestMessage and add the db_token to the Authorization header
            var endpoint = $"{_baseApiUrl}/api/UpdatePost";
            if (string.IsNullOrEmpty(newPost.PostId))
            {   // Create the post if there is no ID
                endpoint = $"{_baseApiUrl}/api/CreatePost";
            }

            string body = JsonSerializer.Serialize(newPost);
            var (error, response) = await _httpRequestHandler.MakeHttpPostRequest(endpoint, body, db_token, userId.ToString());
            if (!error)
            {
                var dbResponseData = JsonSerializer.Deserialize<DbLayerResponse>(response);

                string? postId = dbResponseData?.PostId;
                if (string.IsNullOrEmpty(postId))
                {
                    _logger.LogError("Post ID not found in the response from the DB layer.");
                    return new ObjectResult("Post ID not found in the response from the DB layer.")
                    {
                        StatusCode = StatusCodes.Status500InternalServerError
                    };
                }

                return new OkObjectResult(response);
            }
            else
            {
                return new ObjectResult("Error connecting to the DB layer: " + response)
                {
                    StatusCode = StatusCodes.Status500InternalServerError
                };
            }
        }
    }
}
