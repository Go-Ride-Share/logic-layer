using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using Microsoft.AspNetCore.Http;

namespace GoRideShare
{
    public class GetAllConversations
    {
        private readonly ILogger<GetAllConversations> _logger;
        private readonly string? _baseApiUrl;
        private readonly IHttpRequestHandler _httpRequestHandler;

        public GetAllConversations(ILogger<GetAllConversations> logger, IHttpRequestHandler httpRequestHandler)
        {
            _logger = logger;
            _httpRequestHandler = httpRequestHandler;
            _baseApiUrl = Environment.GetEnvironmentVariable("BASE_API_URL");
        }

        // This function is triggered by an HTTP GET request to fetch a conversation from the DB layer
        [Function("GetAllConversations")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest req)
        {
            // If validation result is not null, return the bad request result
            var validationResult = Utilities.ValidateHeaders(req.Headers, out string userId, out string db_token);
            if (validationResult != null)
            {
                return validationResult;
            }

            // Create the HttpRequestMessage and add the db_token to the Authorization header
            var endpoint = $"{_baseApiUrl}/api/GetAllConversations";
            var (error, response) = await _httpRequestHandler.MakeHttpGetRequest(endpoint, db_token, userId.ToString());
            if (!error)
            {
                var dbResponseData = JsonSerializer.Deserialize<List<Conversation>>(response);
                if (dbResponseData == null)
                {
                    _logger.LogError("Invalid conversations data received from the DB layer.");
                    return new ObjectResult("Invalid conversations data received from the DB layer.")
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
