using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using System.Net.Http.Headers;

namespace GoRideShare
{
    public interface IHttpRequestHandler
    {
        Task<(bool, string)> MakeHttpGetRequest(string endpoint, string? dbToken, string userId);

        Task<(bool, string)> MakeHttpPostRequest(string endpoint, string body, string? dbToken, string userId);
    }

}