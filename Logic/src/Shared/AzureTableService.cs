using Microsoft.AspNetCore.Mvc;
using Azure.Data.Tables;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using System.Net.Http.Headers;

namespace GoRideShare
{
    public class AzureTableService : IAzureTableService
    {

        private static readonly HttpClient _httpClient = new HttpClient();

        public (bool, string) VerifyUserTokens(string logicToken, string userId, string dbToken)
        {
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
                return (true, "Tokens do not match, please sign in again!");
            }
            return (false, "Tokens match");
        }
    
 }

}