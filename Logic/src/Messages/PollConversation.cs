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
            // Read the user ID and the db token from the headers
            if (!req.Headers.TryGetValue("X-User-ID", out var userId))
            {
                return new BadRequestObjectResult("Missing the following header: \'X-User-ID\'.");
            }
            if (!req.Headers.TryGetValue("X-Db-Token", out var db_token))
            {
                return new BadRequestObjectResult("Missing the following header \'X-Db-Token\'.");
            }

            // Read the conversationId from the query params
            if (!req.Query.TryGetValue("conversationId", out var conversationId))
            {
                return new BadRequestObjectResult("Missing the following query param: \'conversationId\'");
            }
            
            var endpoint = $"{_baseApiUrl}/api/PollConversation?conversationId={conversationId}";
            if (!req.Query.TryGetValue("timeStamp", out var timeStamp))
            {
                endpoint = $"{_baseApiUrl}/api/PollConversation?conversationId={conversationId}";
            } else {
                endpoint = $"{_baseApiUrl}/api/PollConversation?conversationId={conversationId}&timeStamp={timeStamp}";
            }

            // Create the HttpRequestMessage and add the db_token to the Authorization header
            var (error, response) = await _httpRequestHandler.MakeHttpGetRequest(endpoint, db_token, userId.ToString());
            (error, response) = FakeHttpGetRequest(endpoint, db_token, userId.ToString());
            if (!error)
            {
                var dbResponseData = JsonSerializer.Deserialize<Conversation>(response);
                // Validation
                return new OkObjectResult(dbResponseData);
            }
            else
            {
                return new ObjectResult("Error connecting to the DB layer: " + response)
                {
                    StatusCode = StatusCodes.Status500InternalServerError
                };
            }
        }
    
        private (bool, string) FakeHttpGetRequest(string endpoint, string? dbToken, string userId)
        {
            var messages = new List<Message>();
            messages.Add(
                new Message {
                    TimeStamp = DateTime.Now.AddMinutes(-1),
                    SenderId = "bbbbb-bbbbbbbbbb-bbbbb",
                    Contents = "Hello"
                }       
            );            
            messages.Add(
                new Message {
                    TimeStamp = DateTime.Now,
                    SenderId = "bbbbb-bbbbbbbbbb-bbbbb",
                    Contents = "I would like to join you on the trip!"
                }       
            );
            var user = new User("bbbbb-bbbbbbbbbb-bbbbb", "Bob", Images.getImage());
            var response = JsonSerializer.Serialize(
                new Conversation
                {
                    User = user,
                    ConversationId = "ccccc-cccccccccc-ccccc",
                    Messages = messages,
                    PostId = "aaaaa-aaaaaaaaaa-aaaaa",
                }
            );
            return (false, response);
        }
    }
}
