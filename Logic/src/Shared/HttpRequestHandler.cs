using System.Text;
using System.Net.Http.Headers;

namespace GoRideShare
{
    public class HttpRequestHandler : IHttpRequestHandler
    {

        private static readonly HttpClient _httpClient = new HttpClient();

        public async Task<(bool, string)> MakeHttpGetRequest(string endpoint, string? db_token, string userId)
        {
            try
            {
                var requestMessage = new HttpRequestMessage(HttpMethod.Get, endpoint) { };
                requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", db_token);
                requestMessage.Headers.Add("X-User-ID", userId);
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
    
        public async Task<(bool, string)> MakeHttpPostRequest(string endpoint, string body, string? db_token, string userId)
        {
            try
            {
                var requestMessage = new HttpRequestMessage(HttpMethod.Post, endpoint) 
                {
                    Content = new StringContent(body, Encoding.UTF8, "application/json")
                };
                requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", db_token);
                requestMessage.Headers.Add("X-User-ID", userId);
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

        public async Task<(bool, string)> MakeHttpPatchRequest(string endpoint, string body, string? db_token, string userId)
        {
            try
            {
                var requestMessage = new HttpRequestMessage(HttpMethod.Patch, endpoint) 
                {
                    Content = new StringContent(body, Encoding.UTF8, "application/json")
                };
                requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", db_token);
                requestMessage.Headers.Add("X-User-ID", userId);
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