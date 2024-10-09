using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;
using System.Net.Http.Headers;
using System.Text.Json;

namespace GoRideShare
{
    public class GetUser(ILogger<GetUser> logger)
    {
        private static readonly HttpClient _httpClient = new HttpClient();
        private readonly ILogger<GetUser> _logger = logger;
        // Base URL for the API (retrieved from environment variables)
        private readonly string? _baseApiUrl = Environment.GetEnvironmentVariable("BASE_API_URL");

        [Function("GetUser")]
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

            // Create the HttpRequestMessage and add the db_token to the Authorization header
            var requestMessage = new HttpRequestMessage(HttpMethod.Get, $"{_baseApiUrl}/api/GetUser");
            requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", db_token);
            requestMessage.Headers.Add("X-User-ID", userId.ToString());

            try
            {
                // Send the request to the database layer
                var response = await _httpClient.SendAsync(requestMessage);

                // Check if the response is successful
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return new OkObjectResult(content);
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return new NotFoundResult();
                }
                else
                {
                    _logger.LogError($"Error fetching user data from database layer: {response.ReasonPhrase}");
                    return new StatusCodeResult(StatusCodes.Status500InternalServerError);
                }
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError($"HTTP request error: {ex.Message}");
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
        }
    }
}
