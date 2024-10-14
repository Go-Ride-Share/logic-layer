using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace GoRideShare
{
    public class FindRides (ILogger<FindRides> logger)
    {
        private readonly ILogger<FindRides> _logger = logger;

        [Function("FindRides/InterCity")]
        public IActionResult InterCity([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest req)
        {
            return new OkObjectResult("Welcome to Azure Functions!");
        }

        [Function("FindRides/IntraCity")]
        public IActionResult IntraCity([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest req)
        {
            return new OkObjectResult("Welcome to Azure Functions!");
        }
    }
}
