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
        private readonly JwtTokenHandler _jwtTokenHandler = new JwtTokenHandler(Environment.GetEnvironmentVariable("JWT_SECRET_KEY"), "GoRideShare", "GoRideShareAPI");

        [Function("CreateUser")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequest req)
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var userData = JsonSerializer.Deserialize<UserRegistrationInfo>(requestBody);
            _logger.LogInformation($"Raw Request Body: {JsonSerializer.Serialize(requestBody)}");

            if (userData == null || string.IsNullOrEmpty(userData.Email))
            {
                return new BadRequestObjectResult("Invalid user data.");
            }

            // var dbLayerResponse = await _httpClient.PostAsync("http://localhost:7071/api/CreateUser",
            var dbLayerResponse = await _httpClient.PostAsync("https://test-dp-func-db.azurewebsites.net/api/CreateUser",
                new StringContent(JsonSerializer.Serialize(userData), Encoding.UTF8, "application/json"));

            if (dbLayerResponse.IsSuccessStatusCode)
            {
                // Generate JWT Token
                var token = _jwtTokenHandler.GenerateToken(userData.Email);

                // return response;
                return new OkObjectResult(new {Token = token});
            }
            else
            {
                var errorMessage = await dbLayerResponse.Content.ReadAsStringAsync();
                _logger.LogError("Failed to create account: " + errorMessage);
                return new BadRequestObjectResult("Error creating user account.");
            }
        }
    }
}
