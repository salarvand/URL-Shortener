# URL Shortener User Guide

Welcome to the URL Shortener service! This guide will help you understand how to use the system to create and manage shortened URLs.

## Getting Started

The URL Shortener service allows you to create shorter, more manageable links from long URLs. These shortened links are easier to share, remember, and track.

### Accessing the Service

The URL Shortener service can be accessed in several ways:
- Via the web interface at `https://[your-domain]`
- Via the REST API at `https://[your-domain]/api`
- Via direct URL redirection at `https://[your-domain]/s/{shortCode}`

## Creating Shortened URLs

### Using the Web Interface

1. Navigate to the home page of the URL Shortener service.
2. Enter the long URL you wish to shorten in the input field.
3. (Optional) Enter a custom short code if you want a personalized link.
4. (Optional) Set an expiration date for the shortened URL.
5. Click the "Shorten" button.
6. Your new shortened URL will be displayed and ready to use.

### Using the API

To create a shortened URL programmatically, make a POST request to the API:

```
POST /api/ShortUrl
Content-Type: application/json

{
  "originalUrl": "https://example.com/very/long/url/that/needs/shortening",
  "customShortCode": "custom123",  // Optional
  "expiresAt": "2023-12-31T23:59:59Z"  // Optional
}
```

See the [API Reference](api-reference.md) for more details.

## Using Shortened URLs

Once you have created a shortened URL, you can:

1. Share it via email, social media, messaging apps, or any other medium.
2. Use it in printed materials where shorter URLs are more practical.
3. Track how many times it has been accessed.

To use a shortened URL, simply enter `https://[your-domain]/s/{shortCode}` in a web browser, or click on the link if it's hyperlinked.

## Managing Your URLs

### Viewing URL Details

1. Navigate to the "My URLs" section of the web interface.
2. Here you can see a list of all your shortened URLs.
3. Click on any URL to see detailed information, including:
   - The original long URL
   - Creation date
   - Expiration date (if set)
   - Number of clicks
   - Click statistics

### API Access for URL Management

You can also manage your URLs programmatically via the API:
- Get information about a specific short URL
- Get a list of all your short URLs
- Track click statistics

See the [API Reference](api-reference.md) for more details.

## URL Expiration

URLs can be set to expire after a certain date. After expiration:
- The URL will no longer redirect to the original destination
- It will return a "Not Found" error when accessed
- It will be marked as "Expired" in the URL management interface

To set an expiration date:
1. When creating a new shortened URL, set the optional "Expires At" field.
2. The date must be in the future.

## Click Tracking and Statistics

The URL Shortener service provides basic analytics for each shortened URL:

### Click Count

The total number of times your shortened URL has been accessed.

### Click Details (if enabled)

For each click, the system can record:
- Date and time
- User agent (browser information)
- IP address (anonymized for privacy)
- Referrer URL (where the click came from)

### Viewing Statistics

1. Navigate to the "My URLs" section.
2. Click on a specific URL.
3. View the "Statistics" tab to see click data.

## Best Practices

### Creating Effective Short URLs

- Keep custom short codes brief and memorable.
- Use meaningful custom codes that hint at the content.
- Consider using expiration dates for time-sensitive content.

### Security Considerations

- Do not use shortened URLs to hide malicious content.
- Be aware that shortened URLs obscure the destination, which may cause recipients to be cautious.
- Consider using URL preview features when available.

### Privacy Considerations

- The service stores click statistics, which may include IP addresses.
- Refer to our Privacy Policy for details on data handling.

## Troubleshooting

### Common Issues

#### "URL Not Found" Error

If you encounter a "URL Not Found" error when using a shortened URL, it may be because:
- The URL has expired
- The short code was mistyped
- The URL was deleted by its creator

#### Invalid URL Error When Creating

If you receive an "Invalid URL" error when trying to create a shortened URL:
- Ensure the URL includes the protocol (http:// or https://)
- Check for typos in the URL
- Verify that the URL is publicly accessible

#### Custom Code Already Taken

If your preferred custom code is already in use:
- Try a different custom code
- Add numbers or variations to make it unique
- Use an auto-generated code instead

### Getting Help

If you encounter issues not covered in this guide:
- Check the FAQ section on the website
- Contact support at support@[your-domain]
- Refer to the API documentation for programmatic issues

## Limitations

The URL Shortener service has the following limitations:

- Maximum original URL length: 2,000 characters
- Maximum custom short code length: 20 characters
- Rate limiting: 100 requests per hour per IP address
- Storage limits: 10,000 URLs per account

## Glossary

- **Short URL**: A condensed URL that redirects to a longer URL
- **Short Code**: The unique identifier in the shortened URL path
- **Original URL**: The full, original URL that is being shortened
- **Expiration Date**: The date after which the shortened URL will no longer work
- **Click Tracking**: Recording statistics about when and how a shortened URL is used 