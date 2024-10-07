using System.Configuration;
using System.Text.Json.Serialization;

namespace GoRideShare
{
    public class DbLayerResponse
    {
        [JsonPropertyName("post_id")]
        public string? PostId { get; set; }
    }

    public class PostDetails
    {
        [JsonPropertyName("postId")]
        public string PostId { get; set; }
        
        [JsonPropertyName("posterId")]
        public required string PosterId { get; set; }

        [JsonPropertyName("name")]
        public required string Name { get; set; }

        [JsonPropertyName("description")]
        public required string Description { get; set; }

        [JsonPropertyName("originLat")]
        public required float OriginLat { get; set; }

        [JsonPropertyName("originLng")]
        public required float OriginLng { get; set; }

        [JsonPropertyName("destinationLat")]
        public required float DestinationLat { get; set; }

        [JsonPropertyName("destinationLng")]
        public required float DestinationLng { get; set; }

        [JsonPropertyName("price")]
        public required float Price { get; set; }

        [JsonPropertyName("seatsAvailable")]
        public required int SeatsAvailable { get; set; }

        public PostDetails() { }

        public PostDetails(
            string postId,
            string posterId,
            string name,
            string description,
            float originLat,
            float originLng,
            float destinationLat,
            float destinationLng,
            float price,
            int seatsAvailable)
        {
            PostId = postId;
            PosterId = posterId;
            Name = name;
            Description = description;
            OriginLat = originLat;
            OriginLng = originLng;
            DestinationLat = destinationLat;
            DestinationLng = destinationLng;
            Price = price;
            SeatsAvailable = seatsAvailable;
        }
    }

}