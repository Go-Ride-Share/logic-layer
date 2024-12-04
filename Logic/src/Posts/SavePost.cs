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
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "Posts")] HttpRequest req)
        {
            // If validation result is not null, return the bad request result
            var validationResult = Utilities.ValidateHeadersAndTokens(req.Headers, out string userId, out string db_token);
            if (validationResult != null)
            {
                return validationResult;
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
            var (invalid, errorMessage) = newPost.validate();
            if (invalid)
            {
                _logger.LogError($"PostDetails are not valid: {errorMessage}");
                return new BadRequestObjectResult(errorMessage);
            }
            newPost.PosterId = userId;

            // Create the HttpRequestMessage and add the db_token to the Authorization header
            var endpoint = $"{_baseApiUrl}/api/posts";
            if (string.IsNullOrEmpty(newPost.PostId))
            {   
                string body = JsonSerializer.Serialize(newPost);
                var (error, response) = await _httpRequestHandler.MakeHttpPostRequest(endpoint, body, db_token, userId.ToString());
                if (!error)
                {
                    var dbResponseData = JsonSerializer.Deserialize<DbLayerResponse>(response);

                    string? id = dbResponseData?.Id;
                    if (string.IsNullOrEmpty(id))
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
            else
            {
                endpoint += "/" + newPost.PostId;
                string body = JsonSerializer.Serialize(newPost);
                var (error, response) = await _httpRequestHandler.MakeHttpPatchRequest(endpoint, body, db_token, userId.ToString());
                if (!error)
                {
                    var dbResponseData = JsonSerializer.Deserialize<DbLayerResponse>(response);

                    string? id = dbResponseData?.Id;
                    if (string.IsNullOrEmpty(id))
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
}
