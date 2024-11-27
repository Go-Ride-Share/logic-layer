using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text;

namespace GoRideShare
{
    public class EditUser
    {
        private readonly IHttpRequestHandler _httpRequestHandler;
        private readonly ILogger<EditUser> _logger;
        // Base URL for the API (retrieved from environment variables)
        private readonly string? _baseApiUrl;

        public EditUser(ILogger<EditUser> logger, IHttpRequestHandler httpRequestHandler)
        {
             _httpRequestHandler = httpRequestHandler;
            _logger = logger;
            _baseApiUrl = Environment.GetEnvironmentVariable("BASE_API_URL");
        }

        [Function("UserEdit")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "patch", Route = "Users")] HttpRequest req)
        {
            // If validation result is not null, return the bad request result
            var validationResult = Utilities.ValidateHeaders(req.Headers, out string userId, out string db_token);
            if (validationResult != null)
            {
                return validationResult;
            }

            // Read the body content from the request
            string requestBody;
            using (var reader = new StreamReader(req.Body))
            {
                requestBody = await reader.ReadToEndAsync();
            }

            var endpoint = $"{_baseApiUrl}/api/users";
            var (error, response) = await _httpRequestHandler.MakeHttpPatchRequest(endpoint, requestBody, db_token, userId.ToString());
            if (!error)
            {
                return new OkObjectResult(new { message = "User updated successfully" });
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
