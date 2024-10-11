using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System.Text;
using System.Text.Json;
using Xunit;

namespace GoRideShare.Tests
{
    public class SavePostTests
    {
        private readonly Mock<ILogger<SavePost>> _loggerMock;
        private readonly Mock<IHttpRequestHandler> _httpRequestHandlerMock;
        private readonly SavePost _savePost;

        public SavePostTests()
        {
            _loggerMock = new Mock<ILogger<SavePost>>();
            _httpRequestHandlerMock = new Mock<IHttpRequestHandler>();
            _savePost = new SavePost(_loggerMock.Object, _httpRequestHandlerMock.Object);
        }

        [Fact]
        public async Task Run_MissingUserIdHeader_ReturnsBadRequest()
        {
            var context = new DefaultHttpContext();
            var request = context.Request;
            request.Headers["X-Db-Token"] = "db-token";

            var result = await _savePost.Run(request);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Missing the following header: 'X-User-ID'.", badRequestResult.Value);
        }

        [Fact]
        public async Task Run_MissingDbTokenHeader_ReturnsBadRequest()
        {
            var context = new DefaultHttpContext();
            var request = context.Request;
            request.Headers["X-User-ID"] = "test_user_id";

            var result = await _savePost.Run(request);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Missing the following header 'X-Db-Token'.", badRequestResult.Value);
        }

        [Fact]
        public async Task Run_IncompletePostData_ReturnsBadRequest()
        {
            var context = new DefaultHttpContext();
            var request = context.Request;
            request.Headers["X-User-ID"] = "test_user_id";
            request.Headers["X-Db-Token"] = "db-token";
            var invalidPost = "[]";
            var requestBody = JsonSerializer.Serialize(invalidPost);
            request.Body = new MemoryStream(Encoding.UTF8.GetBytes(requestBody));

            var result = await _savePost.Run(request);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Incomplete post data.", badRequestResult.Value);
        }

        [Fact]
        public async Task Run_InvalidPostData_ReturnsBadRequest()
        {
            var context = new DefaultHttpContext();
            var request = context.Request;
            request.Headers["X-User-ID"] = "test_user_id";
            request.Headers["X-Db-Token"] = "db-token";
            var invalidPost = new PostDetails{
                PostId = "test_post_id",
                Name ="test post 1",
                Description = "test_desc",
                DepartureDate = "2024-10-10T00:00:00.000",
                OriginLat = 150,
                OriginLng = 200,
                DestinationLat = 120,
                DestinationLng = -200,
                Price = 12,
                SeatsAvailable = 1
            };

            var requestBody = JsonSerializer.Serialize(invalidPost);
            request.Body = new MemoryStream(Encoding.UTF8.GetBytes(requestBody));

            var result = await _savePost.Run(request);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Invalid post data.", badRequestResult.Value);
        }

        [Fact]
        public async Task Run_ValidPostData_ReturnsOkResult()
        {
            var context = new DefaultHttpContext();
            var request = context.Request;
            request.Headers["X-User-ID"] = "test_user_id";
            request.Headers["X-Db-Token"] = "db-token";

            var validPost = new PostDetails{
                PostId = "",
                Name ="test post 1",
                Description = "test_desc",
                DepartureDate = "2024-10-10",
                OriginLat = 12.0f,
                OriginLng = 12.0f,
                DestinationLat = 12.0f,
                DestinationLng = -12.0f,
                Price = 15.0f,
                SeatsAvailable = 1
            };

            var requestBody = JsonSerializer.Serialize(validPost);
            request.Body = new MemoryStream(Encoding.UTF8.GetBytes(requestBody));

            var mockResponse = new DbLayerResponse{ PostId = "test_post_id" };
            var jsonResponse = JsonSerializer.Serialize(mockResponse);

            _httpRequestHandlerMock
                .Setup(m => m.MakeHttpPostRequest(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync((false, JsonSerializer.Serialize(mockResponse)));

            var result = await _savePost.Run(request);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<string>(okResult.Value);
            Assert.Equal(jsonResponse, response);
        }

        [Fact]
        public async Task Run_ValidPostData_InvalidPostID_ReturnsErrorResponse()
        {
            var context = new DefaultHttpContext();
            var request = context.Request;
            request.Headers["X-User-ID"] = "test_user_id";
            request.Headers["X-Db-Token"] = "db-token";

            var validPost = new PostDetails{
                PostId = "",
                Name ="test post 1",
                Description = "test_desc",
                DepartureDate = "2024-10-10",
                OriginLat = 12.0f,
                OriginLng = 12.0f,
                DestinationLat = 12.0f,
                DestinationLng = -12.0f,
                Price = 15.0f,
                SeatsAvailable = 1
            };

            var requestBody = JsonSerializer.Serialize(validPost);
            request.Body = new MemoryStream(Encoding.UTF8.GetBytes(requestBody));

            var mockResponse = new DbLayerResponse{ PostId = "" };
            var jsonResponse = JsonSerializer.Serialize(mockResponse);

            _httpRequestHandlerMock
                .Setup(m => m.MakeHttpPostRequest(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync((false, JsonSerializer.Serialize(mockResponse)));

            var result = await _savePost.Run(request);

            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);
            Assert.Equal("Post ID not found in the response from the DB layer.", objectResult.Value);
        }

        [Fact]
        public async Task Run_ValidPostData_DbConnectionError_ReturnsErrorResponse()
        {
            var context = new DefaultHttpContext();
            var request = context.Request;
            request.Headers["X-User-ID"] = "test_user_id";
            request.Headers["X-Db-Token"] = "db-token";

            var validPost = new PostDetails{
                PostId = "",
                Name ="test post 1",
                Description = "test_desc",
                DepartureDate = "2024-10-10",
                OriginLat = 12.0f,
                OriginLng = 12.0f,
                DestinationLat = 12.0f,
                DestinationLng = -12.0f,
                Price = 15.0f,
                SeatsAvailable = 1
            };

            var requestBody = JsonSerializer.Serialize(validPost);
            request.Body = new MemoryStream(Encoding.UTF8.GetBytes(requestBody));

            _httpRequestHandlerMock
                .Setup(m => m.MakeHttpPostRequest(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync((true, "Error inserting post into the database"));

            var result = await _savePost.Run(request);

            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);
            Assert.Equal("Error connecting to the DB layer: Error inserting post into the database", objectResult.Value);
        }
    }
}
