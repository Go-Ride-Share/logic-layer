using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System.Text.Json;
using Xunit;

namespace GoRideShare.Tests
{
    public class PostMessageTests
    {
        private readonly Mock<ILogger<PostMessage>> _loggerMock;
        private readonly Mock<IHttpRequestHandler> _httpRequestHandlerMock;
        private readonly PostMessage _postMessage;

        public PostMessageTests()
        {
            _loggerMock = new Mock<ILogger<PostMessage>>();
            _httpRequestHandlerMock = new Mock<IHttpRequestHandler>();
            _postMessage = new PostMessage(_loggerMock.Object, _httpRequestHandlerMock.Object);
        }

        [Fact]
        public async Task Run_MissingDbTokenHeader_ReturnsBadRequest()
        {
            var context = new DefaultHttpContext();
            var request = context.Request;
            request.Headers["X-User-ID"] = "test_user_id";

            var result = await _postMessage.Run(request);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Missing the following header: 'X-Db-Token'.", badRequestResult.Value);
        }

        [Fact]
        public async Task Run_MissingUserIdHeader_ReturnsBadRequest()
        {
            var context = new DefaultHttpContext();
            var request = context.Request;
            request.Headers["X-Db-Token"] = "db-token";

            var result = await _postMessage.Run(request);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Missing the following header: 'X-User-ID'.", badRequestResult.Value);
        }

        [Fact]
        public async Task Run_InvalidJson_ReturnsBadRequest()
        {
            var context = new DefaultHttpContext();
            var request = context.Request;
            request.Headers["X-User-ID"] = "test_user_id";
            request.Headers["X-Db-Token"] = "db-token";

            string invalidJson = "invalid json";
            context.Request.Body = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(invalidJson));

            var result = await _postMessage.Run(request);

            var objectResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Incomplete Message data.", objectResult.Value);
        }

        [Fact]
        public async Task Run_ValidationFailedContentsMissing_ReturnsBadRequest()
        {
            var context = new DefaultHttpContext();
            var request = context.Request;
            request.Headers["X-User-ID"] = "test_user_id";
            request.Headers["X-Db-Token"] = "db-token";

            var newMessage = new IncomingMessageRequest("convo_id", "");
            string requestBody = JsonSerializer.Serialize(newMessage);
            context.Request.Body = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(requestBody));

            var result = await _postMessage.Run(request);

            var objectResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Contains("contents cannot be empty", objectResult.Value.ToString());
        }

        [Fact]
        public async Task Run_ValidationFailedConversationIdMissing_ReturnsBadRequest()
        {
            var context = new DefaultHttpContext();
            var request = context.Request;
            request.Headers["X-User-ID"] = "test_user_id";
            request.Headers["X-Db-Token"] = "db-token";

            var newMessage = new IncomingMessageRequest("", "Test_content");
            string requestBody = JsonSerializer.Serialize(newMessage);
            context.Request.Body = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(requestBody));

            var result = await _postMessage.Run(request);

            var objectResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Contains("conversationId is invalid", objectResult.Value.ToString());
        }

        [Fact]
        public async Task Run_ValidRequest_ReturnsOkResponse()
        {
            var context = new DefaultHttpContext();
            var request = context.Request;
            request.Headers["X-User-ID"] = "test_user_id";
            request.Headers["X-Db-Token"] = "db-token";

            var newMessage = new IncomingMessageRequest("Hello", "test_conversation_id");
            string requestBody = JsonSerializer.Serialize(newMessage);
            context.Request.Body = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(requestBody));

            // Mock the HTTP request handler to simulate a successful response
            var mockResponse = new DbLayerResponse { Id = "31hft1-fukeqe2yeu1y8-sga1e2" };
            _httpRequestHandlerMock
                .Setup(m => m.MakeHttpPostRequest(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync((false, JsonSerializer.Serialize(mockResponse)));

            var result = await _postMessage.Run(request);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedResponse = Assert.IsAssignableFrom<DbLayerResponse>(okResult.Value);
            Assert.Equal(mockResponse.Id, returnedResponse.Id);
        }

        // [Fact]
        // public async Task Run_ErrorConnectingToDBLayer_ReturnsServerError()
        // {
        //     var context = new DefaultHttpContext();
        //     var request = context.Request;
        //     request.Headers["X-User-ID"] = "test_user_id";
        //     request.Headers["X-Db-Token"] = "db-token";

        //     var newMessage = new IncomingMessageRequest("Hello", "test_conversation_id");
        //     string requestBody = JsonSerializer.Serialize(newMessage);
        //     context.Request.Body = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(requestBody));

        //     // Mock the HTTP request handler to simulate an error response
        //     _httpRequestHandlerMock
        //         .Setup(m => m.MakeHttpPostRequest(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
        //         .ReturnsAsync((true, "Error connecting to DB layer"));

        //     var result = await _postMessage.Run(request);

        //     var objectResult = Assert.IsType<ObjectResult>(result);
        //     Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);
        //     Assert.Contains("Message Failed: Error connecting to DB layer", objectResult.Value.ToString());
        // }
    }
}
