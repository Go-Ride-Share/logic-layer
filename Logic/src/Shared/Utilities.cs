using System.Configuration;
using System.Text.Json.Serialization;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GoRideShare
{
    public static class Utilities
    {
        // Method that validates headers, outputs the userID and dbToken, returns exception if headers  missing, null if headers are good
        public static IActionResult ValidateHeaders(IHeaderDictionary headers, out string userId, out string dbToken)
        {
            userId = string.Empty;
            dbToken = string.Empty;

            // Check for X-User-ID  and X-DbToken headers
            if (!headers.TryGetValue("X-User-ID", out var userIdValue))
            {
                return new BadRequestObjectResult("Missing the following header: 'X-User-ID'.");
            }
            userId = userIdValue.ToString();

            if (!headers.TryGetValue("X-Db-Token", out var dbTokenValue))
            {
                return new BadRequestObjectResult("Missing the following header: 'X-Db-Token'.");
            }
            dbToken = dbTokenValue.ToString();

            return null; // All headers are valid
        }
    }
}