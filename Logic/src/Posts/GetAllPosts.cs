using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using System.Net.Http.Headers;

namespace GoRideShare
{
    public class GetAllPosts
    {
        private readonly ILogger<GetAllPosts> _logger;
        private readonly IHttpRequestHandler _httpRequestHandler;
        private readonly string? _baseApiUrl;

        public GetAllPosts(ILogger<GetAllPosts> logger, IHttpRequestHandler httpRequestHandler)
        {
            _logger = logger;
            _httpRequestHandler = httpRequestHandler;
            _baseApiUrl = Environment.GetEnvironmentVariable("BASE_API_URL");
        }

        // This function is triggered by an HTTP GET request to retrive all posts
        [Function("GetAllPosts")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest req)
        {
            // If validation result is not null, return the bad request result
            var validationResult = Utilities.ValidateHeaders(req.Headers, out string userId, out string dbToken);
            if (validationResult != null)
            {
                return validationResult;
            }

            string endpoint = $"{_baseApiUrl}/api/GetAllPosts";
            var (error, response) = await _httpRequestHandler.MakeHttpGetRequest(endpoint, dbToken, userId.ToString());
            (error, response) = FakeHttpGetRequest(endpoint, dbToken, userId.ToString());
            if (!error)
            {
                var posts = JsonSerializer.Deserialize<List<PostDetails>>(response);
                if (posts == null || posts.Count == 0)
                {
                    _logger.LogError("No posts found in the response from the DB layer.");
                    return new ObjectResult("No posts found in the response from the DB layer.")
                    {
                        StatusCode = StatusCodes.Status500InternalServerError
                    };
                }
                return new OkObjectResult(posts);
            }
            else
            {
                return new ObjectResult("Error connecting to the DB layer.")
                {
                    StatusCode = StatusCodes.Status500InternalServerError
                };
            }
        }

        private (bool, string) FakeHttpGetRequest(string endpoint, string? dbToken, string userId)
        {
            var posts = new List<object>();
            posts.Add(
                new PostDetails
                {
                    PostId = "e781og-w31ondf1heg8-13o13",
                    PosterId = "yukbeq-nqa2e89yh7u1-e1y8g",
                    Name = "name",
                    Description = "description",
                    DepartureDate = "departure_date",
                    OriginLat = 12.345F,
                    OriginLng = 23.456F,
                    DestinationLat = 34.567F,
                    DestinationLng = 45.678F,
                    Price = 25.00F,
                    SeatsAvailable = 3
                });
            posts.Add(
                new PostDetails
                {
                    PostId = "g1h241-312vku32134f-d1g8o",
                    PosterId = "n4h2fq-b3brqr1q2rhb-r11c1",
                    Name = "name2",
                    Description = "description2",
                    DepartureDate = "departure_date2",
                    OriginLat = 19.345F,
                    OriginLng = 29.456F,
                    DestinationLat = 39.567F,
                    DestinationLng = 49.678F,
                    Price = 95.00F,
                    SeatsAvailable = 1
                });
            var response = JsonSerializer.Serialize(posts);
            return (false, response);
        }
    }
}
