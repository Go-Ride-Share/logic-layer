using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using Microsoft.AspNetCore.Http;

namespace GoRideShare
{
    public class PostMessage 
    {
        private readonly ILogger<PostMessage> _logger;
        private readonly string? _baseApiUrl;
        private readonly IHttpRequestHandler _httpRequestHandler;

        public PostMessage(ILogger<PostMessage> logger, IHttpRequestHandler httpRequestHandler)
        {
            _logger = logger;
            _httpRequestHandler = httpRequestHandler;
            _baseApiUrl = Environment.GetEnvironmentVariable("BASE_API_URL");
        }

        // This function is triggered by an HTTP POST request to create a new message in a conversation
        [Function("MessagesPost")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "Messages")] HttpRequest req)
        {
            // If validation result is not null, return the bad request result
            var validationResult = Utilities.ValidateHeaders(req.Headers, out string userId, out string db_token);
            if (validationResult != null)
            {
                return validationResult;
            }

            // Validate if essential message data is present
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            IncomingMessageRequest? newMessage = null;
            try
            {
                newMessage = JsonSerializer.Deserialize<IncomingMessageRequest>(requestBody);
                if (newMessage != null)
                {
                    var (invalid, errorMessage) = newMessage.validate();
                    if (invalid)
                    {
                        _logger.LogInformation(errorMessage);
                        return new BadRequestObjectResult(errorMessage);
                    }
                }
                else
                {
                    string errorMessage = "Failed to deserialize request body.";
                    _logger.LogInformation(errorMessage);
                    return new BadRequestObjectResult(errorMessage);
                }
            }
            catch (JsonException ex)
            {
                _logger.LogError($"JSON deserialization failed: {ex.Message}");
                return new BadRequestObjectResult("Incomplete Message data.");
            }
            _logger.LogInformation($"Raw Request Body: {JsonSerializer.Serialize(requestBody)}");

            // Add fields required by the DB Layer
            var message = new OutgoingMessageRequest(
                userId: userId.ToString(),
                timeStamp: DateTime.Now,
                conversationId: newMessage.ConversationId,
                contents: newMessage.Contents
            );

            // Create the HttpRequestMessage and add the db_token to the Authorization header
            var endpoint = $"{_baseApiUrl}/api/messages";
            string body = JsonSerializer.Serialize(message);
            var (error, response) = await _httpRequestHandler.MakeHttpPostRequest(endpoint, body, db_token, userId.ToString());
            if (!error)
            {
                var dbResponseData = JsonSerializer.Deserialize<DbLayerResponse>(response);

                string? id = dbResponseData?.Id;
                if (string.IsNullOrEmpty(id))
                {
                    _logger.LogError("Conversation ID not returned in the response from the DB layer.");
                    return new ObjectResult("Conversation ID not returned in the response from the DB layer.")
                    {
                        StatusCode = StatusCodes.Status500InternalServerError
                    };
                }

                return new OkObjectResult(dbResponseData);
            }
            else
            {
                _logger.LogError($"Error: {response}");
                return new ObjectResult("Message Failed: " + response)
                {
                    StatusCode = StatusCodes.Status500InternalServerError
                };
            }
        }
    }
}
