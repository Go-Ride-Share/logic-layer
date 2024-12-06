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
        private readonly Utilities _utilities;

        public GetUser(ILogger<GetUser> logger, IHttpRequestHandler httpRequestHandler, Utilities utilities)
        {
            _logger = logger;
            _utilities = utilities;
            _httpRequestHandler = httpRequestHandler;
            _baseApiUrl = Environment.GetEnvironmentVariable("BASE_API_URL");
        }

        // This function is triggered by an HTTP GET request to retrieve user information
        [Function("UserGet")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "Users/{user_id}")] HttpRequest req, string? user_id)
        {
            // If validation result is not null, return the bad request result
            var validationResult = _utilities.ValidateHeadersAndTokens(req.Headers, out string userId, out string db_token);
            if (validationResult != null)
            {
                return validationResult;
            }

            if (string.IsNullOrEmpty(user_id?.Trim()))
            {
                _logger.LogError("Invalid Query Parameter: `user_id` not passed");
                return new BadRequestObjectResult("Invalid Query Parameter: `user_id` not passed");
            } else {
                _logger.LogInformation($"user_id: {user_id}");
            }

            string endpoint = $"{_baseApiUrl}/api/users/{userId}";
            _logger.LogInformation($"Endpoint: {endpoint}");
            var (error, response) = await _httpRequestHandler.MakeHttpGetRequest(endpoint, db_token, user_id);

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
