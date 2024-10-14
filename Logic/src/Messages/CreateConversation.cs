using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using Microsoft.AspNetCore.Http;

namespace GoRideShare
{
    public class CreateConversation
    {
        private readonly ILogger<CreateConversation> _logger;
        private readonly string? _baseApiUrl;
        private readonly IHttpRequestHandler _httpRequestHandler;

        public CreateConversation(ILogger<CreateConversation> logger, IHttpRequestHandler httpRequestHandler)
        {
            _logger = logger;
            _httpRequestHandler = httpRequestHandler;
            _baseApiUrl = Environment.GetEnvironmentVariable("BASE_API_URL");
        }

        // This function is triggered by an HTTP POST request to create a new post
        [Function("CreateConversation")]
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

            // Read the request body to get the user's registration information
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            Conversation? newConvo = null;
            try
            {
                newConvo = JsonSerializer.Deserialize<Conversation>(requestBody);
            }
            catch (JsonException ex)
            {
                _logger.LogError($"JSON deserialization failed: {ex.Message}");
            }
            _logger.LogInformation($"Raw Request Body: {JsonSerializer.Serialize(requestBody)}");

            // Validate if essential post data is present
            if (newConvo == null)
            {
                return new BadRequestObjectResult("Incomplete Convo. data.");
            }
            else if (string.IsNullOrEmpty(newConvo.UserId))
            {
                return new BadRequestObjectResult("A userId is required");
            }
            newConvo.UserId = userId;

            // Create the HttpRequestMessage and add the db_token to the Authorization header
            var endpoint = $"{_baseApiUrl}/api/CreateConversation";

            string body = JsonSerializer.Serialize(newConvo);
            var (error, response) = await _httpRequestHandler.MakeHttpPostRequest(endpoint, body, db_token, userId.ToString());
            (error, response) = FakeHttpPostRequest(endpoint, body, db_token, userId.ToString());
            if (!error)
            {
                var dbResponseData = JsonSerializer.Deserialize<DbLayerResponse>(response);

                string? id = dbResponseData?.Id;
                if (string.IsNullOrEmpty(id))
                {
                    _logger.LogError("Conversation ID not found in the response from the DB layer.");
                    return new ObjectResult("Conversation ID not found in the response from the DB layer.")
                    {
                        StatusCode = StatusCodes.Status500InternalServerError
                    };
                }

                return new OkObjectResult(response);
            }
            else
            {
                return new ObjectResult("Error connecting to the DB layer: " + response)
                {
                    StatusCode = StatusCodes.Status500InternalServerError
                };
            }
        }
    
        private (bool, string) FakeHttpPostRequest(string endpoint, string body, string? dbToken, string userId)
        {
            var response = JsonSerializer.Serialize(
                new DbLayerResponse
                {
                    Id = "guyka1-3uy127r3113e1v-c12d1v",
                }
            );
            return (false, response);
        }
    }
}
