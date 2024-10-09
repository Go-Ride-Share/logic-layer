using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using System.Net.Http.Headers;

namespace GoRideShare
{
    public class GetPosts(ILogger<GetPosts> logger)
    {
        private static readonly HttpClient _httpClient = new HttpClient();
        private readonly ILogger<GetPosts> _logger = logger;
        private readonly string? _baseApiUrl = Environment.GetEnvironmentVariable("BASE_API_URL");

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
           
            // Create the HttpRequestMessage and add the db_token to the Authorization header
            var requestMessage = new HttpRequestMessage(HttpMethod.Get, $"{_baseApiUrl}/api/GetPosts?userId={posterId}"){};
            requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", db_token);
            requestMessage.Headers.Add("X-User-ID", userId.ToString());

            // Call the backend API to verify the login credentials
            var dbLayerResponse = await _httpClient.SendAsync(requestMessage);

            // Check if the backend API response indicates success
            if (dbLayerResponse.IsSuccessStatusCode)
            {
                try
                {
                    var dbResponseContent = await dbLayerResponse.Content.ReadAsStringAsync();
                    var posts = JsonSerializer.Deserialize<List<PostDetails>>(dbResponseContent);

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
                _logger.LogError("Failed to get posts: " + errorMessage);
                return new BadRequestObjectResult("Failed to get posts: " + errorMessage);
            }
        }
    }
}
