using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using System.Net.Http.Headers;

namespace GoRideShare
{
    public class FindPost
    {
        private readonly ILogger<FindPost> _logger;
        private readonly IHttpRequestHandler _httpRequestHandler;
        private readonly string? _baseApiUrl;

        public FindPost(ILogger<FindPost> logger, IHttpRequestHandler httpRequestHandler)
        {
            _logger = logger;
            _httpRequestHandler = httpRequestHandler;
            _baseApiUrl = Environment.GetEnvironmentVariable("BASE_API_URL");
        }

        // This function is triggered by an HTTP GET request to retrive a users posts
        [Function("FindPost")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "Posts/Search")] HttpRequest req)
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            SearchCriteria? searchCriteria;
            try
            {
                searchCriteria = JsonSerializer.Deserialize<SearchCriteria>(requestBody);
                if (searchCriteria != null)
                {
                    var (invalid, errorMessage) = searchCriteria.validate();
                    if (invalid)
                    {
                        _logger.LogError($"Search Criteria are not valid: {errorMessage}");
                        return new BadRequestObjectResult(errorMessage);
                    }
                    searchCriteria.PageStart = 0;
                    searchCriteria.PageSize = 20;
                    requestBody = JsonSerializer.Serialize(searchCriteria);
                }
                else
                {
                    _logger.LogError("Input was null");
                    return new BadRequestObjectResult("Input was null");
                }
            }
            catch (JsonException ex)
            {
                _logger.LogError($"JSON deserialization failed: {ex.Message}");
                return new BadRequestObjectResult("Incomplete Search Criteria.");
            }
            _logger.LogInformation($"Raw Request Body: {requestBody}");

            string endpoint = $"{_baseApiUrl}/api/posts/search";
            var (error, response) = await _httpRequestHandler.MakeHttpPostRequest(endpoint, requestBody);

            if (!error)
            {
                var posts = JsonSerializer.Deserialize<List<PostDetails>>(response);
                if (posts == null || posts.Count == 0)
                {
                    _logger.LogInformation("No posts found for the search criteria.");
                    return new OkObjectResult("[]");
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
    }
}
