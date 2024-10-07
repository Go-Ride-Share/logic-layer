using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;

namespace GoRideShare
{
    public class SavePost(ILogger<SavePost> logger)
    {
        private static readonly HttpClient _httpClient = new HttpClient();
        private readonly ILogger<SavePost> _logger = logger;
        private readonly string? _baseApiUrl = Environment.GetEnvironmentVariable("BASE_API_URL");

        // This function is triggered by an HTTP POST request to create a new post
        [Function("SavePost")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequest req)
        {
            // Read the request body to get the user's registration information
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var newPost = JsonSerializer.Deserialize<PostDetails>(requestBody);
            _logger.LogInformation($"Raw Request Body: {JsonSerializer.Serialize(requestBody)}");

            // Validate if essential post data is present
            if (newPost == null || string.IsNullOrEmpty(newPost.Name) ||
                string.IsNullOrEmpty(newPost.Description) || 
                string.IsNullOrEmpty(newPost.PosterId) || 
                90 < newPost.OriginLat || newPost.OriginLat < -90 ||
                90 < newPost.DestinationLat || newPost.DestinationLat < -90 ||
                180 < newPost.OriginLng || newPost.OriginLng < -180 ||
                180 < newPost.DestinationLng || newPost.DestinationLng < -180)
            {
                return new BadRequestObjectResult("Incomplete post data.");
            }

            // Create the HttpRequestMessage and add the db_token to the Authorization header
            var endpoint = $"{_baseApiUrl}/api/UpdatePost";
            if ( string.IsNullOrEmpty(newPost.PostId) ) 
            {   // Create the post if there is no ID
                endpoint = $"{_baseApiUrl}/api/CreatePost";
            }

            var requestMessage = new HttpRequestMessage(HttpMethod.Post, endpoint)
            {
                Content = new StringContent(JsonSerializer.Serialize(newPost), Encoding.UTF8, "application/json")
            };

            // Call the backend API to verify the login credentials
            var dbLayerResponse = await _httpClient.SendAsync(requestMessage);

            // Check if the backend API response indicates success
            if (dbLayerResponse.IsSuccessStatusCode)
            {
                try
                {
                    // Extract the response content and deserialize it to get the user_id and post_id
                    var dbResponseContent = await dbLayerResponse.Content.ReadAsStringAsync();
                    var dbResponseData = JsonSerializer.Deserialize<DbLayerResponse>(dbResponseContent);

                    string? postId = dbResponseData?.PostId;
                    if (string.IsNullOrEmpty(postId))
                    {
                        _logger.LogError("Post ID not found in the response from the DB layer.");
                        return new StatusCodeResult(StatusCodes.Status500InternalServerError);
                    }

                    return new OkObjectResult(new
                    {
                        postId = postId,
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogError($"An error occurred while processing the request: {ex.Message}");
                    return new StatusCodeResult(StatusCodes.Status500InternalServerError);
                }
            }
            else
            {
                // Log the error message and return a 400 Bad Request response
                var errorMessage = await dbLayerResponse.Content.ReadAsStringAsync();
                _logger.LogError("Failed to create account: " + errorMessage);
                return new BadRequestObjectResult("Failed to create account: " + errorMessage);
            }
        }
    }
}
