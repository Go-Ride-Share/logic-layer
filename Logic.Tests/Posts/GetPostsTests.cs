using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System.Text.Json;
using Xunit;

namespace GoRideShare.Tests
{
    public class GetPostsTests
    {
        private readonly Mock<ILogger<GetPosts>> _loggerMock;
        private readonly Mock<IHttpRequestHandler> _httpRequestHandlerMock;
        private readonly Mock<IAzureTableService> _azureTableService;
        private readonly Utilities _utilities;
        private readonly GetPosts _getPosts;

        public GetPostsTests()
        {
            _loggerMock = new Mock<ILogger<GetPosts>>();
            _httpRequestHandlerMock = new Mock<IHttpRequestHandler>();
            _azureTableService = new Mock<IAzureTableService>();
            _utilities = new Utilities(_azureTableService.Object);
            _getPosts = new GetPosts(_loggerMock.Object, _httpRequestHandlerMock.Object, _utilities);
        }

        [Fact]
        public async Task Run_MissingUserIdHeader_ReturnsBadRequest()
        {
            var context = new DefaultHttpContext();
            var request = context.Request;
            request.Headers["X-Db-Token"] = "db-token";

            var result = await _getPosts.Run(request, "0523e365-2499-46ad-b71f-c12e5128f2ee");

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Missing the following header: 'X-User-ID'.", badRequestResult.Value);
        }

        [Fact]
        public async Task Run_Missingdb_tokenHeader_ReturnsBadRequest()
        {
            var context = new DefaultHttpContext();
            var request = context.Request;
            request.Headers["X-User-ID"] = "test_user_id";

            var result = await _getPosts.Run(request, "0523e365-2499-46ad-b71f-c12e5128f2ee");

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Missing the following header: 'X-Db-Token'.", badRequestResult.Value);
        }

        [Fact]
        public async Task Run_MissingUserIdQueryParam_ReturnsBadRequest()
        {
            var context = new DefaultHttpContext();
            var request = context.Request;
            request.Headers["X-User-ID"] = "test_user_id";
            request.Headers["X-Db-Token"] = "db-token";

            var result = await _getPosts.Run(request, "");

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Invalid Path Parameter: `user_id` not passed", badRequestResult.Value);
        }

        [Fact]
        public async Task Run_ValidRequest_NoPostsFound_ReturnsEmptyResponse()
        {
            var context = new DefaultHttpContext();
            var request = context.Request;
            request.Headers["X-User-ID"] = "09c453af-8065-42b7-9836-947eace8d6aa";
            request.Headers["X-Db-Token"] = "db-token";
            context.Request.RouteValues["user_id"] = "test-user-id";

            _httpRequestHandlerMock
                .Setup(m => m.MakeHttpGetRequest(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync((false, "[]"));

            var result = await _getPosts.Run(request, "0523e365-2499-46ad-b71f-c12e5128f2ee");

            var objectResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("[]", objectResult.Value);
        }

        [Fact]
        public async Task Run_ValidRequest_PostsFound_ReturnsOkResponse()
        {
            var context = new DefaultHttpContext();
            var request = context.Request;
            request.Headers["X-User-ID"] = "09c453af-8065-42b7-9836-947eace8d6aa";
            request.Headers["X-Db-Token"] = "token";
            request.QueryString = new QueryString("?userId=<test-user-id>");

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
                    SeatsAvailable = 2,
                    CreatedAt = DateTime.Parse("2024-10-10")
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
                    SeatsAvailable = 2,
                    CreatedAt = DateTime.Parse("2024-10-10")
                }
            };

            _httpRequestHandlerMock
                .Setup(m => m.MakeHttpGetRequest(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync((false, JsonSerializer.Serialize(mockPosts)));

            var result = await _getPosts.Run(request, "0523e365-2499-46ad-b71f-c12e5128f2ee");

            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedPosts = Assert.IsAssignableFrom<List<PostDetails>>(okResult.Value);
            Assert.Equal(mockPosts.Count, returnedPosts.Count);
        }

        [Fact]
        public async Task Run_ValidRequest_DbConnectionError_ReturnsErrorResponse()
        {
            var context = new DefaultHttpContext();
            var request = context.Request;
            request.Headers["X-User-ID"] = "09c453af-8065-42b7-9836-947eace8d6aa";
            request.Headers["X-Db-Token"] = "db-token";
            context.Request.RouteValues["user_id"] = "<test-user-id>";

            _httpRequestHandlerMock
                .Setup(m => m.MakeHttpGetRequest(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync((true, "[]"));

            var result = await _getPosts.Run(request, "0523e365-2499-46ad-b71f-c12e5128f2ee");

            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);
            Assert.Equal("Error connecting to the DB layer.", objectResult.Value);
        }
    }
}
