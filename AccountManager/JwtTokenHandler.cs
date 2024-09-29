using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

public class JwtTokenHandler
{
    // Secret key used to sign the JWT tokens
    private readonly string? _secretKey;
    // The issuer of the token
    private readonly string _issuer;
    // The users of the token
    private readonly string _audience;

    public JwtTokenHandler(string? secretKey, string issuer, string audience)
    {
        _secretKey = secretKey;
        _issuer = issuer;
        _audience = audience;
    }

    // Method to generate a JWT token based on the provided email
    public string GenerateToken(string email)
    {
        // Define the claims to include in the token (in this case, the user's email)
        var claims = new[]
        {
            new Claim(ClaimTypes.Email, email)
        };

        // Check if the secret key is provided
        if(_secretKey != null)   
        {
            // Create a symmetric security key using the secret key
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
            // Create signing credentials using HMAC SHA256 algorithm
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // Create the token with the specified issuer, audience, claims, and expiry (1 day)
            var token = new JwtSecurityToken(
                issuer: _issuer,
                audience: _audience,
                claims: claims,
                expires: DateTime.Now.AddDays(1),
                signingCredentials: creds);

            // Serialize the token to a string and return it
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        // If secret key is not provided, return an empty string
        return "";
    }

    // Method to validate the JWT token and return the associated claims principal
    public ClaimsPrincipal? ValidateToken(string token)
    {
        // Check if the secret key is provided
        if(_secretKey != null)   
        {
            // Create a symmetric security key using the secret key
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
            // Initialize the token handler to process the JWT token
            var tokenHandler = new JwtSecurityTokenHandler();

            // Define the parameters for validating the token
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true, // Validate the issuer of the token
                ValidateAudience = true, // Validate the audience of the token
                ValidateLifetime = true, // Validate token expiration
                ValidateIssuerSigningKey = true, // Ensure token is signed by the correct key
                ValidIssuer = _issuer, // Specify the valid issuer
                ValidAudience = _audience, // Specify the valid audience
                IssuerSigningKey = key // Use the secret key to validate the signature
            };
            try
            {
                // Validate the token and return the claims principal (user identity)
                return tokenHandler.ValidateToken(token, validationParameters, out _);
            }
            catch
            {
                // Return null if validation fails
                return null;
            }
        }
        
        // Return null if secret key is not provided
        return null;
    }
}
