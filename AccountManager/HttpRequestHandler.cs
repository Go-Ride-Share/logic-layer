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

        public async Task<(bool, string)> MakeHttpGetRequest(string endpoint)
        {
            try
            {
                var requestMessage = new HttpRequestMessage(HttpMethod.Get, endpoint) { };
                
                // Initialize JwtTokenHandler and validate environment variables
                var jwtTokenHandler = new JwtTokenHandler(Environment.GetEnvironmentVariable("OAUTH_CLIENT_ID_DB"),
                                                        Environment.GetEnvironmentVariable("OAUTH_CLIENT_SECRET_DB"),
                                                        Environment.GetEnvironmentVariable("OAUTH_TENANT_ID_DB"),
                                                        Environment.GetEnvironmentVariable("OAUTH_SCOPE_DB"));

                // Generate the OAuth 2.0 token for db_layer
                string db_token = await jwtTokenHandler.GenerateTokenAsync();

                requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", db_token);

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
            catch (System.NotSupportedException)
            {
                return (true, "Invalid Database URL");
            }
        }
    }

}