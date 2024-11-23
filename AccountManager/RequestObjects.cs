using System.Configuration;
using System.Text.Json.Serialization;

namespace GoRideShare
{
    public class LoginCredentials(string email, string passwordHash)
    {
        [JsonPropertyName("email")]
        public string Email { get; set; } = email;
        [JsonPropertyName("password")]
        public string PasswordHash { get; set; } = passwordHash;
    }

    public class UserRegistrationInfo
    {
        [JsonPropertyName("email")]
        public string? Email { get; set; }

        [JsonPropertyName("password")]
        public string? PasswordHash { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("bio")]
        public string? Bio { get; set; }

        [JsonPropertyName("phone")]
        public string? PhoneNumber { get; set; }

        [JsonPropertyName("photo")]
        public string? Photo { get; set; }

        [JsonPropertyName("id")]
        public string? UserId {get; set;}

        [JsonPropertyName("picture")]
        public string? PhotoUrl {get; set;}

    }

    public class PostDetails
    {
        [JsonPropertyName("postId")]
        public Guid? PostId { get; set; }

        [JsonRequired]
        [JsonPropertyName("posterId")]
        public required Guid PosterId { get; set; }

        [JsonRequired]
        [JsonPropertyName("name")]
        public required string Name { get; set; }

        [JsonRequired]
        [JsonPropertyName("description")]
        public required string Description { get; set; }

        [JsonPropertyName("originName")]
        public string? OriginName { get; set; }

        [JsonRequired]
        [JsonPropertyName("originLat")]
        public required float OriginLat { get; set; }

        [JsonRequired]
        [JsonPropertyName("originLng")]
        public required float OriginLng { get; set; }

        [JsonPropertyName("destinationName")]
        public string? DestinationName { get; set; }

        [JsonRequired]
        [JsonPropertyName("destinationLat")]
        public required float DestinationLat { get; set; }

        [JsonRequired]
        [JsonPropertyName("destinationLng")]
        public required float DestinationLng { get; set; }

        [JsonRequired]
        [JsonPropertyName("price")]
        public required float Price { get; set; }

        [JsonRequired]
        [JsonPropertyName("seatsAvailable")]
        public required int SeatsAvailable { get; set; }

        [JsonPropertyName("seatsTaken")]
        public int? SeatsTaken { get; set; }

        [JsonRequired]
        [JsonPropertyName("departureDate")]
        public required string DepartureDate { get; set; }

        [JsonRequired]
        [JsonPropertyName("createdAt")]
        public required DateTime CreatedAt { get; set; }

        [JsonRequired]
        [JsonPropertyName("user")]
        public required User Poster { get; set; }

        public PostDetails() { }

        public (bool, string) validate()
        {
            if (DepartureDate == "")
            {
                return (true, "DepartureDate cannot be empty");
            }
            if (Description == "")
            {
                return (true, "Description cannot be empty");
            }
            if (Name == "")
            {
                return (true, "Name cannot be empty");
            }
            if (90 < OriginLat || OriginLat < -90)
            {
                return (true, "OriginLat is Invalid");
            }
            if (180 < OriginLng || OriginLng < -180)
            {
                return (true, "OriginLat is Invalid");
            }
            if (180 < OriginLng || OriginLng < -180)
            {
                return (true, "OriginLng is Invalid");
            }
            if (180 < DestinationLng || DestinationLng < -180)
            {
                return (true, "DestinationLng is Invalid");
            }

            var (error, response) = Poster.validate();
            if (error)
            {
                return (error, response);
            }

            return (false, "");
        }
    }

    public class SearchCriteria
    {
        [JsonRequired]
        [JsonPropertyName("originLat")]
        public required float OriginLat { get; set; }

        [JsonRequired]
        [JsonPropertyName("originLng")]
        public required float OriginLng { get; set; }

        [JsonRequired]
        [JsonPropertyName("destinationLat")]
        public required float DestinationLat { get; set; }

        [JsonRequired]
        [JsonPropertyName("destinationLng")]
        public required float DestinationLng { get; set; }

        [JsonPropertyName("pageStart")]
        public float PageStart { get; set; }

        [JsonPropertyName("pageSize")]
        public float PageSize { get; set; }

        [JsonPropertyName("price")]
        public float? Price { get; set; }

        [JsonPropertyName("numSeats")]
        public int? NumSeats { get; set; }

        [JsonPropertyName("departureDate")]
        public string? DepartureDate { get; set; }

        public SearchCriteria() { }

        public (bool, string) validate()
        {
            if (DepartureDate != null)
            {
                if (DepartureDate == "")
                {
                    return (true, "DepartureDate cannot be empty");
                }

                string dateFormat = "yyyy-MM-ddTHH:mm:ss.fffZ"; // ISO 8601 format
                if (!DateTimeOffset.TryParseExact(
                        DepartureDate,
                        dateFormat,
                        null,
                        System.Globalization.DateTimeStyles.AssumeUniversal,
                        out DateTimeOffset result
                ))
                {
                    return (true, "DepartureDate uses an Incorrect format; \n Instead use: yyyy-MM-ddTHH:mm:ss.fffZ (ISO 8601 format)");
                }
            }

            if (90 < OriginLat || OriginLat < -90)
            {
                return (true, "OriginLat is Invalid");
            }
            if (180 < OriginLng || OriginLng < -180)
            {
                return (true, "OriginLat is Invalid");
            }
            if (180 < OriginLng || OriginLng < -180)
            {
                return (true, "OriginLng is Invalid");
            }
            if (180 < DestinationLng || DestinationLng < -180)
            {
                return (true, "DestinationLng is Invalid");
            }
            return (false, "");
        }
    }

    public class DbLayerResponse
    {
        [JsonPropertyName("user_id")]
        public string? UserId { get; set; }
        [JsonPropertyName("photo")]
        public string? Photo { get; set; }
    }

    public class User
    {
        [JsonPropertyName("userId")]
        public Guid? UserId { get; set; }

        [JsonRequired]
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("photo")]
        public string? Photo { get; set; }

        public User() { }

        public User
        (
            Guid userId,
            string name,
            string? photo
        )
        {
            UserId = userId;
            Name = name;
            Photo = photo;
        }

        public (bool, string) validate()
        {
            if (Name == "")
            {
                return (true, "name cannot be empty");
            }
            if (Photo == "")
            {
                return (true, "photo cannot be empty");
            }
            return (false, "");
        }
    }

    public class GoogleOAuthResponse
    {
        [JsonPropertyName("access_token")]
        public string? AccessToken {get; set;}
        [JsonPropertyName("id_token")]
        public string? IdToken {get; set;}
    }
}