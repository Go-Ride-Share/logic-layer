using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;

namespace GoRideShare
{
    // This class handles the login verification process for users
    public class VerifyLoginCredentials(ILogger<VerifyLoginCredentials> logger)
    {
        private static readonly HttpClient _httpClient = new HttpClient();
        private readonly ILogger<VerifyLoginCredentials> _logger = logger;
        // Base URL for the API (retrieved from environment variables)
        private readonly string? _baseApiUrl = Environment.GetEnvironmentVariable("BASE_API_URL");
        // JWT Token handler for generating tokens upon successful login
        private readonly JwtTokenHandler _jwtTokenHandler = new JwtTokenHandler(Environment.GetEnvironmentVariable("JWT_SECRET_KEY"), "GoRideShare", "GoRideShareAPI");

        // This function is triggered by an HTTP POST request to verify login credentials
        [Function("VerifyLoginCredentials")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequest req)
        {
            // Read the request body to get the user's login data (email and password hash)
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var userData = JsonSerializer.Deserialize<LoginCredentials>(requestBody);
            
            _logger.LogInformation($"Raw Request Body: {requestBody}");

            // Validate if the email and password hash are provided
            if (userData == null || string.IsNullOrEmpty(userData.Email) || string.IsNullOrEmpty(userData.PasswordHash))
            {
                return new BadRequestObjectResult("Incomplete user data.");
            }

            // Call the backend API to verify the login credentials
            var dbLayerResponse = await _httpClient.PostAsync($"{_baseApiUrl}/api/VerifyLoginCredentials",
                new StringContent(JsonSerializer.Serialize(userData), Encoding.UTF8, "application/json"));

            // Check if the backend API response indicates success
            if (dbLayerResponse.IsSuccessStatusCode)
            {
                // Generate a JWT token for the user if login is successful
                var token = _jwtTokenHandler.GenerateToken(userData.Email);

                // If token generation is successful, return it in the response
                if(token != "")
                {
                    return new OkObjectResult(new {Token = token});
                }

                // If token generation fails, return 500 Internal Server Error
                return new ContentResult{StatusCode = StatusCodes.Status500InternalServerError}; 
            }
            else
            {
                // Log the error message and return a 400 Bad Request response
                var errorMessage = await dbLayerResponse.Content.ReadAsStringAsync();
                _logger.LogError("Failed to login into the account: " + errorMessage);
                return new BadRequestObjectResult("Failed to login into the account: " + errorMessage);
            }
        }
    }
}
