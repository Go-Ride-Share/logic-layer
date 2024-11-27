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
                    PostId = Guid.Parse("09c453af-8065-42b7-9836-947eace8d6aa"),
                    PosterId = Guid.Parse("2447c169-3476-4aee-872f-a64469a8138e"),
                    Name = "test post 1",
                    Description = "test_desc",
                    OriginName = null,
                    OriginLat = 12.34f,
                    OriginLng = 56.78f,
                    DestinationName = null,
                    DestinationLat = 90.12f,
                    DestinationLng = 34.56f,
                    Price = 15.0f,
                    SeatsAvailable = 2,
                    SeatsTaken = null,
                    DepartureDate = "2024-10-10",
                    CreatedAt = DateTime.Parse("2024-10-10"),
                    Poster = new User
                    {
                        UserId = Guid.Parse("2447c169-3476-4aee-872f-a64469a8138e"),
                        Name = "test user",
                        Photo = null
                    }
                },
                new PostDetails
                {
                    PostId = Guid.Parse("09c453af-8065-42b7-9836-947eace8d6aa"),
                    PosterId = Guid.Parse("2447c169-3476-4aee-872f-a64469a8138e"), 
                    Name = "test Post 2",
                    Description = "test_desc",
                    OriginName = null,
                    OriginLat = 21.43f,
                    OriginLng = 65.87f,
                    DestinationName = null,
                    DestinationLat = 80.21f,
                    DestinationLng = 43.65f,
                    Price = 35.0f,
                    SeatsAvailable = 2,
                    SeatsTaken = null,
                    DepartureDate = "2024-11-11",
                    CreatedAt = DateTime.Parse("2024-10-10"),
                    Poster = new User
                    {
                        UserId = Guid.Parse("2447c169-3476-4aee-872f-a64469a8138e"), 
                        Name = "test user",  
                        Photo = null
                    }
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
