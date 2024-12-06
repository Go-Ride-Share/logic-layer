using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Azure.Data.Tables;

namespace GoRideShare
{
    public class CreateAccount(ILogger<CreateAccount> logger)
    {
        private static readonly HttpClient _httpClient = new HttpClient();
        private readonly ILogger<CreateAccount> _logger = logger;
        // Base URL for the API (retrieved from environment variables)
        private readonly string? _baseApiUrl = Environment.GetEnvironmentVariable("BASE_API_URL");

        // This function is triggered by an HTTP POST request to create a new user
        [Function("UserCreate")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "Users")] HttpRequest req)
        {
            // Read the request body to get the user's registration information
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            _logger.LogInformation($"Raw Request Body: {requestBody}");

            UserRegistrationInfo? userData = null;
            try 
            {
                userData = JsonSerializer.Deserialize<UserRegistrationInfo>(requestBody);
            }
            catch (JsonException e)
            {
                _logger.LogError(e.Message);
                return new BadRequestObjectResult("There is a porblem in your request.");
            }

            // Validate if essential user data is present
            if (userData == null || string.IsNullOrEmpty(userData.Email) ||
            string.IsNullOrEmpty(userData.Name) || string.IsNullOrEmpty(userData.PasswordHash))
            {
                _logger.LogError("Incomplete user data.");
                return new BadRequestObjectResult("Incomplete user data.");
            }

            // Generate the OAuth 2.0 token for db_layer
            var jwtTokenHandler = new JwtTokenHandler(Environment.GetEnvironmentVariable("OAUTH_CLIENT_ID_DB"),
                                                    Environment.GetEnvironmentVariable("OAUTH_CLIENT_SECRET_DB"),
                                                    Environment.GetEnvironmentVariable("OAUTH_TENANT_ID_DB"),
                                                    Environment.GetEnvironmentVariable("OAUTH_SCOPE_DB"));
            string db_token = await jwtTokenHandler.GenerateTokenAsync();

            // Send request to db
            var requestMessage = new HttpRequestMessage(HttpMethod.Post, $"{_baseApiUrl}/api/users")
            {
                Content = new StringContent(JsonSerializer.Serialize(userData), Encoding.UTF8, "application/json")
            };
            requestMessage.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", db_token);
            var dbLayerResponse = await _httpClient.SendAsync(requestMessage);

            // Check db response
            if (dbLayerResponse.IsSuccessStatusCode)
            {
                try
                {
                    // Extract the response content and deserialize it to get the user_id
                    string dbResponseContent = await dbLayerResponse.Content.ReadAsStringAsync();
                    DbLayerResponse? dbResponseData = JsonSerializer.Deserialize<DbLayerResponse>(dbResponseContent);

                    string? userId = dbResponseData?.UserId;
                    if (string.IsNullOrEmpty(userId))
                    {
                        _logger.LogError("Failed to create account.");
                        return new ObjectResult("Failed to create account.")
                        {
                            StatusCode = StatusCodes.Status500InternalServerError
                        };                    
                    }

                    // Initialize JwtTokenHandler and validate environment variables for logic_layer
                    jwtTokenHandler = new JwtTokenHandler(Environment.GetEnvironmentVariable("OAUTH_CLIENT_ID"),
                                                            Environment.GetEnvironmentVariable("OAUTH_CLIENT_SECRET"),
                                                            Environment.GetEnvironmentVariable("OAUTH_TENANT_ID"),
                                                            Environment.GetEnvironmentVariable("OAUTH_SCOPE"));

                    // Generate the OAuth 2.0 token for logic_layer
                    string logic_token = await jwtTokenHandler.GenerateTokenAsync();

                    await SecurityGuard.SaveUserTokens(userId, logic_token, db_token);

                    // Return both OAuth 2.0 tokens, the user_id, and the photo to the client
                    return new OkObjectResult(new
                    {
                        User_id = userId,
                        Logic_token = logic_token,
                        db_token = db_token,
                        Photo = userData.Photo ?? null
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
