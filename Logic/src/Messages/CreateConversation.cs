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
            // If validation result is not null, return the bad request result
            var validationResult = Utilities.ValidateHeaders(req.Headers, out string userId, out string db_token);
            if (validationResult != null)
            {
                return validationResult;
            }

            // Read the request body to get the user's registration information
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            IncomingConversationRequest? newConvo = null;
            try
            {
                newConvo = JsonSerializer.Deserialize<IncomingConversationRequest>(requestBody);
                var (invalid, errorMessage) = newConvo.validate();
                if (invalid)
                {
                    return new BadRequestObjectResult(errorMessage);
                }
            }
            catch (JsonException ex)
            {
                _logger.LogError($"JSON deserialization failed: {ex.Message}");
                return new BadRequestObjectResult("Incomplete Conversation Request data.");
            }
            _logger.LogInformation($"Raw Request Body: {JsonSerializer.Serialize(requestBody)}");

            // Add fields required by the DB Layer
            var conversation = new OutgoingConversationRequest(
                userId: newConvo.UserId,
                timeStamp: DateTime.Now,
                contents: newConvo.Contents
            );

            // Create the HttpRequestMessage and add the db_token to the Authorization header
            var endpoint = $"{_baseApiUrl}/api/conversations";
            string body = JsonSerializer.Serialize(conversation);
            var (error, response) = await _httpRequestHandler.MakeHttpPostRequest(endpoint, body, db_token, userId.ToString());
            if (!error)
            {
                var dbResponseData = JsonSerializer.Deserialize<Conversation>(response);
                // Validation
                if (dbResponseData == null || string.IsNullOrWhiteSpace(dbResponseData.ConversationId) ||
                    dbResponseData.Messages == null ||
                    dbResponseData.Messages.Count == 0 ||
                    dbResponseData.User == null)
                {
                    _logger.LogError("Invalid/Incomplete conversation data received from the DB layer.");
                    return new ObjectResult("Invalid conversation data received from the DB layer.")
                    {
                        StatusCode = StatusCodes.Status400BadRequest
                    };
                }
                return new OkObjectResult(dbResponseData);
            }
            else
            {
                _logger.LogError($"Error: {response}");
                return new ObjectResult("Error connecting to the DB layer: " + response)
                {
                    StatusCode = StatusCodes.Status500InternalServerError
                };
            }
        }
    }
}
