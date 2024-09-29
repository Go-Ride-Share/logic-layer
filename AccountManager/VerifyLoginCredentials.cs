using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;

namespace GoRideShare
{
    public class VerifyLoginCredentials(ILogger<VerifyLoginCredentials> logger)
    {
        private static readonly HttpClient _httpClient = new HttpClient();
        private readonly ILogger<VerifyLoginCredentials> _logger = logger;
        private readonly string? _baseApiUrl = Environment.GetEnvironmentVariable("BASE_API_URL");
        private readonly JwtTokenHandler _jwtTokenHandler = new JwtTokenHandler(Environment.GetEnvironmentVariable("JWT_SECRET_KEY"), "GoRideShare", "GoRideShareAPI");

        [Function("VerifyLoginCredentials")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequest req)
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var userData = JsonSerializer.Deserialize<LoginCredentials>(requestBody);
            _logger.LogInformation($"Raw Request Body: {JsonSerializer.Serialize(requestBody)}");

            if (userData == null || string.IsNullOrEmpty(userData.Email) || string.IsNullOrEmpty(userData.PasswordHash))
            {
                return new BadRequestObjectResult("Incomplete user data.");
            }

            var dbLayerResponse = await _httpClient.PostAsync($"{_baseApiUrl}/api/VerifyLoginCredentials",
                new StringContent(JsonSerializer.Serialize(userData), Encoding.UTF8, "application/json"));

            if (dbLayerResponse.IsSuccessStatusCode)
            {
                var token = _jwtTokenHandler.GenerateToken(userData.Email);
                if(token != "")
                {
                    return new OkObjectResult(new {Token = token});
                }
                return new ContentResult{StatusCode = StatusCodes.Status500InternalServerError}; 
            }
            else
            {
                var errorMessage = await dbLayerResponse.Content.ReadAsStringAsync();
                _logger.LogError("Failed to login into the account: " + errorMessage);
                return new BadRequestObjectResult("Failed to login into the account: " + errorMessage);
            }
        }
    }
}
