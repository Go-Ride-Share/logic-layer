using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Grpc.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace GoRideShare
{
    public class GoogleSignIn
    {
        private readonly ILogger<GoogleSignIn> _logger;
        private static readonly HttpClient _httpClient = new();
        private readonly string? _baseApiUrl = Environment.GetEnvironmentVariable("BASE_API_URL");
        private const int Status409Conflict = 409;
        private const int Status401Unauthorized = 401;

        public GoogleSignIn(ILogger<GoogleSignIn> logger)
        {
            _logger = logger;
        }

        [Function("GoogleSignIn")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequest req)
        {
            string authorizationCode = await new StreamReader(req.Body).ReadToEndAsync();

            // Use the authorization code to get the google profile
            (UserRegistrationInfo? userInfo, string error) = await GetGoogleProfile(authorizationCode);
            if (userInfo == null)
            {
                _logger.LogError($"Could not get google prfile: {error}");
                return new BadRequestObjectResult(error);
            }
            // Check if the current user is registered
            JwtTokenHandler dbTokenHandler = new (Environment.GetEnvironmentVariable("OAUTH_CLIENT_ID_DB"),
                                                    Environment.GetEnvironmentVariable("OAUTH_CLIENT_SECRET_DB"),
                                                    Environment.GetEnvironmentVariable("OAUTH_TENANT_ID_DB"),
                                                    Environment.GetEnvironmentVariable("OAUTH_SCOPE_DB"));

            string dbToken = await dbTokenHandler.GenerateTokenAsync();
            HttpRequestMessage googleLoginRequest = new(HttpMethod.Post, $"{_baseApiUrl}/api/GoogleLogin");
            googleLoginRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", dbToken);
            googleLoginRequest.Content = new StringContent(JsonSerializer.Serialize(userInfo), Encoding.UTF8, "application/json");

            HttpResponseMessage googleLoginResponseMessage = await _httpClient.SendAsync(googleLoginRequest);
            int statusCode = (int)googleLoginResponseMessage.StatusCode;

            string googleLoginResponseContent;
            if (statusCode == Status409Conflict)
            {
                return new BadRequestObjectResult("Use email and password to log in.");
            }
            else if (statusCode == Status401Unauthorized) // Need to register the user
            {
                string createProfileError;
                (googleLoginResponseContent, createProfileError) = await CreateProfileFromGoogle(userInfo, dbToken);

                if (createProfileError != "")
                    return new BadRequestObjectResult(createProfileError);
            }
            else if(googleLoginResponseMessage.IsSuccessStatusCode)
            {
                googleLoginResponseContent = await googleLoginResponseMessage.Content.ReadAsStringAsync();
            }
            else
            {
                string loginError = $"Could not log in due to an unexpected error: {googleLoginResponseMessage.StatusCode}";
                _logger.LogError(loginError);
                return new BadRequestObjectResult(loginError);
            }
            
            DbLayerResponse? googleLoginResponse = JsonSerializer.Deserialize<DbLayerResponse>(googleLoginResponseContent);

            if (googleLoginResponse == null || string.IsNullOrEmpty(googleLoginResponse.UserId?.Trim()) 
                || string.IsNullOrEmpty(googleLoginResponse.Photo?.Trim()))
            {
                string registerUserError = $"Failed to deserialize response when trying to register Google user:{googleLoginResponseContent}";
                _logger.LogError(registerUserError);
                return new BadRequestObjectResult(registerUserError);
            }

            // Get logic token
            JwtTokenHandler logicTokenHandler = new(Environment.GetEnvironmentVariable("OAUTH_CLIENT_ID"),
                                                        Environment.GetEnvironmentVariable("OAUTH_CLIENT_SECRET"),
                                                        Environment.GetEnvironmentVariable("OAUTH_TENANT_ID"),
                                                        Environment.GetEnvironmentVariable("OAUTH_SCOPE"));
            string logicToken = await logicTokenHandler.GenerateTokenAsync();

            // Succeful return
            return new OkObjectResult(new {
                User_id = googleLoginResponse.UserId,
                Logic_token = logicToken,
                db_token = dbToken,
                Photo = googleLoginResponse.Photo
            });
        }
    
        private async Task<(UserRegistrationInfo? GetUserInfoFromGoogle, string error)> GetGoogleProfile(string authorizationCode) 
        {
            string error = "";
            try { 
                string googleTokenUrl = Environment.GetEnvironmentVariable("GOOGLE_TOKEN_URL") ?? "";
                string googleProfileUrl = Environment.GetEnvironmentVariable("GOOGLE_PROFILE_URL") ?? "";;
                string googleClientSecret = Environment.GetEnvironmentVariable("GOOGLE_CLIENT_SECRET") ?? "";
                string googleCleintId = Environment.GetEnvironmentVariable("GOOGLE_CLIENT_ID") ?? "";
                string redirectUri =  Environment.GetEnvironmentVariable("REDIRECT_URI") ?? "";

                // Use the passed authorization code to get the access token            
                var formData = new List<KeyValuePair<string, string>> 
                {
                    new("code", authorizationCode),
                    new("client_id", googleCleintId),
                    new("client_secret", googleClientSecret),
                    new("grant_type", "authorization_code"),
                    new("redirect_uri", redirectUri)
                };
            
                // Make Http request and parse the response
                HttpRequestMessage accessTokenRequest = new(HttpMethod.Post, googleTokenUrl);
                accessTokenRequest.Content = new FormUrlEncodedContent(formData);

                HttpResponseMessage accessTokenResponseMessage = await _httpClient.SendAsync(accessTokenRequest);
                if (!accessTokenResponseMessage.IsSuccessStatusCode) 
                {
                    error = $"Failed to retireve access token: HTTP Status: {accessTokenResponseMessage.StatusCode}";
                    _logger.LogError(error);
                    return (null, error);
                }
                
                string accessTokenResponseContent = await accessTokenResponseMessage.Content.ReadAsStringAsync();
                GoogleOAuthResponse? accessTokenResponseObject = JsonSerializer.Deserialize<GoogleOAuthResponse>(accessTokenResponseContent);

                // Use the recieved access token to get user google profile
                HttpRequestMessage profileRequest = new(HttpMethod.Get, googleProfileUrl);
                profileRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessTokenResponseObject?.AccessToken);
                
                HttpResponseMessage profileResponseMessage = await _httpClient.SendAsync(profileRequest);
                if (!profileResponseMessage.IsSuccessStatusCode)
                {
                    error = $"Failed to retireve profile: HTTP Status: {profileResponseMessage.StatusCode}";
                    _logger.LogError(error);
                    return (null, error);   
                }

                string profileResponseContent = await profileResponseMessage.Content.ReadAsStringAsync();
                
                // Parse the response
                UserRegistrationInfo? userInfo = JsonSerializer.Deserialize<UserRegistrationInfo>(profileResponseContent);
                
                if (userInfo == null || string.IsNullOrEmpty(userInfo.Name?.Trim()) 
                    || string.IsNullOrEmpty(userInfo.Name?.Trim()) || string.IsNullOrEmpty(userInfo.UserId?.Trim()))
                {
                    error = "Faield to desirialize user profile or the name, email or id is missing."; 
                    _logger.LogError(error);
                    return (null, error);
                }
                
                userInfo.UserId = $"googleuser-{userInfo.UserId}";
                userInfo.PasswordHash = "googleuser";

                // Finished with no errors
                return (userInfo, error);
            } 
            catch (Exception e) 
            { 
                error = $"Ran into an unexpected error {e}"; 
                _logger.LogError(error);
                return (null, error);
            }
        } // GetUserInfoFromGoogle()    
    
        private async Task<(string googleLoginResponseContent, string error)> CreateProfileFromGoogle(UserRegistrationInfo userInfo, string dbToken)
        {
            // Get the picture
            HttpRequestMessage photoRequestMessage = new(HttpMethod.Get, userInfo.PhotoUrl);
            HttpResponseMessage photoResponseMessage = await _httpClient.SendAsync(photoRequestMessage);

            if (!photoResponseMessage.IsSuccessStatusCode)
            {
                string photoError = $"Failed downloading user photo: {photoResponseMessage.StatusCode}";
                _logger.LogError(photoError);
                return ("", photoError);
            }

            byte[] photoBytes = await photoResponseMessage.Content.ReadAsByteArrayAsync();
            string photoBase64 = $"data:image/jpeg;base64,{Convert.ToBase64String(photoBytes)}";

            userInfo.Photo = photoBase64;

            // Send info for registration
            HttpRequestMessage registerUserRequestMessage = new(HttpMethod.Post, $"{_baseApiUrl}/api/CreateUser");
            registerUserRequestMessage.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", dbToken);
            registerUserRequestMessage.Content = new StringContent(JsonSerializer.Serialize(userInfo), Encoding.UTF8, "application/json");
            HttpResponseMessage registerUserResponseMessage = await _httpClient.SendAsync(registerUserRequestMessage);

            if (!registerUserResponseMessage.IsSuccessStatusCode)
            {
                string registrationError = $"Failed to register the user using Google profile: {registerUserResponseMessage.StatusCode}";
                _logger.LogError(registrationError);
                return ("", registrationError);
            }
            
            // Succeful return
            return (await registerUserResponseMessage.Content.ReadAsStringAsync(), "");
        }
    }
}