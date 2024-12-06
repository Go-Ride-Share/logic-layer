using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;

namespace GoRideShare
{
    // This class handles the login verification process for users
    public class LoginUser(ILogger<LoginUser> logger)
    {
        private static readonly HttpClient _httpClient = new HttpClient();
        private readonly ILogger<LoginUser> _logger = logger;
        // Base URL for the API (retrieved from environment variables)
        private readonly string? _baseApiUrl = Environment.GetEnvironmentVariable("BASE_API_URL");

        // This function is triggered by an HTTP POST request to verify login credentials
        [Function("PasswordLogin")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "Users/PasswordLogin")] HttpRequest req)
        {
            // Read the request body to get the user's login data (email and password hash)
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            _logger.LogInformation($"Raw Request Body: {requestBody}");

            LoginCredentials? userData = null;
            try 
            {
                userData = JsonSerializer.Deserialize<LoginCredentials>(requestBody);
            }
            catch (JsonException e)
            {
                _logger.LogError(e.Message);
                return new BadRequestObjectResult("There is a porblem in your request.");
            }

            // Validate if the email and password hash are provided
            if (userData == null || string.IsNullOrEmpty(userData.Email) || string.IsNullOrEmpty(userData.PasswordHash))
            {
                return new BadRequestObjectResult("Incomplete user data.");
            }

            // Initialize JwtTokenHandler and validate environment variables
            var jwtTokenHandler = new JwtTokenHandler(Environment.GetEnvironmentVariable("OAUTH_CLIENT_ID_DB"),
                                                    Environment.GetEnvironmentVariable("OAUTH_CLIENT_SECRET_DB"),
                                                    Environment.GetEnvironmentVariable("OAUTH_TENANT_ID_DB"),
                                                    Environment.GetEnvironmentVariable("OAUTH_SCOPE_DB"));

            // Generate the OAuth 2.0 token for db_layer
            string db_token = await jwtTokenHandler.GenerateTokenAsync();

            // Create the HttpRequestMessage and add the db_token to the Authorization header
            var requestMessage = new HttpRequestMessage(HttpMethod.Post, $"{_baseApiUrl}/api/users/PasswordLogin")
            {
                Content = new StringContent(JsonSerializer.Serialize(userData), Encoding.UTF8, "application/json")
            };

            // Add the db_token to the Authorization header
            requestMessage.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", db_token);

            // Call the backend API to verify the login credentials
            var dbLayerResponse = await _httpClient.SendAsync(requestMessage);

            // Check if the backend API response indicates success
            if (dbLayerResponse.IsSuccessStatusCode)
            {
                try
                {
                    // Extract the response content and deserialize it to get the user_id
                    var dbResponseContent = await dbLayerResponse.Content.ReadAsStringAsync();
                    var dbResponseData = JsonSerializer.Deserialize<DbLayerResponse>(dbResponseContent);

                    string? userId = dbResponseData?.UserId;
                    string? photo = dbResponseData?.Photo;

                    if (string.IsNullOrEmpty(userId))
                    {
                        _logger.LogError("User ID not found in the response from the DB layer.");
                        return new ObjectResult("Invalid login credentials.")
                        {
                            StatusCode = StatusCodes.Status401Unauthorized
                        };
                    }

                    // Initialize JwtTokenHandler and validate environment variables
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
                        Photo = photo
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
                _logger.LogError("Failed to login into the account: " + errorMessage);
                return new BadRequestObjectResult("Failed to login into the account: " + errorMessage);
            }
        }
    }
}
