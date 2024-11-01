using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System.Text.Json;
using Xunit;

namespace GoRideShare.Tests
{
    public class GetAllPostsTests
    {
        private readonly Mock<ILogger<GetAllPosts>> _loggerMock;
        private readonly Mock<IHttpRequestHandler> _httpRequestHandlerMock;
        private readonly GetAllPosts _getAllPosts;

        public GetAllPostsTests()
        {
            _loggerMock = new Mock<ILogger<GetAllPosts>>();
            _httpRequestHandlerMock = new Mock<IHttpRequestHandler>();
            _getAllPosts = new GetAllPosts(_loggerMock.Object, _httpRequestHandlerMock.Object);
        }

        [Fact]
        public async Task Run_ValidRequest_NoPostsFound_ReturnsErrorResponse()
        {
            var context = new DefaultHttpContext();
            var request = context.Request;

            _httpRequestHandlerMock
                .Setup(m => m.MakeHttpGetRequest(It.IsAny<string>()))
                .ReturnsAsync((false, "[]"));

            var result = await _getAllPosts.Run(request);

            var objectResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("[]", objectResult.Value);
        }

        [Fact]
        public async Task Run_ValidRequest_PostsFound_ReturnsOkResponse()
        {
            var context = new DefaultHttpContext();
            var request = context.Request;

            var mockPosts = new List<PostDetails>
            {
                new PostDetails
                {
                    PostId = "test_post_id",
                    PosterId = "test_poster_id",
                    Name = "test post 1",
                    Description = "test_desc",
                    DepartureDate = "2024-10-10",
                    OriginLat = 12.34f,
                    OriginLng = 56.78f,
                    DestinationLat = 90.12f,
                    DestinationLng = 34.56f,
                    Price = 15.0f,
                    SeatsAvailable = 2
                },
                new PostDetails
                {
                    PostId = "test_post_id",
                    PosterId = "test_poster_id",
                    Name = "test Post 2",
                    Description = "test_desc",
                    DepartureDate = "2024-11-11",
                    OriginLat = 21.43f,
                    OriginLng = 65.87f,
                    DestinationLat = 80.21f,
                    DestinationLng = 43.65f,
                    Price = 35.0f,
                    SeatsAvailable = 2
                }
            };

            _httpRequestHandlerMock
                .Setup(m => m.MakeHttpGetRequest(It.IsAny<string>()))
                .ReturnsAsync((false, JsonSerializer.Serialize(mockPosts)));

            var result = await _getAllPosts.Run(request);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedPosts = Assert.IsAssignableFrom<List<PostDetails>>(okResult.Value);
            Assert.Equal(mockPosts.Count, returnedPosts.Count);
        }

        [Fact]
        public async Task Run_ValidRequest_DbConnectionError_ReturnsErrorResponse()
        {
            var context = new DefaultHttpContext();
            var request = context.Request;

            _httpRequestHandlerMock
                .Setup(m => m.MakeHttpGetRequest(It.IsAny<string>()))
                .ReturnsAsync((true, "[]"));

            var result = await _getAllPosts.Run(request);

            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);
            Assert.Equal("Error connecting to the DB layer.", objectResult.Value);
        }

        [Fact]
        public async Task Run_ValidRequest_InternalServerError_ReturnsErrorResponse()
        {
            var context = new DefaultHttpContext();
            var request = context.Request;

            _httpRequestHandlerMock
                .Setup(m => m.MakeHttpGetRequest(It.IsAny<string>()))
                .ReturnsAsync((true, "Internal Server Error")); // Simulating a 500 error

            var result = await _getAllPosts.Run(request);

            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);
            Assert.Equal("Error connecting to the DB layer.", objectResult.Value);
        }

    }
}
