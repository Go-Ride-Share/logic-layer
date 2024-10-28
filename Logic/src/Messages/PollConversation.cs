using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using Microsoft.AspNetCore.Http;

namespace GoRideShare
{
    public class PollConversation
    {
        private readonly ILogger<PollConversation> _logger;
        private readonly string? _baseApiUrl;
        private readonly IHttpRequestHandler _httpRequestHandler;

        public PollConversation(ILogger<PollConversation> logger, IHttpRequestHandler httpRequestHandler)
        {
            _logger = logger;
            _httpRequestHandler = httpRequestHandler;
            _baseApiUrl = Environment.GetEnvironmentVariable("BASE_API_URL");
        }

        // This function is triggered by an HTTP GET request to fetch a conversation from the DB layer
        [Function("PollConversation")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest req)
        {
            // If validation result is not null, return the bad request result
            var validationResult = Utilities.ValidateHeaders(req.Headers, out string userId, out string db_token);
            if (validationResult != null)
            {
                return validationResult;
            }

            // Read the conversationId from the query params
            if (!req.Query.TryGetValue("conversationId", out var conversationId))
            {
                return new BadRequestObjectResult("Missing the following query param: \'conversationId\'");
            }

            // Timestamp is an optional parameter to limit the response size
            var endpoint = $"{_baseApiUrl}/api/PollConversation?conversationId={conversationId}";
            if (req.Query.TryGetValue("timeStamp", out var timeStamp))
            {
                endpoint = $"{_baseApiUrl}/api/PollConversation?conversationId={conversationId}&timeStamp={timeStamp}";
            }

            // Create the HttpRequestMessage and add the db_token to the Authorization header
            var (error, response) = await _httpRequestHandler.MakeHttpGetRequest(endpoint, db_token, userId.ToString());
            (error, response) = FakeHttpGetRequest(endpoint, db_token, userId.ToString());
            if (!error)
            {
                var dbResponseData = JsonSerializer.Deserialize<Conversation>(response);
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

        private (bool, string) FakeHttpGetRequest(string endpoint, string? db_token, string userId)
        {
            var messages = new List<Message>();
            messages.Add(
                new Message(
                    timeStamp: DateTime.Now.AddMinutes(-1),
                    senderId: "bbbbb-bbbbbbbbbb-bbbbb",
                    contents: "Hello"
                )
            );
            messages.Add(
                new Message(
                    timeStamp: DateTime.Now,
                    senderId: "bbbbb-bbbbbbbbbb-bbbbb",
                    contents: "I would like to join you on the trip!"
                )
            );
            var user = new User("bbbbb-bbbbbbbbbb-bbbbb", "Bob", Images.getImage());
            var response = JsonSerializer.Serialize(
                new Conversation
                (
                    user: user,
                    conversationId: "ccccc-cccccccccc-ccccc",
                    messages: messages,
                    postId: "aaaaa-aaaaaaaaaa-aaaaa"
                )
            );
            return (false, response);
        }
    }
}
