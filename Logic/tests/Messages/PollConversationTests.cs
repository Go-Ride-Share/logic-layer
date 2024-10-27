using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace GoRideShare.Tests
{
    public class PollConversationTests
    {
        private readonly Mock<ILogger<PollConversation>> _loggerMock;
        private readonly Mock<IHttpRequestHandler> _httpRequestHandlerMock;
        private readonly PollConversation _pollConversation;

        public PollConversationTests()
        {
            _loggerMock = new Mock<ILogger<PollConversation>>();
            _httpRequestHandlerMock = new Mock<IHttpRequestHandler>();
            _pollConversation = new PollConversation(_loggerMock.Object, _httpRequestHandlerMock.Object);
        }

        [Fact]
        public async Task Run_MissingDbTokenHeader_ReturnsBadRequest()
        {
            var context = new DefaultHttpContext();
            var request = context.Request;
            request.Headers["X-User-ID"] = "test_user_id";

            var result = await _pollConversation.Run(request);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Missing the following header: 'X-Db-Token'.", badRequestResult.Value);
        }

        [Fact]
        public async Task Run_MissingUserIdHeader_ReturnsBadRequest()
        {
            var context = new DefaultHttpContext();
            var request = context.Request;
            request.Headers["X-Db-Token"] = "db-token";

            var result = await _pollConversation.Run(request);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Missing the following header: 'X-User-ID'.", badRequestResult.Value);
        }

        [Fact]
        public async Task Run_MissingConversationIdQueryParam_ReturnsBadRequest()
        {
            var context = new DefaultHttpContext();
            var request = context.Request;
            request.Headers["X-User-ID"] = "test_user_id";
            request.Headers["X-Db-Token"] = "db-token";

            var result = await _pollConversation.Run(request);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Missing the following query param: 'conversationId'", badRequestResult.Value);
        }

        // [Fact]
        // public async Task Run_ErrorGettingConversation_ReturnsServerError()
        // {
        //     var context = new DefaultHttpContext();
        //     var request = context.Request;
        //     request.Headers["X-User-ID"] = "test_user_id";
        //     request.Headers["X-Db-Token"] = "db-token";

        //     // Add conversationId query parameter
        //     request.QueryString = new QueryString("?conversationId=test_conversation_id");

        //     // Mock the HTTP request handler to simulate an error response
        //     _httpRequestHandlerMock
        //         .Setup(m => m.MakeHttpGetRequest(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
        //         .ReturnsAsync((true, "Error connecting to DB layer"));

        //     var result = await _pollConversation.Run(request);

        //     var objectResult = Assert.IsType<ObjectResult>(result);
        //     Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);
        //     Assert.Contains("Error connecting to the DB layer", objectResult.Value.ToString());
        // }

        // [Fact]
        // public async Task Run_InvalidJson_ReturnsBadRequest()
        // {
        //     var context = new DefaultHttpContext();
        //     var request = context.Request;
        //     request.Headers["X-User-ID"] = "test_user_id";
        //     request.Headers["X-Db-Token"] = "db-token";
        //     request.QueryString = new QueryString("?conversationId=test_conversation_id");

        //     // Simulate the HTTP handler returning an invalid JSON response
        //     _httpRequestHandlerMock
        //         .Setup(m => m.MakeHttpGetRequest(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
        //         .ReturnsAsync((false, "invalid json"));

        //     var result = await _pollConversation.Run(request);

        //     var objectResult = Assert.IsType<ObjectResult>(result);
        //     Assert.Equal(StatusCodes.Status400BadRequest, objectResult.StatusCode);
        //     Assert.Equal("Invalid conversation data received from the DB layer.", objectResult.Value);
        // }

        // [Fact]
        // public async Task Run_ValidRequest_ReturnsOkResponse()
        // {
        //     var context = new DefaultHttpContext();
        //     var request = context.Request;
        //     request.Headers["X-User-ID"] = "test_user_id";
        //     request.Headers["X-Db-Token"] = "db-token";
        //     request.QueryString = new QueryString("?conversationId=test_conversation_id");

        //     // Create mock conversation data
        //     var mockConversation = new Conversation
        //     (
        //         user: new User("bbbbb-bbbbbbbbbb-bbbbb", "Bob", Images.getImage()),
        //         conversationId: "ccccc-cccccccccc-ccccc",
        //         messages: new List<Message>
        //         {
        //             new Message(DateTime.UtcNow.AddMinutes(-1), "bbbbb-bbbbbbbbbb-bbbbb", "Hello"),
        //             new Message(DateTime.UtcNow, "bbbbb-bbbbbbbbbb-bbbbb", "I would like to join you on the trip!")
        //         },
        //         postId: "aaaaa-aaaaaaaaaa-aaaaa"
        //     );

        //     var mockResponse = JsonSerializer.Serialize(mockConversation);

        //     // Mock the HTTP request handler to simulate a successful response
        //     _httpRequestHandlerMock
        //         .Setup(m => m.MakeHttpGetRequest(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
        //         .ReturnsAsync((false, mockResponse));

        //     var result = await _pollConversation.Run(request);

        //     var okResult = Assert.IsType<OkObjectResult>(result);
        //     var returnedConversation = Assert.IsAssignableFrom<Conversation>(okResult.Value);
        //     Assert.Equal(mockConversation.ConversationId, returnedConversation.ConversationId);
        // }
    }
}
