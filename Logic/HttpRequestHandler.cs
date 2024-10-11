using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using System.Net.Http.Headers;

namespace GoRideShare
{
    public class HttpRequestHandler : IHttpRequestHandler
    {

        private static readonly HttpClient _httpClient = new HttpClient();

        public async Task<(bool, string)> MakeHttpGetRequest(string endpoint, string? dbToken, string userId)
        {
            var requestMessage = new HttpRequestMessage(HttpMethod.Get, endpoint) { };
            requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", dbToken);
            requestMessage.Headers.Add("X-User-ID", userId);

            // Call the backend API to verify the login credentials

            try
            {
                var dbLayerResponse = await _httpClient.SendAsync(requestMessage);

                if (dbLayerResponse.IsSuccessStatusCode)
                {
                    try
                    {
                        var dbResponseContent = await dbLayerResponse.Content.ReadAsStringAsync();

                        return (false, dbResponseContent);
                    }
                    catch (Exception ex)
                    {
                        return (true, ex.Message);
                    }
                }
                else if (dbLayerResponse.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return (true, "404: Not Found");
                }
                else if (dbLayerResponse.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    return (true, "400: Bad Request");
                }
                else
                {
                    var errorMessage = await dbLayerResponse.Content.ReadAsStringAsync();
                    return (true, errorMessage);
                }
            }
            catch (System.Net.Http.HttpRequestException ex)
            {
                return (true, ex.Message);
            }
        }
    }

}