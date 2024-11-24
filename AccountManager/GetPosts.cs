using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using System.Net.Http.Headers;
using Microsoft.Extensions.Primitives;

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

        // This function is triggered by an HTTP GET request to retrieve all posts
        [Function("PostsGet")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "Posts")] HttpRequest req)
        {

            string? post_id = null;
            string endpoint = "";
            if (req.Query.TryGetValue("postId", out StringValues postIdParam))
            {
                Guid post_guid = Guid.Empty;
                if (Guid.TryParse(postIdParam[0], out post_guid))
                {
                    post_id = post_guid.ToString();
                    endpoint = $"{_baseApiUrl}/api/posts/?post_id={post_id}";
                    var (error, response) = await _httpRequestHandler.MakeHttpGetRequest(endpoint);

                    if (!error)
                    {
                        List<PostDetails>? posts_list = JsonSerializer.Deserialize<List<PostDetails>>(response);
                        if (posts_list == null || posts_list.Count != 1)
                        {
                            _logger.LogError($"Error fetching post");
                            return new OkObjectResult("{}");
                        }
                        return new OkObjectResult(posts_list[0]);
                    }
                    else
                    {
                        _logger.LogError("Error connecting to the DB layer.");
                        _logger.LogError(response);
                        return new ObjectResult("Error connecting to the DB layer.")
                        {
                            StatusCode = StatusCodes.Status500InternalServerError
                        };
                    }
                }
                else
                {
                    _logger.LogError("Invalid postId query param");
                    return new BadRequestObjectResult("ERROR: Invalid Query Parameter: postId");
                }
            }
            else
            {
                endpoint = $"{_baseApiUrl}/api/Posts";
                var (error, response) = await _httpRequestHandler.MakeHttpGetRequest(endpoint);

                if (!error)
                {
                    var posts = JsonSerializer.Deserialize<List<PostDetails>>(response);
                    if (posts == null || posts.Count == 0)
                    {
                        _logger.LogError("No posts found in the response from the DB layer.");
                        return new OkObjectResult("[]");
                    }
                    return new OkObjectResult(posts);
                }
                else
                {
                    _logger.LogError("Error connecting to the DB layer.");
                    _logger.LogError(response);
                    return new ObjectResult("Error connecting to the DB layer.")
                    {
                        StatusCode = StatusCodes.Status500InternalServerError
                    };
                }
            }
        }
    }
}
