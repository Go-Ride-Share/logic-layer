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
            // If validation result is not null, return the bad request result
            var validationResult = Utilities.ValidateHeaders(req.Headers, out string userId, out string dbToken);
            if (validationResult != null)
            {
                return validationResult;
            }

            string endpoint = $"{_baseApiUrl}/api/GetUser";
            var (error, response) = await _httpRequestHandler.MakeHttpGetRequest(endpoint, dbToken, userId.ToString());

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
