using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using Microsoft.AspNetCore.Http;

namespace GoRideShare
{
    public class PollMessages
    {
        private readonly ILogger<PollMessages> _logger;
        private readonly string? _baseApiUrl;
        private readonly IHttpRequestHandler _httpRequestHandler;

        public PollMessages(ILogger<PollMessages> logger, IHttpRequestHandler httpRequestHandler)
        {
            _logger = logger;
            _httpRequestHandler = httpRequestHandler;
            _baseApiUrl = Environment.GetEnvironmentVariable("BASE_API_URL");
        }

        // This function is triggered by an HTTP GET request to fetch a conversation from the DB layer
        [Function("PollMessages")]
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
            if (!req.Query.TryGetValue("timeStamp", out var timeStamp))
            {
                return new BadRequestObjectResult("Missing the following query param: \'timeStamp\'");
            }
            
            // Create the HttpRequestMessage and add the db_token to the Authorization header
            var endpoint = $"{_baseApiUrl}/api/PollMessages?conversationId={conversationId}&timeStamp={timeStamp}";

            var (error, response) = await _httpRequestHandler.MakeHttpGetRequest(endpoint, db_token, userId.ToString());
            (error, response) = FakeHttpGetRequest(endpoint, db_token, userId.ToString());
            if (!error)
            {
                var dbResponseData = JsonSerializer.Deserialize<List<Message>>(response);
                // Validate all the conversation IDs match
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
    
        private (bool, string) FakeHttpGetRequest(string endpoint, string? dbToken, string userId)
        {
            var messages = new List<Message>();
            messages.Add(
                new Message {
                    TimeStamp = DateTime.Now.AddMinutes(-1),
                    SenderId = "guyka1-3uy127r3113e1v-c12d1v",
                    ConversationId = "fauwe1-1nuoih13131uda-g78o11",
                    Contents = "Hello"
                }       
            );            
            messages.Add(
                new Message {
                    TimeStamp = DateTime.Now,
                    SenderId = "guyka1-3uy127r3113e1v-c12d1v",
                    ConversationId = "fauwe1-1nuoih13131uda-g78o11",
                    Contents = "I would like to join you on the trip!"
                }       
            );
            var response = JsonSerializer.Serialize(
                messages
            );
            return (false, response);
        }
    }
}
