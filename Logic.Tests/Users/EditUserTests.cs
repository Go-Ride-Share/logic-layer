using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System.IO;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace GoRideShare.Tests
{
    public class EditUserTests
    {
        private readonly Mock<ILogger<EditUser>> _loggerMock;
        private readonly Mock<IHttpRequestHandler> _httpRequestHandlerMock;
        private readonly EditUser _editUser;

        public EditUserTests()
        {
            _loggerMock = new Mock<ILogger<EditUser>>();
            _httpRequestHandlerMock = new Mock<IHttpRequestHandler>();
            _editUser = new EditUser(_loggerMock.Object, _httpRequestHandlerMock.Object);
        }

        [Fact]
        public async Task Run_MissingDbTokenHeader_ReturnsBadRequest()
        {
            var context = new DefaultHttpContext();
            var request = context.Request;
            request.Headers["X-User-ID"] = "test_user_id";

            var result = await _editUser.Run(request);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Missing the following header: 'X-Db-Token'.", badRequestResult.Value);
        }

        [Fact]
        public async Task Run_MissingUserIdHeader_ReturnsBadRequest()
        {
            var context = new DefaultHttpContext();
            var request = context.Request;
            request.Headers["X-Db-Token"] = "db-token";

            var result = await _editUser.Run(request);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Missing the following header: 'X-User-ID'.", badRequestResult.Value);
        }

        [Fact]
        public async Task Run_ValidRequest_UserUpdatedSuccessfully_ReturnsOkResponse()
        {
            var context = new DefaultHttpContext();
            var request = context.Request;
            request.Headers["X-User-ID"] = "test_user_id";
            request.Headers["X-Db-Token"] = "db-token";

            var requestBody = JsonSerializer.Serialize(new { Name = "Updated Name", Email = "updated@example.com" });
            context.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(requestBody));

            // Mock the HTTP request handler to simulate a successful response
            _httpRequestHandlerMock
                .Setup(m => m.MakeHttpPostRequest(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync((false, JsonSerializer.Serialize(new { Message = "Updated Successfully" })));


            var result = await _editUser.Run(request);

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task Run_ValidRequest_UserNotFound_ReturnsNotFound()
        {
            var context = new DefaultHttpContext();
            var request = context.Request;
            request.Headers["X-User-ID"] = "test_user_id";
            request.Headers["X-Db-Token"] = "db-token";

            var requestBody = JsonSerializer.Serialize(new { Name = "Updated Name", Email = "updated@example.com" });
            context.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(requestBody));

            // Mock the HTTP request handler to simulate a not found response
            _httpRequestHandlerMock
                .Setup(m => m.MakeHttpPostRequest(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync((true, "404: Not Found"));

            var result = await _editUser.Run(request);

            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task Run_ValidRequest_ReturnsBadRequest()
        {
            var context = new DefaultHttpContext();
            var request = context.Request;
            request.Headers["X-User-ID"] = "test_user_id";
            request.Headers["X-Db-Token"] = "db-token";

            var requestBody = JsonSerializer.Serialize(new { Name = "Updated Name", Email = "updated@example.com" });
            context.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(requestBody));

            // Mock the HTTP request handler to simulate a not found response
            _httpRequestHandlerMock
                .Setup(m => m.MakeHttpPostRequest(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync((true, "400: Bad Request"));

            var result = await _editUser.Run(request);

            var notFoundResult = Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task Run_ErrorConnectingToDBLayer_ReturnsServerError()
        {
            var context = new DefaultHttpContext();
            var request = context.Request;
            request.Headers["X-User-ID"] = "test_user_id";
            request.Headers["X-Db-Token"] = "db-token";

            var requestBody = JsonSerializer.Serialize(new { Name = "Updated Name", Email = "updated@example.com" });
            context.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(requestBody));

            // Mock the HTTP request handler to simulate an error response
            _httpRequestHandlerMock
                .Setup(m => m.MakeHttpPostRequest(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync((true, "Error connecting to DB layer"));

            var result = await _editUser.Run(request);

            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);
            Assert.Equal("Error connecting to the DB layer.", objectResult.Value.ToString());
        }

    }
}
