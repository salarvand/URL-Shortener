# URL Shortener Storage Optimization

This document outlines the storage optimization strategies implemented in the URL Shortener application to minimize the storage footprint while maintaining functionality.

## 1. Storage Optimization Strategies

### 1.1 Expired URL Purging

- Expired URLs are automatically purged from the database on a regular schedule
- URLs with clicks are compressed and archived before deletion
- URLs with no clicks are permanently deleted
- Purging happens daily via a background service
- Manual purging can be triggered via the API

### 1.2 Click Statistics Aggregation

- Individual click statistics are aggregated for older URLs
- Raw click data older than 30 days is summarized into aggregated records
- Aggregation includes:
  - Total click count
  - Browser distribution (summarized from user agents)
  - Geographic distribution (summarized from IP addresses)
  - Referrer distribution (summarized from referrer URLs)
- Aggregation happens weekly via a background service
- Manual aggregation can be triggered via the API

### 1.3 URL Data Compression

- URLs older than 90 days that haven't been accessed recently are compressed
- Compression uses GZip algorithm to reduce storage size
- Original URL data is stored in a compressed binary format
- Essential metadata (short code, creation date, etc.) remains uncompressed for quick access
- Compression happens monthly via a background service
- Manual compression can be triggered via the API

### 1.4 Efficient Data Structures

- Short codes are limited to 20 characters maximum
- Database indexes are optimized for common query patterns
- Foreign keys ensure referential integrity without duplicating data
- Nullable fields are used where appropriate to save space

## 2. Storage Models

### 2.1 Primary Models

- **ShortUrl**: Stores active short URLs with full data
- **ClickStatistic**: Stores individual click events with detailed information

### 2.2 Optimization Models

- **AggregatedClickStatistic**: Stores summarized click data for a time period
- **CompressedShortUrl**: Stores compressed version of old URLs

## 3. Implementation Details

### 3.1 Background Service

A background service (`StorageOptimizationBackgroundService`) runs the following tasks:
- Daily: Purge expired URLs
- Weekly: Aggregate old click statistics
- Monthly: Compress old URL data

### 3.2 Storage Optimizer

The `StorageOptimizer` service implements the optimization logic:
- `PurgeExpiredUrlsAsync()`: Removes expired URLs
- `AggregateOldClickStatisticsAsync()`: Summarizes old click data
- `CompressOldUrlDataAsync()`: Compresses old URLs
- `GetStorageStatisticsAsync()`: Provides storage usage metrics

### 3.3 API Endpoints

The following API endpoints are available for storage management:
- `GET /api/storage/stats`: Get storage usage statistics
- `POST /api/storage/purge`: Manually purge expired URLs
- `POST /api/storage/aggregate`: Manually aggregate old click statistics
- `POST /api/storage/compress`: Manually compress old URL data

## 4. Storage Efficiency Metrics

### 4.1 Compression Ratios

- URL data: ~70% reduction in size through compression
- Click statistics: ~90% reduction through aggregation

### 4.2 Storage Estimation

- Each short URL: ~200 bytes
- Each click statistic: ~250 bytes
- Each aggregated statistic: ~600 bytes (but replaces many individual records)
- Each compressed URL: ~100 bytes

### 4.3 Example Scenario

For 1 million URLs with an average of 10 clicks each:
- Without optimization: ~2.7 GB
- With optimization: ~500 MB (over 80% reduction)

## 5. Future Enhancements

- Implement tiered storage (hot/cold data separation)
- Add data retention policies configurable by administrators
- Implement batch processing for very large datasets
- Add more detailed storage analytics and monitoring
- Consider distributed storage solutions for extreme scale 