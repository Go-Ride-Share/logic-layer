## `AccountManager` Function App API Definition

### /CreateUser
Request paylaod template:
```
{
    email: string, ex "test@email.com",
    password: string, ex "testPassword",
    name: string, ex "testName",
    bio: string, ex "testBio",
    phone: string, ex "4312245323",
    photo: string, ex "testPhotoUrl"
}
```

Response payload template:
```
{
    logic_token: string, ex "oauth-token-generated-by-microsoft-for-logic-access",
    db_token: string, ex "oauth-token-generated-by-microsoft-for-db-access"
}
```

### /VerifyLoginCredentials
Request paylaod template:
```
{
    email: string, ex "test@email.com",
    password: string, ex "testPassword"
}
```

Response payload template:
```
{
    logic_token: string, ex "oauth-token-generated-by-microsoft-for-logic-access",
    db_token: string, ex "oauth-token-generated-by-microsoft-for-db-access"
}
```

## `Logic` Function App API Definition

### /FindRides/Intercity
Request paylaod template:
```
{
    origin: string, ex "Winnipeg, MB, Canada"
    destination: string, ex "Winnipeg, MB, Canada"
    date: date, the date trip is leaving
    seatsNeeded: 1,
}
```

### /FindRides/Intracity
Request payload template:
```
{
    city: string, ex "Winnipeg, MB, Canada"
    destination: float, coordinates of where the passnager wants to go
    maxWalkingMinutes: int, how many minutes the passanger is willing to walk to get to their final destination
    departureMinDateTime: DateTime, this is the earliest the passanger is willing to depart
    departureMaxDateTime: DateTime, this is the latest the passanger is willing to depart
    date: string, the date trip is leaving
    seatsNeeded: 1,
}
```
