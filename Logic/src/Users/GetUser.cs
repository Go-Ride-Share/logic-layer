using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Moq;
using System.Net.Http.Headers;
using System.Text.Json;

namespace GoRideShare
{
    public class GetUser
    {
        private readonly ILogger<GetUser> _logger;
        private readonly IHttpRequestHandler _httpRequestHandler;
        private readonly string? _baseApiUrl;

        public GetUser(ILogger<GetUser> logger, IHttpRequestHandler httpRequestHandler)
        {
            _logger = logger;
            _httpRequestHandler = httpRequestHandler;
            _baseApiUrl = Environment.GetEnvironmentVariable("BASE_API_URL");
        }

        // This function is triggered by an HTTP GET request to retrieve user information
        [Function("GetUser")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest req)
        {
            // Read the user ID and the db token from the headers
            if (!req.Headers.TryGetValue("X-User-ID", out var userId))
            {
                return new BadRequestObjectResult("Missing the following header: 'X-User-ID'.");
            }
            if (!req.Headers.TryGetValue("X-Db-Token", out var db_token))
            {
                return new BadRequestObjectResult("Missing the following header 'X-Db-Token'.");
            }

            string endpoint = $"{_baseApiUrl}/api/GetUser";
            var (error, response) = await _httpRequestHandler.MakeHttpGetRequest(endpoint, db_token, userId.ToString());

            if (!error)
            {
                return new OkObjectResult(response);
            }
            else
            {
                if (response.Equals("404: Not Found"))
                {
                    return new NotFoundObjectResult("User not found in the database!");
                }
                else if (response.Equals("400: Bad Request"))
                {
                    return new BadRequestObjectResult(response);
                }
                return new ObjectResult("Error connecting to the DB layer.")
                {
                    StatusCode = StatusCodes.Status500InternalServerError
                };
            }
        }
    }
}
