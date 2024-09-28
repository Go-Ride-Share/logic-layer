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
        private readonly JwtTokenHandler _jwtTokenHandler = new JwtTokenHandler(Environment.GetEnvironmentVariable("JWT_SECRET_KEY"), "GoRideShare", "GoRideShareAPI");

        [Function("VerifyLoginCredentials")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequest req)
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var userData = JsonSerializer.Deserialize<UserRegistrationInfo>(requestBody);
            _logger.LogInformation($"Raw Request Body: {JsonSerializer.Serialize(requestBody)}");

            if (userData == null || string.IsNullOrEmpty(userData.Email))
            {
                return new BadRequestObjectResult("Invalid user data.");
            }

            // var dbLayerResponse = await _httpClient.PostAsync("http://localhost:7071/api/VerifyLoginCredentials",
            var dbLayerResponse = await _httpClient.PostAsync("https://test-dp-func-db.azurewebsites.net/api/VerifyLoginCredentials",
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
                return new BadRequestObjectResult("Error! Incorrect email or password.");
            }
        }

        // public IActionResult Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequest req)
        // {
        //     LoginCredentials loginCredentials = new("some email", "cc3f4fd9608d575655ed31844b2349cf37be8ec5e4b0ec8ba9994fbc6653666f");
        //     string json = JsonSerializer.Serialize<LoginCredentials>(loginCredentials);

        //     _logger.LogInformation("C# HTTP trigger function processed a request.");
        //     return new ContentResult
        //     {
        //         Content = json,
        //         ContentType = "application/json",
        //         StatusCode = StatusCodes.Status200OK
        //     };
        // }
    }
}
