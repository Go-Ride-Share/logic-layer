using System.Reflection.Metadata;
using Azure;
using Azure.Data.Tables;

namespace GoRideShare;
public class SecurityGuard
{
    static readonly string connectionString = Environment.GetEnvironmentVariable("USER_TOKENS_TABLE_CONNECTION_STRING") ?? "";
    static readonly string tableName = "UserTokens";
    
    public static async Task SaveUserTokens(string userId, string logicToken, string dbToken)
    {
        // Get all tokens stored for this user
        TableClient client = new(connectionString, tableName);
        string filter = TableClient.CreateQueryFilter($"(PartitionKey eq {userId})");
        List<TableEntity> userTokensSets = [];

        await foreach (TableEntity tokenSet in client.QueryAsync<TableEntity>(filter))
        {
            userTokensSets.Add(tokenSet);
        }

        // Save the new tokens
        if (userTokensSets.Count == 2)
        {
            // Find the older token set and update it
            TableEntity oldestEntity = userTokensSets[0];
            
            if (DateTime.Parse(userTokensSets[1]["Timestamp"].ToString() ?? "") <= DateTime.Parse(userTokensSets[0]["Timestamp"].ToString() ?? ""))
            {
                oldestEntity = userTokensSets[1];
            }

            oldestEntity["LogicToken"] = logicToken;
            oldestEntity["DbToken"] = dbToken;

            await client.UpdateEntityAsync(oldestEntity, ETag.All, TableUpdateMode.Replace);
        }
        else
        {
            string newRowKey = "2";
            if (userTokensSets.Count == 0) 
            {   
                // Adding the first token set for this user
                newRowKey = "1";
            }

            TableEntity newEntity = new(userId, newRowKey)
            {
                {"LogicToken", logicToken },
                {"DbToken", dbToken}
            };

            await client.AddEntityAsync(newEntity);
        }       
    } // SaveUserTokens
}