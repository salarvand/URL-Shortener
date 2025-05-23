# URL Shortener API Reference

This document provides detailed documentation for the URL Shortener API endpoints, request/response formats, and examples.

## Base URL

All API endpoints are relative to the base URL:
```
https://[your-domain]/api
```

## Authentication

Currently, the API does not require authentication. Rate limiting may be applied to prevent abuse.

## Common HTTP Status Codes

- `200 OK`: The request was successful
- `201 Created`: A new resource was created successfully
- `400 Bad Request`: The request was invalid
- `404 Not Found`: The requested resource was not found
- `500 Internal Server Error`: An error occurred on the server

## Endpoints

### Short URL Operations

#### Create a Short URL

Creates a new shortened URL with optional customization.

```
POST /ShortUrl
```

**Request Body**:

```json
{
  "originalUrl": "https://example.com/very/long/url/that/needs/shortening",
  "customShortCode": "custom123",  // Optional
  "expiresAt": "2023-12-31T23:59:59Z"  // Optional
}
```

**Response** (201 Created):

```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "originalUrl": "https://example.com/very/long/url/that/needs/shortening",
  "shortCode": "custom123",
  "shortUrl": "/s/custom123",
  "createdAt": "2023-04-15T10:30:00Z",
  "expiresAt": "2023-12-31T23:59:59Z",
  "clickCount": 0,
  "isActive": true,
  "isExpired": false
}
```

**Error Responses**:

- `400 Bad Request`:
  ```json
  {
    "message": "Validation failed",
    "errors": {
      "originalUrl": ["Original URL is required", "URL format is invalid"]
    }
  }
  ```

- `400 Bad Request` (for duplicate short code):
  ```json
  {
    "message": "Short code 'custom123' is already in use"
  }
  ```

#### Get a Short URL by Code

Retrieves information about a short URL using its code.

```
GET /ShortUrl/{code}
```

**Parameters**:
- `code` (path parameter): The short code of the URL

**Response** (200 OK):

```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "originalUrl": "https://example.com/very/long/url/that/needs/shortening",
  "shortCode": "custom123",
  "shortUrl": "/s/custom123",
  "createdAt": "2023-04-15T10:30:00Z",
  "expiresAt": "2023-12-31T23:59:59Z",
  "clickCount": 5,
  "isActive": true,
  "isExpired": false
}
```

**Error Responses**:

- `404 Not Found`:
  ```json
  {
    "message": "Short URL with code 'custom123' not found"
  }
  ```

#### Get All Short URLs

Retrieves a list of all short URLs.

```
GET /ShortUrl
```

**Response** (200 OK):

```json
[
  {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "originalUrl": "https://example.com/very/long/url/that/needs/shortening",
    "shortCode": "custom123",
    "shortUrl": "/s/custom123",
    "createdAt": "2023-04-15T10:30:00Z",
    "expiresAt": "2023-12-31T23:59:59Z",
    "clickCount": 5,
    "isActive": true,
    "isExpired": false
  },
  {
    "id": "4fa85f64-5717-4562-b3fc-2c963f66afb7",
    "originalUrl": "https://another-example.com/page",
    "shortCode": "abc123",
    "shortUrl": "/s/abc123",
    "createdAt": "2023-04-16T14:20:00Z",
    "expiresAt": null,
    "clickCount": 10,
    "isActive": true,
    "isExpired": false
  }
]
```

#### Redirect to Original URL

Redirects to the original URL and records click statistics.

```
GET /ShortUrl/redirect/{code}
```

**Parameters**:
- `code` (path parameter): The short code of the URL

**Response** (302 Found):
- Redirects to the original URL

**Error Responses**:

- `404 Not Found`:
  ```json
  {
    "message": "Short URL with code 'custom123' not found or has expired"
  }
  ```

### Storage Management Operations

#### Get Storage Statistics

Retrieves statistics about the URL shortener database.

```
GET /Storage/statistics
```

**Response** (200 OK):

```json
{
  "totalUrls": 100,
  "activeUrls": 80,
  "expiredUrls": 20,
  "totalClickStatistics": 5000,
  "urlStorageBytes": 20000,
  "clickStatisticsStorageBytes": 150000,
  "totalStorageBytes": 170000
}
```

#### Purge Expired URLs

Removes expired URLs from the primary storage and optionally archives them.

```
POST /Storage/purge
```

**Response** (200 OK):

```json
{
  "purgedCount": 15,
  "message": "Successfully purged 15 expired URLs"
}
```

#### Compress Old URL Data

Compresses URL data older than a specified age.

```
POST /Storage/compress
```

**Request Body**:

```json
{
  "olderThanDays": 90  // Optional, defaults to 90
}
```

**Response** (200 OK):

```json
{
  "compressedCount": 25,
  "message": "Successfully compressed 25 URL records"
}
```

#### Aggregate Old Click Statistics

Aggregates click statistics older than a specified age.

```
POST /Storage/aggregate
```

**Request Body**:

```json
{
  "olderThanDays": 30  // Optional, defaults to 30
}
```

**Response** (200 OK):

```json
{
  "aggregatedCount": 1000,
  "message": "Successfully aggregated 1000 click statistics"
}
```

## Using the API with cURL

### Create a Short URL

```bash
curl -X POST https://your-domain.com/api/ShortUrl \
  -H "Content-Type: application/json" \
  -d '{"originalUrl": "https://example.com/long/url"}'
```

### Get a Short URL by Code

```bash
curl -X GET https://your-domain.com/api/ShortUrl/abc123
```

### Redirect to Original URL

```bash
curl -X GET -L https://your-domain.com/api/ShortUrl/redirect/abc123
```

## Using the API with JavaScript

### Create a Short URL

```javascript
fetch('https://your-domain.com/api/ShortUrl', {
  method: 'POST',
  headers: {
    'Content-Type': 'application/json'
  },
  body: JSON.stringify({
    originalUrl: 'https://example.com/long/url',
    customShortCode: 'mycode',
    expiresAt: new Date(Date.now() + 7 * 24 * 60 * 60 * 1000).toISOString()
  })
})
.then(response => response.json())
.then(data => console.log(data))
.catch(error => console.error('Error:', error));
```

### Get a Short URL by Code

```javascript
fetch('https://your-domain.com/api/ShortUrl/abc123')
.then(response => response.json())
.then(data => console.log(data))
.catch(error => console.error('Error:', error));
``` 