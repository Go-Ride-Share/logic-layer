using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GoRideShare
{
    public class Utilities
    {

        private readonly IAzureTableService _azureTableService;

        public Utilities(IAzureTableService azureTableService)
        {
            _azureTableService = azureTableService;
        }

        // Method that validates headers, outputs the userID and dbToken, and verifies that the tokens are matching 
        // returns exception if headers missing or tokens mismatch, null if all is good
        public IActionResult? ValidateHeadersAndTokens(IHeaderDictionary headers, out string userId, out string dbToken)
        {
            // Verify headers
            userId = string.Empty;
            dbToken = string.Empty;

            // Check for X-User-ID  and X-db_token headers
            if (!headers.TryGetValue("X-User-ID", out var userIdValue))
            {
                return new BadRequestObjectResult("Missing the following header: 'X-User-ID'.");
            }
            userId = userIdValue.ToString();

            if (!headers.TryGetValue("X-Db-Token", out var db_tokenValue))
            {
                return new BadRequestObjectResult("Missing the following header: 'X-Db-Token'.");
            }
            dbToken = db_tokenValue.ToString();

            // Authorization is guarteed to be a header, otherwise Azure would not let this be called
            headers.TryGetValue("Authorization", out var logicTokenValue);
            string logicToken = logicTokenValue.ToString().Replace("Bearer ", "");

            // Verify tokens
            var (invalid, errorMessage) = _azureTableService.VerifyUserTokens(logicToken, userId, dbToken);
            if (invalid)
            {
                return new UnauthorizedObjectResult(errorMessage);
            }

            return null; // All headers are valid
        }
    }
} 