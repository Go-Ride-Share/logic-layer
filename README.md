
# Go Ride Share Logic Layer API

## Overview
The `Go Ride Share Logic Layer API` is an internal service for managing user accounts, posts, conversations, and messaging functionalities for the ride-sharing application. Below is a detailed explanation of each API endpoint, including request and response formats.

---

## Users

### **POST /api/users**
**Description:** Creates a new user.  
**Request Payload:**
```json
{
  "email": "test@email.com",
  "password": "testPassword",
  "name": "testName",
  "bio": "testBio",
  "phone": "4312245323",
  "photo": "photo_encoding"
}
```
**Response Payload:**
```json
{
  "user_id": "new_user_id",
  "photo": "photo_encoding",
  "db_token": "data-layer-access-token",
  "logic_token": "logic-layer-access-token",
}
```

### **GET /api/users/{user_id}**
**Description:** Retrieves a user's profile.  
**Path Parameter:**
- `user_id` (string): ID of the user to retrieve.

**Response Payload:**
```json
{
  "email": "test@email.com",
  "name": "testName",
  "bio": "testBio",
  "phone": "4312245323",
  "photo": "photo_encoding"
}
```

### **PATCH /api/users/{user_id}**
**Description:** Edits an existing user's details.  
**Path Parameter:**
- `user_id` (string): ID of the user to edit.

**Request Payload:**
```json
{
  "name": "new_name",
  "bio": "new_bio",
  "phone": "new_phone_number",
  "photo": "new_photo_encoding"
}
```
**Response Payload:**
```json
{
  "user_id": "user_id",
  "photo": "string"
}
```

### **POST /api/users/passwordlogin**
**Description:** Authenticates a user using email and password.  
**Request Payload:**
```json
{
  "email": "test@email.com",
  "password": "testPassword"
}
```
**Response Payload:**
```json
{
  "user_id": "user_id",
  "photo": "photo_encoding",
  "db_token": "data-layer-access-token",
  "logic_token": "logic-layer-access-token"
}
```

### **POST /api/users/googlelogin**
**Description:** Authenticates a user using Google login.  
**Request Payload:**
```json
{
  "email": "google_user@gmail.com",
  "password": "password"
}
```
**Response Payload:**
```json
{
  "user_id": "user_id",
  "photo": "photo_encoding",
  "db_token": "data-layer-access-token",
  "logic_token": "logic-layer-access-token",
}
```

---

## Posts

### **POST /api/posts**
**Description:** Creates a new post.  

**Request Payload:**
```json
{
  "name": "Tets Post",
  "description": "Description",
  "departureDate": "2024-12-06T02:40:06Z",
  "originName": "OName",
  "originLat": 40.712776,
  "originLng": -74.005974,
  "destinationName": "DName",
  "destinationLat": 34.052235,
  "destinationLng": -118.24368,
  "price": 30.0,
  "seatsAvailable": 2
}
```
**Response Payload:**
```json
{
  "post_id": "new_post_id"
}
```

### **GET /api/posts**
**Description:** Retrieves all posts.

**Response Payload:**
```json
[
  {
    "post_id": "post_id",
    "posterId": "poster_id",
    "name": "Test post 1",
    "description": "Description",
    "departureDate": "2024-12-06T02:40:06Z",
    "originName": "OName",
    "originLat": 40.712776,
    "originLng": -74.005974,
    "destinationName": "DName",
    "destinationLat": 34.052235,
    "destinationLng": -118.24368,
    "price": 30.0,
    "seatsAvailable": 2,
    "createdAt": "2024-12-03T02:40:06Z",
    "user": {
        "userId": "user_id",
        "name": "name",
        "photo": "photo_encoding"
    }
  },
  {
    "post_id": "post_id",
    "posterId": "poster_id",
    "name": "Test post 2",
    "description": "Description",
    "departureDate": "2024-12-09T02:40:06Z",
    "originName": "OName",
    "originLat": 40.712776,
    "originLng": -74.005974,
    "destinationName": "DName",
    "destinationLat": 34.052235,
    "destinationLng": -118.24368,
    "price": 30.0,
    "seatsAvailable": 2,
    "createdAt": "2024-12-03T02:40:06Z",
    "user": {
        "userId": "user_id",
        "name": "name",
        "photo": "photo_encoding"
    }
  }
]
```
### **GET /api/posts?postId={post_id}**
**Description:** Retrieves a single post.  
**Query Parameters:**
- `postId` (string): Post ID to get the single post from the DB.

**Response Payload:**
```json
{
  "post_id": "post_id",
  "posterId": "poster_id",
  "name": "Test post 1",
  "description": "Description",
  "departureDate": "2024-12-06T02:40:06Z",
  "originName": "OName",
  "originLat": 40.712776,
  "originLng": -74.005974,
  "destinationName": "DName",
  "destinationLat": 34.052235,
  "destinationLng": -118.24368,
  "price": 30.0,
  "seatsAvailable": 2,
  "createdAt": "2024-12-03T02:40:06Z",
  "user": {
      "userId": "user_id",
      "name": "name",
      "photo": "photo_encoding"
  }
}
```

### **GET /api/posts/{user_id}**
**Description:** Retrieves all posts made by a specific user.  
**Path Parameter:**
- `user_id` (string): User ID of the poster.

**Response Payload:**
```json
[
  {
    "post_id": "post_id",
    "posterId": "poster_id",
    "name": "Test post 1",
    "description": "Description",
    "departureDate": "2024-12-06T02:40:06Z",
    "originName": "OName",
    "originLat": 40.712776,
    "originLng": -74.005974,
    "destinationName": "DName",
    "destinationLat": 34.052235,
    "destinationLng": -118.24368,
    "price": 30.0,
    "seatsAvailable": 2,
    "createdAt": "2024-12-03T02:40:06Z"
  },
  {
    "post_id": "post_id",
    "posterId": "poster_id",
    "name": "Test post 2",
    "description": "Description",
    "departureDate": "2024-12-09T02:40:06Z",
    "originName": "OName",
    "originLat": 40.712776,
    "originLng": -74.005974,
    "destinationName": "DName",
    "destinationLat": 34.052235,
    "destinationLng": -118.24368,
    "price": 30.0,
    "seatsAvailable": 2,
    "createdAt": "2024-12-03T02:40:06Z"
  }
]
```

---

## Conversations

### **POST /api/conversations**
**Description:** Starts a new conversation between two users.  
**Request Payload:**
```json
{
  "recipientId": "recipient_user_id",
  "contents": "Hello",
  "timeStamp": "2024-12-06T12:00:00Z"
}
```
**Response Payload:**
```json
{
  "conversation_id": "conversation_id",
  "user": {
    "userId": "recipient_user_id",
    "name": "Someone",
    "photo": "photo_encoding"
  },
  "messages": [
    {
      "userId": "sender_user_id",
      "contents": "Hello",
      "timeStamp": "2024-12-06T12:00:00Z"
    }
  ]
}
```

### **GET /api/conversations**
**Description:** Retrieves all conversations for a user.  

**Response Payload:**
```json
[
  {
    "conversation_id": "conversation_id_1",
    "user": {
      "userId": "recipient_user_id",
      "name": "Name 1",
      "photo": "photo_encoding"
    },
    "messages": [
      {
        "userId": "sender_user_id",
        "contents": "Hello",
        "timeStamp": "2024-12-06T12:00:00Z"
      }
    ]
  },
  {
    "conversation_id": "conversation_id_2",
    "user": {
      "userId": "recipient_user_id",
      "name": "Name 2",
      "photo": "photo_encoding"
    },
    "messages": [
      {
        "userId": "sender_user_id",
        "contents": "Hello",
        "timeStamp": "2024-12-06T12:00:00Z"
      }
    ]
  }
]
```

---

## Messages

### **POST /api/messages**
**Description:** Sends a message in an existing conversation.  

**Request Payload:**
```json
{
  "conversationId": "conversation_id",
  "contents": "Is this ride still available?",
  "timeStamp": "2024-12-06T12:05:00Z"
}
```
**Response Payload:**
```json
{
  "id": "conversation_id"
}
```

### **GET /api/messages/{conversationId}**
**Description:** Retrieves messages from a specific conversation.  
**Path Parameter:**
- `conversationId` (string): ID of the conversation.

**Query Parameters:**
- `limit` (integer): Maximum number of messages to retrieve (default: 50).
- `timeStamp` (string): Start time for retrieving messages.

**Response Payload:**
```json
[
    {
      "conversationId": "sender_user_id",
      "contents": "Hello",
      "timeStamp": "2024-12-06T12:00:00Z"
    },
    {
      "conversationId": "sender_user_id",
      "contents": "Hey",
      "timeStamp": "2024-12-07T12:00:00Z"
    }
]
```

---

## Notes
- All date and time formats should follow ISO 8601.