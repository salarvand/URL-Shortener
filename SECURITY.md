# URL Shortener Security Measures

This document outlines the security measures implemented in the URL Shortener application to protect both the system and its users.

## 1. URL Validation and Scanning

### 1.1 URL Format Validation
- All URLs are validated for proper format using both domain validation and FluentValidation
- URLs must be valid absolute URLs with HTTP or HTTPS schemes
- Implemented at multiple layers (domain, application, and API) for defense in depth

### 1.2 Malicious URL Detection
- URLs are scanned for potentially malicious patterns using regex-based detection
- Known malicious domains are blocked using a blocklist approach
- Suspicious patterns like executable file extensions and common phishing keywords are detected
- The system is designed to be extensible to integrate with external URL scanning services like Google Safe Browsing API or VirusTotal

## 2. Rate Limiting

### 2.1 IP-Based Rate Limiting
- Prevents abuse of the API by limiting requests per IP address
- Different limits for different endpoints based on sensitivity:
  - URL creation: 10 requests per minute
  - URL redirection: 60 requests per minute
  - Other endpoints: 30 requests per minute
- Configurable time windows and request limits
- Returns 429 Too Many Requests status code when limits are exceeded

## 3. Security Headers

### 3.1 HTTP Security Headers
- Content-Security-Policy (CSP): Restricts resource loading to trusted sources
- X-Content-Type-Options: Prevents MIME-sniffing attacks
- X-Frame-Options: Prevents clickjacking attacks
- X-XSS-Protection: Enables browser's XSS protection
- Strict-Transport-Security (HSTS): Enforces HTTPS connections
- Referrer-Policy: Controls referrer information leakage
- X-Download-Options: Prevents file download exploits
- X-Permitted-Cross-Domain-Policies: Restricts cross-domain policies

## 4. HTTPS Enforcement

- All HTTP requests are redirected to HTTPS
- HSTS is enabled in production to enforce secure connections
- Secure cookies are used when applicable

## 5. CORS Policy

- Cross-Origin Resource Sharing (CORS) is configured with a restrictive policy
- Only specified origins are allowed to access the API
- Prevents cross-site request forgery (CSRF) attacks

## 6. Input Validation

- All user inputs are validated using FluentValidation
- Custom short codes are validated against a strict pattern
- Expiration dates are validated to be in the future
- Validation is performed at both the API and domain layers

## 7. Error Handling

- Detailed error messages are only shown in development
- Generic error messages are returned in production
- Prevents information leakage through error messages
- All errors are logged for monitoring and analysis

## 8. Secure Redirect Implementation

- Redirects are implemented securely to prevent open redirect vulnerabilities
- URL validation occurs before any redirect happens
- Tracking information is collected securely

## 9. Monitoring and Logging

- Security events are logged for monitoring
- Rate limit violations are logged
- Malicious URL detections are logged
- IP addresses of requests are logged for security analysis

## 10. Future Enhancements

- Integration with external URL scanning services
- User authentication and authorization for URL management
- Captcha for URL creation to prevent automated abuse
- Enhanced analytics and threat detection
- Regular security audits and penetration testing 