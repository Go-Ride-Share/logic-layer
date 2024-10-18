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

    public class User
    {
        [JsonRequired]
        [JsonPropertyName("userId")]
        public string UserId { get; set; }
        
        [JsonRequired]
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("Profile")]
        public string Profile { get; set; }
        
        public User
        (
            string userId,
            string name,
            string profile
        )
        {
            UserId = userId;
            Name = name;
            Profile = profile;
        }
    }

    public class Conversation
    {
        [JsonRequired]
        [JsonPropertyName("conversationId")]
        public string ConversationId { get; set; }

        [JsonRequired]
        [JsonPropertyName("messages")]
        public List<Message> Messages { get; set; }

        [JsonRequired]
        [JsonPropertyName("user")]
        public User User { get; set; }

        [JsonRequired]
        [JsonPropertyName("postId")]
        public string PostId { get; set; }

        public Conversation() { }

        public Conversation
        (
            string conversationId,
            User user,
            List<Message> messages,
            string postId
        )
        {
            User = user;
            ConversationId = conversationId;
            Messages = messages;
            PostId = postId;
        }
    }

    public class Message
    {
        [JsonRequired]
        [JsonPropertyName("timeStamp")]
        public DateTime TimeStamp  { get; set; }
        
        [JsonRequired]
        [JsonPropertyName("senderId")]
        public string SenderId { get; set; }

        [JsonRequired]
        [JsonPropertyName("contents")]
        public string Contents { get; set; }
        public Message() { }
        public Message
        (
            DateTime timestamp,
            string senderId,
            string contents
        )
        {
            TimeStamp = timestamp;
            SenderId = senderId;
            Contents = contents;
        }
    }

    public class IncomingConversationRequest
    {
        [JsonRequired]
        [JsonPropertyName("userId")]
        public string UserId  { get; set; }
        
        [JsonRequired]
        [JsonPropertyName("contents")]
        public string Contents { get; set; }

        public IncomingConversationRequest() { }
        public IncomingConversationRequest
        (
            string contents,
            string userId
        )
        {
            UserId = userId;
            Contents = contents;
        }

        public (bool, string) validate()
        {
            if ( Contents == "")
            {
                return (true, "contents cannot be empty");
            }
            if ( UserId == "")
            {
                return (true, "userId is invalid");
            }
            return (false, "");
        }
    }

    public class OutgoingConversationRequest
    {
        [JsonRequired]
        [JsonPropertyName("userId")]
        public string UserId  { get; set; }
        
        [JsonRequired]
        [JsonPropertyName("contents")]
        public string Contents { get; set; }

        [JsonRequired]
        [JsonPropertyName("timeStamp")]
        public System.DateTime TimeStamp { get; set; }

        public OutgoingConversationRequest() { }
        public OutgoingConversationRequest
        (
            string contents,
            string userId,
            System.DateTime timeStamp
        )
        {
            UserId = userId;
            Contents = contents;
            TimeStamp = timeStamp;
        }
    }

    public class IncomingMessageRequest
    {
        [JsonRequired]
        [JsonPropertyName("conversationId")]
        public string ConversationId  { get; set; }
        
        [JsonRequired]
        [JsonPropertyName("contents")]
        public string Contents { get; set; }

        public IncomingMessageRequest() { }
        public IncomingMessageRequest
        (
            string conversationId,
            string contents
        )
        {
            ConversationId = conversationId;
            Contents = contents;
        }

        public (bool, string) validate()
        {
            if ( Contents == "")
            {
                return (true, "contents cannot be empty");
            }
            if ( ConversationId == "")
            {
                return (true, "conversationId is invalid");
            }
            return (false, "");
        }
    }

    public class OutgoingMessageRequest
    {
        [JsonRequired]
        [JsonPropertyName("conversationId")]
        public string ConversationId  { get; set; }
        
        [JsonRequired]
        [JsonPropertyName("contents")]
        public string Contents { get; set; }
        
        [JsonRequired]
        [JsonPropertyName("userId")]
        public string UserId { get; set; }

        [JsonRequired]
        [JsonPropertyName("timeStamp")]
        public System.DateTime TimeStamp { get; set; }

        public OutgoingMessageRequest() { }
        public OutgoingMessageRequest
        (
            string conversationId,
            string contents,
            string userId,
            System.DateTime timeStamp
        )
        {
            ConversationId = conversationId;
            Contents = contents;
            TimeStamp = timeStamp;
            UserId = userId;
        }
    }

}