using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker.Http;
using Azure.Data.Tables;
using Microsoft.Extensions.Logging;

namespace GoRideShare
{
    public static class Utilities
    {
        // Method that validates headers, outputs the userID and dbToken, and verifies that the tokens are matching 
        // returns exception if headers missing or tokens mismatch, null if all is good
        public static IActionResult? ValidateHeadersAndTokens(IHeaderDictionary headers, out string userId, out string dbToken)
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
            string connectionString = Environment.GetEnvironmentVariable("USER_TOKENS_TABLE_CONNECTION_STRING") ?? "";
            string tableName = "UserTokens";

            // Get all tokens stored for this user
            TableClient client = new(connectionString, tableName);
            string filter = TableClient.CreateQueryFilter($"(PartitionKey eq {userId})");
            List<TableEntity> userTokensSets = [];

            foreach (TableEntity tokenSet in client.Query<TableEntity>(filter))
            {
                userTokensSets.Add(tokenSet);
            }

            bool foundValidToken = false;
            for (int i = 0; i < userTokensSets.Count && !foundValidToken; i++)
            {
                TableEntity curTokenSet = userTokensSets[i];
                string? curPartitionKey = curTokenSet["PartitionKey"].ToString();
                string? curLogicToken = curTokenSet["LogicToken"].ToString();
                string? curDbToken = curTokenSet["DbToken"].ToString();
                if (curPartitionKey == userId && curLogicToken == logicToken && curDbToken == dbToken)
                {
                    foundValidToken = true;
                }
            }

            if (!foundValidToken)
            {
                return new UnauthorizedObjectResult("Tokens do not match");
            }
            return null; // All headers are valid
        }
    }
} 