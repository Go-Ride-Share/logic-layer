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
        public string PosterId { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("originLat")]
        public float OriginLat { get; set; }

        [JsonPropertyName("originLng")]
        public float OriginLng { get; set; }

        [JsonPropertyName("destinationLat")]
        public float DestinationLat { get; set; }

        [JsonPropertyName("destinationLng")]
        public float DestinationLng { get; set; }

        [JsonPropertyName("price")]
        public float Price { get; set; }

        [JsonPropertyName("seatsAvailable")]
        public int SeatsAvailable { get; set; }

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