namespace GoRideShare
{
    public interface IHttpRequestHandler
    {
        Task<(bool, string)> MakeHttpGetRequest(string endpoint, string? db_token, string userId);

        Task<(bool, string)> MakeHttpPostRequest(string endpoint, string body, string? db_token, string userId);

        Task<(bool, string)> MakeHttpPatchRequest(string endpoint, string body, string? db_token, string userId);
    }

}