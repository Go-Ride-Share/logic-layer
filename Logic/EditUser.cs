using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text;

namespace GoRideShare
{
    public class EditUser(ILogger<EditUser> logger)
    {
        private static readonly HttpClient _httpClient = new HttpClient();
        private readonly ILogger<EditUser> _logger = logger;
        // Base URL for the API (retrieved from environment variables)
        private readonly string? _baseApiUrl = Environment.GetEnvironmentVariable("BASE_API_URL");

        [Function("EditUser")]
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

            // Read the body content from the request
            string requestBody;
            using (var reader = new StreamReader(req.Body))
            {
                requestBody = await reader.ReadToEndAsync();
            }

            // Create the HttpRequestMessage and add the db_token to the Authorization header
            var requestMessage = new HttpRequestMessage(HttpMethod.Post, $"{_baseApiUrl}/api/EditUser");
            requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", db_token);
            requestMessage.Headers.Add("X-User-ID", userId.ToString());

            _logger.LogInformation($"REQQQQQQQQ: {requestMessage}");

            // Set the request content
            requestMessage.Content = new StringContent(requestBody, Encoding.UTF8, "application/json");

            try
            {
                // Send the request to the database layer
                var response = await _httpClient.SendAsync(requestMessage);

                // Check if the response is successful
                if (response.IsSuccessStatusCode)
                {
                    return new OkObjectResult(new { message = "User updated successfully" });
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
