using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System.Text.Json;
using Xunit;

namespace GoRideShare.Tests
{
    public class GetUserTests
    {
        private readonly Mock<ILogger<GetUser>> _loggerMock;
        private readonly Mock<IHttpRequestHandler> _httpRequestHandlerMock;
        private readonly GetUser _getUser;

        public GetUserTests()
        {
            _loggerMock = new Mock<ILogger<GetUser>>();
            _httpRequestHandlerMock = new Mock<IHttpRequestHandler>();
            _getUser = new GetUser(_loggerMock.Object, _httpRequestHandlerMock.Object);
        }

        [Fact]
        public async Task Run_MissingUserIdHeader_ReturnsBadRequest()
        {
            var context = new DefaultHttpContext();
            var request = context.Request;
            request.Headers["X-Db-Token"] = "db-token";

            var result = await _getUser.Run(request);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Missing the following header: 'X-User-ID'.", badRequestResult.Value);
        }

        [Fact]
        public async Task Run_Missingdb_tokenHeader_ReturnsBadRequest()
        {
            var context = new DefaultHttpContext();
            var request = context.Request;
            request.Headers["X-User-ID"] = "test_user_id";

            var result = await _getUser.Run(request);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Missing the following header: 'X-Db-Token'.", badRequestResult.Value);
        }

        [Fact]
        public async Task Run_ValidRequest_UserFound_ReturnsOkResponse()
        {
            var context = new DefaultHttpContext();
            var request = context.Request;
            request.Headers["X-User-ID"] = "test_user_id";
            request.Headers["X-Db-Token"] = "db-token";

            var mockUser = new
            {
                Name = "Name Name",
                Email = "test@email.com",
                Bio = "Test Bio",
                Phone = "4312225555",
                Photo = "photo_encoding"
            };
            var jsonResponse = JsonSerializer.Serialize(mockUser);

            _httpRequestHandlerMock
                .Setup(m => m.MakeHttpGetRequest(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync((false, JsonSerializer.Serialize(mockUser)));

            var result = await _getUser.Run(request);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedUser = Assert.IsType<string>(okResult.Value);
            Assert.Equal(jsonResponse, returnedUser);
        }

        [Fact]
        public async Task Run_ValidRequest_UserNotFound_ReturnsNotFound()
        {
            var context = new DefaultHttpContext();
            var request = context.Request;
            request.Headers["X-User-ID"] = "test_user_id";
            request.Headers["X-Db-Token"] = "db-token";

            _httpRequestHandlerMock
                .Setup(m => m.MakeHttpGetRequest(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync((true, "404: Not Found"));

            var result = await _getUser.Run(request);

            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            var response = Assert.IsType<string>(notFoundResult.Value);
            Assert.Equal("User not found in the database!", response);
        }

        [Fact]
        public async Task Run_ValidRequest_DbLayer_ReturnsBadRequest()
        {
            var context = new DefaultHttpContext();
            var request = context.Request;
            request.Headers["X-User-ID"] = "test_user_id";
            request.Headers["X-Db-Token"] = "db-token";

            _httpRequestHandlerMock
                .Setup(m => m.MakeHttpGetRequest(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync((true, "400: Bad Request"));

            var result = await _getUser.Run(request);

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task Run_ValidRequest_DbConnectionError_ReturnsErrorResponse()
        {
            var context = new DefaultHttpContext();
            var request = context.Request;
            request.Headers["X-User-ID"] = "test_user_id";
            request.Headers["X-Db-Token"] = "db-token";

            _httpRequestHandlerMock
                .Setup(m => m.MakeHttpGetRequest(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync((true, ""));

            var result = await _getUser.Run(request);

            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);
            Assert.Equal("Error connecting to the DB layer.", objectResult.Value);
        }
    }
}
