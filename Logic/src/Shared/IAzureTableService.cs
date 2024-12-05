namespace GoRideShare
{
    public interface IAzureTableService
    {
        (bool, string) VerifyUserTokens(string endpoint, string? db_token, string userId);
    }

}