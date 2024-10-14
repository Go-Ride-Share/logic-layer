using System.Configuration;
using System.Text.Json.Serialization;

namespace GoRideShare
{
    public class DbLayerResponse
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }
    }

    public class PostDetails
    {
        [JsonPropertyName("postId")]
        public string PostId { get; set; }
        
        [JsonPropertyName("posterId")]
        public string PosterId { get; set; }

        [JsonPropertyName("name")]
        public required string Name { get; set; }

        [JsonPropertyName("description")]
        public required string Description { get; set; }

        [JsonPropertyName("departureDate")]
        public required string DepartureDate { get; set; }

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
            string departureDate,
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
            DepartureDate = departureDate;
            OriginLat = originLat;
            OriginLng = originLng;
            DestinationLat = destinationLat;
            DestinationLng = destinationLng;
            Price = price;
            SeatsAvailable = seatsAvailable;
        }
    }

    public class Conversation
    {
        [JsonPropertyName("userId")]
        public string UserId { get; set; }
        
        [JsonPropertyName("conversationId")]
        public string ConversationId { get; set; }

        [JsonPropertyName("messages")]
        public List<Message> Messages { get; set; } = new List<Message>();

        public Conversation() { }

        public Conversation
        (
            string userId,
            string conversationId,
            List<Message> messages
        )
        {
            UserId = userId;
            ConversationId = conversationId;
            Messages = messages;
        }
    }

    public class Message
    {
        [JsonPropertyName("timeStamp")]
        public DateTime TimeStamp  { get; set; }
        
        [JsonPropertyName("senderId")]
        public string SenderId { get; set; }

        [JsonPropertyName("conversationId")]
        public string ConversationId { get; set; }
        
        [JsonPropertyName("contents")]
        public string Contents { get; set; }
        public Message() { }
        public Message
        (
            DateTime timestamp,
            string senderId,
            string conversationId,
            string contents
        )
        {
            TimeStamp = timestamp;
            SenderId = senderId;
            ConversationId = conversationId;
            Contents = contents;
        }
    }
}