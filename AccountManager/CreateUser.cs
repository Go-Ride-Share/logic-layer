using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;

namespace GoRideShare
{
    public class CreateAccount(ILogger<CreateAccount> logger)
    {
        private static readonly HttpClient _httpClient = new HttpClient();
        private readonly ILogger<CreateAccount> _logger = logger;
        // Base URL for the API (retrieved from environment variables)
        private readonly string? _baseApiUrl = Environment.GetEnvironmentVariable("BASE_API_URL");
        // JWT Token handler for generating tokens upon successful account creation
        private readonly JwtTokenHandler _jwtTokenHandler = new JwtTokenHandler(Environment.GetEnvironmentVariable("JWT_SECRET_KEY"), "GoRideShare", "GoRideShareAPI");

        // This function is triggered by an HTTP POST request to create a new user
        [Function("CreateUser")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequest req)
        {
            // Read the request body to get the user's registration information
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var userData = JsonSerializer.Deserialize<UserRegistrationInfo>(requestBody);
            _logger.LogInformation($"Raw Request Body: {JsonSerializer.Serialize(requestBody)}");

            // Validate if essential user data is present
            if (userData == null || string.IsNullOrEmpty(userData.Email) || 
            string.IsNullOrEmpty(userData.Name) || string.IsNullOrEmpty(userData.PasswordHash)
            || string.IsNullOrEmpty(userData.PhoneNumber))
            {
                return new BadRequestObjectResult("Incomplete user data.");
            }

            // Call the backend API to insert the user data into the database
            var dbLayerResponse = await _httpClient.PostAsync($"{_baseApiUrl}/api/CreateUser",
                new StringContent(JsonSerializer.Serialize(userData), Encoding.UTF8, "application/json"));

            // Check if the backend API response indicates success
            if (dbLayerResponse.IsSuccessStatusCode)
            {
                // Generate a JWT token for the user after successful account creation
                var token = _jwtTokenHandler.GenerateToken(userData.Email);
                
                // If token generation is successful, return the token in the response
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
                _logger.LogError("Failed to create account: " + errorMessage);
                return new BadRequestObjectResult("Failed to create account: " + errorMessage);
            }
        }
    }
}
