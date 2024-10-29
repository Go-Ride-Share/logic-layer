using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System.Text.Json;
using Xunit;

namespace GoRideShare.Tests
{
    public class CreateConversationTests
    {
        private readonly Mock<ILogger<CreateConversation>> _loggerMock;
        private readonly Mock<IHttpRequestHandler> _httpRequestHandlerMock;
        private readonly CreateConversation _createConversation;

        public CreateConversationTests()
        {
            _loggerMock = new Mock<ILogger<CreateConversation>>();
            _httpRequestHandlerMock = new Mock<IHttpRequestHandler>();
            _createConversation = new CreateConversation(_loggerMock.Object, _httpRequestHandlerMock.Object);
        }

        [Fact]
        public async Task Run_Missingdb_tokenHeader_ReturnsBadRequest()
        {
            var context = new DefaultHttpContext();
            var request = context.Request;
            request.Headers["X-User-ID"] = "test_user_id";

            var result = await _createConversation.Run(request);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Missing the following header: 'X-Db-Token'.", badRequestResult.Value);
        }

        [Fact]
        public async Task Run_MissingUserIdHeader_ReturnsBadRequest()
        {
            var context = new DefaultHttpContext();
            var request = context.Request;
            request.Headers["X-Db-Token"] = "db-token";

            var result = await _createConversation.Run(request);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Missing the following header: 'X-User-ID'.", badRequestResult.Value);
        }

        [Fact]
        public async Task Run_InvalidJson_ReturnsBadRequest()
        {
            var context = new DefaultHttpContext();
            var request = context.Request;
            context.Request.Headers["X-User-ID"] = "test_user_id";
            request.Headers["X-Db-Token"] = "db-token";

            string invalidJson = "invalid json";
            context.Request.Body = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(invalidJson));

            var result = await _createConversation.Run(request);

            var objectResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Incomplete Conversation Request data.", objectResult.Value);
        }

        [Fact]
        public async Task Run_ValidationFailedMissingUserId_ReturnsBadRequest()
        {
            var context = new DefaultHttpContext();
            var request = context.Request;
            context.Request.Headers["X-User-ID"] = "test_user_id";
            request.Headers["X-Db-Token"] = "db-token";

            var newConversation = new IncomingConversationRequest("Hello", "");
            string requestBody = JsonSerializer.Serialize(newConversation);
            context.Request.Body = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(requestBody));

            var result = await _createConversation.Run(request);

            // Assert
            var objectResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Contains("userId is invalid", objectResult.Value.ToString()); // Adjust according to your validation logic
        }

        [Fact]
        public async Task Run_ValidationFailedMissingContents_ReturnsBadRequest()
        {
            var context = new DefaultHttpContext();
            var request = context.Request;
            context.Request.Headers["X-User-ID"] = "test_user_id";
            request.Headers["X-Db-Token"] = "db-token";

            var newConversation = new IncomingConversationRequest("", "test_user_id");
            string requestBody = JsonSerializer.Serialize(newConversation);
            context.Request.Body = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(requestBody));

            var result = await _createConversation.Run(request);

            // Assert
            var objectResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Contains("contents cannot be empty", objectResult.Value.ToString()); // Adjust according to your validation logic
        }

        [Fact]
        public async Task Run_ValidRequest_ReturnsOkResponse()
        {
            var context = new DefaultHttpContext();
            var request = context.Request;
            context.Request.Headers["X-User-ID"] = "test_user_id";
            request.Headers["X-Db-Token"] = "db-token";

            var messages = new List<Message>
            {
                new Message
                (
                    timeStamp: DateTime.UtcNow,
                    senderId: "userId",
                    contents: "Hello, this is a test message."
                )
            };

            var user = new User
            (
                userId: "userId",
                name: "Test User",
                photo: "test_profile_image.png"
            );

            var conversation = new Conversation
            (
                conversationId: "conversationId",
                user: user,
                messages: messages,
                postId: "test_post_id"
            );

            var mockConversation = JsonSerializer.Serialize(conversation);

            var newConversation = new IncomingConversationRequest("Hello, this is a test conversation.", "test_user_id");
            string requestBody = JsonSerializer.Serialize(newConversation);
            context.Request.Body = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(requestBody));

            // Mock the HTTP request handler to simulate a successful response
            _httpRequestHandlerMock
                .Setup(m => m.MakeHttpPostRequest(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync((false, JsonSerializer.Serialize(conversation)));

            var result = await _createConversation.Run(request);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedConversation = Assert.IsAssignableFrom<Conversation>(okResult.Value);
            Assert.Equal(mockConversation, JsonSerializer.Serialize(returnedConversation));
        }

        [Fact]
        public async Task Run_ErrorConnectingToDBLayer_ReturnsServerError()
        {
            var context = new DefaultHttpContext();
            var request = context.Request;
            context.Request.Headers["X-User-ID"] = "test_user_id";
            request.Headers["X-Db-Token"] = "db-token";

            var newConversation = new IncomingConversationRequest("Hello", "test_user_id");
            string requestBody = JsonSerializer.Serialize(newConversation);
            context.Request.Body = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(requestBody));

            // Mock the HTTP request handler to simulate an error response
            _httpRequestHandlerMock
                .Setup(m => m.MakeHttpPostRequest(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync((true, "Error connecting to DB layer"));

            // Act
            var result = await _createConversation.Run(request);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);
            Assert.Contains("Error connecting to the DB layer", objectResult.Value.ToString());
        }
    }
}
