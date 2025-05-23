# URL Shortener Database Schema

This document provides a detailed description of the database schema for the URL Shortener application.

## Overview

The URL Shortener database uses Microsoft SQL Server and follows a relational model. The schema is designed to efficiently store shortened URLs and their click statistics while supporting features like expiration dates and storage optimization.

## Entity Relationship Diagram

```
┌─────────────────────┐       ┌──────────────────────┐
│                     │       │                      │
│     ShortUrls       │       │   ClickStatistics    │
│                     │       │                      │
├─────────────────────┤       ├──────────────────────┤
│ Id (PK)             │       │ Id (PK)              │
│ OriginalUrl         │       │ ShortUrlId (FK)      │
│ ShortCode           │       │ ClickedAt           │
│ CreatedAt           │       │ UserAgent            │
│ ExpiresAt           │       │ IpAddress            │
│ ClickCount          │       │ RefererUrl           │
│ IsActive            │       │                      │
└────────┬────────────┘       └──────────┬───────────┘
         │                               │
         │ 1                         0..* │
         └───────────────────────────────┘
               
                  ┌─────────────────────────┐
                  │                         │
                  │ AggregatedClickStats    │
                  │                         │
                  ├─────────────────────────┤
                  │ Id (PK)                 │
                  │ ShortUrlId (FK)         │
                  │ DateGroup               │
                  │ ClickCount              │
                  │ StartDate               │
                  │ EndDate                 │
                  └────────┬────────────────┘
                           │
                           │
                  ┌────────┴─────────────────┐
                  │                          │
                  │ CompressedShortUrls      │
                  │                          │
                  ├──────────────────────────┤
                  │ Id (PK)                  │
                  │ OriginalShortUrlId       │
                  │ OriginalUrl              │
                  │ ShortCode                │
                  │ CreatedAt                │
                  │ ExpiresAt                │
                  │ TotalClicks              │
                  │ CompressedAt             │
                  └──────────────────────────┘
```

## Tables

### ShortUrls

Stores the main information about shortened URLs.

| Column       | Type             | Constraints         | Description                                  |
|--------------|------------------|---------------------|----------------------------------------------|
| Id           | UNIQUEIDENTIFIER | PK, NOT NULL        | Unique identifier for the short URL          |
| OriginalUrl  | NVARCHAR(2000)   | NOT NULL            | The original long URL                        |
| ShortCode    | NVARCHAR(20)     | UNIQUE, NOT NULL    | The unique short code for the URL            |
| CreatedAt    | DATETIME2        | NOT NULL            | When the short URL was created               |
| ExpiresAt    | DATETIME2        | NULL                | When the short URL expires (optional)        |
| ClickCount   | INT              | NOT NULL, DEFAULT 0 | Number of times the URL has been clicked     |
| IsActive     | BIT              | NOT NULL, DEFAULT 1 | Whether the short URL is active              |

#### Indexes
- Primary Key: `PK_ShortUrls` on `Id`
- Unique Index: `IX_ShortUrls_ShortCode` on `ShortCode`
- Index: `IX_ShortUrls_CreatedAt` on `CreatedAt`
- Index: `IX_ShortUrls_ExpiresAt` on `ExpiresAt` (filtered for non-null values)

### ClickStatistics

Stores detailed information about each click on a shortened URL.

| Column       | Type             | Constraints        | Description                               |
|--------------|------------------|-------------------|-------------------------------------------|
| Id           | UNIQUEIDENTIFIER | PK, NOT NULL      | Unique identifier for the click record    |
| ShortUrlId   | UNIQUEIDENTIFIER | FK, NOT NULL      | Reference to the ShortUrl                 |
| ClickedAt    | DATETIME2        | NOT NULL          | When the click occurred                   |
| UserAgent    | NVARCHAR(500)    | NULL              | Browser/device information                |
| IpAddress    | NVARCHAR(50)     | NULL              | IP address (may be anonymized)            |
| RefererUrl   | NVARCHAR(2000)   | NULL              | Where the click came from                 |

#### Indexes
- Primary Key: `PK_ClickStatistics` on `Id`
- Foreign Key: `FK_ClickStatistics_ShortUrls` on `ShortUrlId` referencing `ShortUrls(Id)`
- Index: `IX_ClickStatistics_ShortUrlId` on `ShortUrlId`
- Index: `IX_ClickStatistics_ClickedAt` on `ClickedAt`

### AggregatedClickStats

Stores aggregated click statistics for storage optimization.

| Column       | Type             | Constraints        | Description                                |
|--------------|------------------|-------------------|--------------------------------------------|
| Id           | UNIQUEIDENTIFIER | PK, NOT NULL      | Unique identifier for the aggregated stats |
| ShortUrlId   | UNIQUEIDENTIFIER | FK, NOT NULL      | Reference to the ShortUrl                  |
| DateGroup    | NVARCHAR(20)     | NOT NULL          | Grouping period (e.g., 'day', 'week')      |
| ClickCount   | INT              | NOT NULL          | Total number of clicks in this period      |
| StartDate    | DATETIME2        | NOT NULL          | Start of the aggregation period            |
| EndDate      | DATETIME2        | NOT NULL          | End of the aggregation period              |

#### Indexes
- Primary Key: `PK_AggregatedClickStats` on `Id`
- Foreign Key: `FK_AggregatedClickStats_ShortUrls` on `ShortUrlId` referencing `ShortUrls(Id)`
- Index: `IX_AggregatedClickStats_ShortUrlId_DateGroup` on `ShortUrlId, DateGroup`

### CompressedShortUrls

Stores compressed information about expired or inactive short URLs.

| Column             | Type             | Constraints        | Description                                   |
|--------------------|------------------|-------------------|-----------------------------------------------|
| Id                 | UNIQUEIDENTIFIER | PK, NOT NULL      | Unique identifier for the compressed record    |
| OriginalShortUrlId | UNIQUEIDENTIFIER | NOT NULL          | The ID of the original ShortUrl                |
| OriginalUrl        | NVARCHAR(2000)   | NOT NULL          | The original long URL                          |
| ShortCode          | NVARCHAR(20)     | NOT NULL          | The short code                                 |
| CreatedAt          | DATETIME2        | NOT NULL          | When the short URL was created                 |
| ExpiresAt          | DATETIME2        | NULL              | When the short URL expired                     |
| TotalClicks        | INT              | NOT NULL          | Total number of clicks the URL received        |
| CompressedAt       | DATETIME2        | NOT NULL          | When the record was compressed                 |

#### Indexes
- Primary Key: `PK_CompressedShortUrls` on `Id`
- Index: `IX_CompressedShortUrls_OriginalShortUrlId` on `OriginalShortUrlId`
- Index: `IX_CompressedShortUrls_ShortCode` on `ShortCode`

## Database Functions and Stored Procedures

### Functions

#### `fn_GetActiveShortUrl`

Returns a ShortUrl if it exists and is active.

```sql
CREATE FUNCTION fn_GetActiveShortUrl (@shortCode NVARCHAR(20))
RETURNS TABLE
AS
RETURN
(
    SELECT *
    FROM ShortUrls
    WHERE ShortCode = @shortCode
      AND IsActive = 1
      AND (ExpiresAt IS NULL OR ExpiresAt > GETUTCDATE())
);
```

### Stored Procedures

#### `sp_AggregateClickStatistics`

Aggregates click statistics older than a specified date.

```sql
CREATE PROCEDURE sp_AggregateClickStatistics
    @olderThanDays INT = 30
AS
BEGIN
    DECLARE @cutoffDate DATETIME2 = DATEADD(DAY, -@olderThanDays, GETUTCDATE());
    
    -- Insert aggregated data
    INSERT INTO AggregatedClickStats (Id, ShortUrlId, DateGroup, ClickCount, StartDate, EndDate)
    SELECT 
        NEWID() AS Id,
        ShortUrlId,
        'day' AS DateGroup,
        COUNT(*) AS ClickCount,
        CAST(CONVERT(VARCHAR(10), ClickedAt, 120) AS DATETIME2) AS StartDate,
        DATEADD(DAY, 1, CAST(CONVERT(VARCHAR(10), ClickedAt, 120) AS DATETIME2)) AS EndDate
    FROM ClickStatistics
    WHERE ClickedAt < @cutoffDate
    GROUP BY ShortUrlId, CAST(CONVERT(VARCHAR(10), ClickedAt, 120) AS DATETIME2);
    
    -- Delete the original detailed records
    DELETE FROM ClickStatistics
    WHERE ClickedAt < @cutoffDate;
    
    -- Return count of aggregated records
    SELECT @@ROWCOUNT AS AggregatedCount;
END;
```

#### `sp_CompressOldUrls`

Compresses old short URLs by moving them to the CompressedShortUrls table.

```sql
CREATE PROCEDURE sp_CompressOldUrls
    @olderThanDays INT = 90,
    @onlyExpired BIT = 0
AS
BEGIN
    DECLARE @cutoffDate DATETIME2 = DATEADD(DAY, -@olderThanDays, GETUTCDATE());
    
    -- Insert into compressed table
    INSERT INTO CompressedShortUrls (Id, OriginalShortUrlId, OriginalUrl, ShortCode, 
                                    CreatedAt, ExpiresAt, TotalClicks, CompressedAt)
    SELECT 
        NEWID() AS Id,
        Id AS OriginalShortUrlId,
        OriginalUrl,
        ShortCode,
        CreatedAt,
        ExpiresAt,
        ClickCount AS TotalClicks,
        GETUTCDATE() AS CompressedAt
    FROM ShortUrls
    WHERE CreatedAt < @cutoffDate
      AND (@onlyExpired = 0 OR (ExpiresAt IS NOT NULL AND ExpiresAt < GETUTCDATE()));
    
    -- Delete from ClickStatistics
    DELETE FROM ClickStatistics
    WHERE ShortUrlId IN (
        SELECT Id
        FROM ShortUrls
        WHERE CreatedAt < @cutoffDate
          AND (@onlyExpired = 0 OR (ExpiresAt IS NOT NULL AND ExpiresAt < GETUTCDATE()))
    );
    
    -- Delete from ShortUrls
    DELETE FROM ShortUrls
    WHERE CreatedAt < @cutoffDate
      AND (@onlyExpired = 0 OR (ExpiresAt IS NOT NULL AND ExpiresAt < GETUTCDATE()));
    
    -- Return count of compressed records
    SELECT @@ROWCOUNT AS CompressedCount;
END;
```

## Database Migrations

The database schema is managed through Entity Framework Core migrations. The migration history is stored in the `__EFMigrationsHistory` table.

## Query Examples

### Get Active Short URL by Code

```sql
SELECT * 
FROM ShortUrls
WHERE ShortCode = 'abc123'
  AND IsActive = 1
  AND (ExpiresAt IS NULL OR ExpiresAt > GETUTCDATE());
```

### Get Click Statistics for a Short URL

```sql
SELECT 
    s.ShortCode,
    COUNT(c.Id) AS TotalClicks,
    MIN(c.ClickedAt) AS FirstClick,
    MAX(c.ClickedAt) AS LastClick
FROM ShortUrls s
LEFT JOIN ClickStatistics c ON s.Id = c.ShortUrlId
WHERE s.Id = '3fa85f64-5717-4562-b3fc-2c963f66afa6'
GROUP BY s.ShortCode;
```

### Get Clicks by Day for a Short URL

```sql
SELECT 
    CONVERT(DATE, c.ClickedAt) AS ClickDate,
    COUNT(*) AS ClickCount
FROM ClickStatistics c
WHERE c.ShortUrlId = '3fa85f64-5717-4562-b3fc-2c963f66afa6'
GROUP BY CONVERT(DATE, c.ClickedAt)
ORDER BY ClickDate;
```

## Performance Considerations

### Table Partitioning

For high-volume deployments, the ClickStatistics table can be partitioned by date to improve query performance:

```sql
-- Example of table partitioning by month
CREATE PARTITION FUNCTION MonthlyPartitionFunction (DATETIME2)
AS RANGE RIGHT FOR VALUES (
    '2023-01-01', '2023-02-01', '2023-03-01', /* and so on */
);

CREATE PARTITION SCHEME MonthlyPartitionScheme
AS PARTITION MonthlyPartitionFunction
ALL TO ([PRIMARY]);

CREATE TABLE ClickStatistics (
    -- columns as defined above
)
ON MonthlyPartitionScheme(ClickedAt);
```

### Indexing Strategy

The indexing strategy focuses on common access patterns:
- Looking up short URLs by their code (most frequent operation)
- Finding click statistics for a specific short URL
- Finding expired URLs for cleanup
- Aggregating statistics by date ranges

## Data Retention Policy

The database supports automatic data retention through scheduled jobs:
- Click statistics older than 30 days are aggregated by default
- Short URLs that have been inactive for 90 days can be compressed
- Expired URLs are automatically compressed after their expiration

## Backup and Recovery Strategy

It is recommended to:
- Perform daily full backups of the database
- Set up transaction log backups every 15-30 minutes for point-in-time recovery
- Test database restoration regularly
- Configure automated backup verification 