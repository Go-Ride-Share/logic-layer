using System.Configuration;
using System.Text.Json.Serialization;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GoRideShare
{
    public static class Utilities
    {
        // Method that validates headers, outputs the userID and db_token, returns exception if headers  missing, null if headers are good
        public static IActionResult? ValidateHeaders(IHeaderDictionary headers, out string userId, out string db_token)
        {
            userId = string.Empty;
            db_token = string.Empty;

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
            db_token = db_tokenValue.ToString();

            return null; // All headers are valid
        }
    }
}