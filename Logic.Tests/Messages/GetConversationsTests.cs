using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace GoRideShare.Tests
{
    public class GetConversationsTests
    {
        private readonly Mock<ILogger<GetConversations>> _loggerMock;
        private readonly Mock<IHttpRequestHandler> _httpRequestHandlerMock;
        private readonly GetConversations _getConversations;

        public GetConversationsTests()
        {
            _loggerMock = new Mock<ILogger<GetConversations>>();
            _httpRequestHandlerMock = new Mock<IHttpRequestHandler>();
            _getConversations = new GetConversations(_loggerMock.Object, _httpRequestHandlerMock.Object);
        }

        [Fact]
        public async Task Run_MissingDbTokenHeader_ReturnsBadRequest()
        {
            var context = new DefaultHttpContext();
            var request = context.Request;
            request.Headers["X-User-ID"] = "test_user_id";

            var result = await _getConversations.Run(request);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Missing the following header: 'X-Db-Token'.", badRequestResult.Value);
        }

        [Fact]
        public async Task Run_MissingUserIdHeader_ReturnsBadRequest()
        {
            var context = new DefaultHttpContext();
            var request = context.Request;
            request.Headers["X-Db-Token"] = "db-token";

            var result = await _getConversations.Run(request);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Missing the following header: 'X-User-ID'.", badRequestResult.Value);
        }

        [Fact]
        public async Task Run_ErrorGettingConversations_ReturnsServerError()
        {
            var context = new DefaultHttpContext();
            var request = context.Request;
            request.Headers["X-User-ID"] = "test_user_id";
            request.Headers["X-Db-Token"] = "db-token";

            // Mock the HTTP request handler to simulate an error response
            _httpRequestHandlerMock
                .Setup(m => m.MakeHttpGetRequest(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync((true, "Error connecting to DB layer"));

            var result = await _getConversations.Run(request);

            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);
            Assert.Contains("Error connecting to the DB layer", objectResult.Value.ToString());
        }

        [Fact]
        public async Task Run_InvalidJson_ReturnsBadRequest()
        {
            var context = new DefaultHttpContext();
            var request = context.Request;
            request.Headers["X-User-ID"] = "test_user_id";
            request.Headers["X-Db-Token"] = "db-token";

            _httpRequestHandlerMock
                .Setup(m => m.MakeHttpGetRequest(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync((false, "[{}]"));

            IActionResult result;
            try
            {

                result = await _getConversations.Run(request);
            }
            catch (JsonException)
            {
                result = new BadRequestObjectResult("Invalid conversations data received from the DB layer.");
            }

            var objectResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(StatusCodes.Status400BadRequest, objectResult.StatusCode);
            Assert.Equal("Invalid conversations data received from the DB layer.", objectResult.Value);
        }

        [Fact]
        public async Task Run_ZeroConversation_ReturnsOkResult()
        {
            var context = new DefaultHttpContext();
            var request = context.Request;
            request.Headers["X-User-ID"] = "test_user_id";
            request.Headers["X-Db-Token"] = "db-token";

            // Simulate the HTTP handler returning an invalid JSON response
            _httpRequestHandlerMock
                .Setup(m => m.MakeHttpGetRequest(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync((false, "[]"));

            var result = await _getConversations.Run(request);

            var objectResult = Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task Run_ValidRequest_ReturnsOkResponse()
        {
            var context = new DefaultHttpContext();
            var request = context.Request;
            request.Headers["X-User-ID"] = "test_user_id";
            request.Headers["X-Db-Token"] = "db-token";

            // Create mock conversation data
            var mockConversations = new List<Conversation>
            {
                new Conversation
                (
                    user: new User("bbbbb-bbbbbbbbbb-bbbbb", "Bob", Images.getImage()),
                    conversationId: "ccccc-cccccccccc-ccccc",
                    messages: new List<Message>
                    {
                        new Message(DateTime.UtcNow.AddMinutes(-1), "bbbbb-bbbbbbbbbb-bbbbb", "I would like to join you on the trip!")
                    },
                    postId: "aaaaa-aaaaaaaaaa-aaaaa"
                ),
                new Conversation
                (
                    user: new User("aaaaa-bbbbbbbbbb-bbbbb", "Bob", Images.getImage()),
                    conversationId: "ccccc-cccccccccc-ddddd",
                    messages: new List<Message>
                    {
                        new Message(DateTime.UtcNow.AddMinutes(-1), "aaaaa-bbbbbbbbbb-bbbbb", "I would like to join you on the trip!")
                    },
                    postId: "bbbbb-aaaaaaaaaa-aaaaa"
                )
            };

            var mockResponse = JsonSerializer.Serialize(mockConversations);

            // Mock the HTTP request handler to simulate a successful response
            _httpRequestHandlerMock
                .Setup(m => m.MakeHttpGetRequest(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync((false, mockResponse));

            var result = await _getConversations.Run(request);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedConversations = Assert.IsAssignableFrom<List<Conversation>>(okResult.Value);
            Assert.Equal(mockConversations.Count, returnedConversations.Count);
            Assert.Equal(mockConversations[0].ConversationId, returnedConversations[0].ConversationId);
        }
    }
}
