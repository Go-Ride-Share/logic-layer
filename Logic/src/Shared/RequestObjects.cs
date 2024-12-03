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
        public string? PostId { get; set; }
        
        [JsonPropertyName("posterId")]
        public string? PosterId { get; set; }
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

        [JsonRequired]
        [JsonPropertyName("departureDate")]
        public required string DepartureDate { get; set; }
        
        [JsonPropertyName("createdAt")]
        public DateTime CreatedAt { get; set; }

        public PostDetails(){}

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
            if ( 90 < OriginLat || OriginLat < -90 )
            {
                return (true, "OriginLat is Invalid");
            }
            if ( 180 < OriginLng || OriginLng < -180 )
            {
                return (true, "OriginLat is Invalid");
            }
            if ( 180 < OriginLng || OriginLng < -180 )
            {
                return (true, "OriginLng is Invalid");
            }
            if ( 180 < DestinationLng || DestinationLng < -180 )
            {
                return (true, "DestinationLng is Invalid");
            }

            return (false, "");
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

        [JsonPropertyName("photo")]
        public string Photo { get; set; }
        
        public User
        (
            string userId,
            string name,
            string photo
        )
        {
            UserId = userId;
            Name = name;
            Photo = photo;
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

        [JsonPropertyName("postId")]
        public string PostId { get; set; }

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

        public Message
        (
            DateTime timeStamp,
            string senderId,
            string contents
        )
        {
            TimeStamp = timeStamp;
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

    public class  SearchCriteria
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

        [JsonRequired]
        [JsonPropertyName("pageStart")]
        public required float PageStart { get; set; }

        [JsonRequired]
        [JsonPropertyName("pageSize")]
        public required float PageSize { get; set; }

        [JsonRequired]
        [JsonPropertyName("price")]
        public float? Price { get; set; }

        [JsonRequired]
        [JsonPropertyName("departureDate")]
        public required string DepartureDate { get; set; }

        public SearchCriteria(){}

        public (bool, string) validate()
        {
            if (DepartureDate == "")
            {
                return (true, "DepartureDate cannot be empty");
            }

            //
            //  Parse into the correct Date Format
            //

            if ( 90 < OriginLat || OriginLat < -90 )
            {
                return (true, "OriginLat is Invalid");
            }
            if ( 180 < OriginLng || OriginLng < -180 )
            {
                return (true, "OriginLat is Invalid");
            }
            if ( 180 < OriginLng || OriginLng < -180 )
            {
                return (true, "OriginLng is Invalid");
            }
            if ( 180 < DestinationLng || DestinationLng < -180 )
            {
                return (true, "DestinationLng is Invalid");
            }
            if ( PageSize < 1 )
            {
                return (true, "PageSize is Invalid");
            }
            if ( PageStart < 0 )
            {
                return (true, "PageStart is Invalid");
            }
            
            return (false, "");
        }
    }
}