# URL Shortener Deployment Guide

This guide provides detailed instructions for deploying the URL Shortener application using Docker or Kubernetes in different environments.

## Deployment Options

The URL Shortener application supports multiple deployment options:

1. **Docker Deployment**: Suitable for development and small-scale production
2. **Kubernetes Deployment**: Recommended for production and scalable environments
3. **Manual Deployment**: For traditional server environments

## Prerequisites

### General Requirements
- .NET SDK 8.0 or later
- SQL Server 2019 or later
- Git for source control

### Docker Deployment
- Docker Engine 20.10 or later
- Docker Compose 2.0 or later
- At least 4GB of RAM for the containers

### Kubernetes Deployment
- Kubernetes cluster 1.22 or later
- kubectl configured to communicate with your cluster
- Helm 3.0 or later
- Storage class that supports persistent volumes

## Docker Deployment

### Step 1: Clone the Repository

```bash
git clone https://github.com/yourusername/ShortenerUrl.git
cd ShortenerUrl
```

### Step 2: Configure Environment

Create a `.env` file in the project root (or modify the existing one) with the following settings:

```
ASPNETCORE_ENVIRONMENT=Production
SQL_SERVER_PASSWORD=YourStrongPassword123!
SQL_SERVER_PORT=1433
API_PORT=5000
API_HTTPS_PORT=5001
```

### Step 3: Build and Run with Docker Compose

```bash
docker-compose -f deploy/docker/docker-compose.yml up -d
```

This command will:
- Build the API container using the multi-stage Dockerfile
- Start SQL Server with persistent storage
- Configure networking between containers
- Expose the API on ports 5000/5001

### Step 4: Verify Deployment

1. Check container status:
   ```bash
   docker-compose -f deploy/docker/docker-compose.yml ps
   ```

2. View logs:
   ```bash
   docker-compose -f deploy/docker/docker-compose.yml logs -f
   ```

3. Test the API:
   ```bash
   curl http://localhost:5000/health
   ```

### Step 5: Scale Services (Optional)

To scale the API service:

```bash
docker-compose -f deploy/docker/docker-compose.yml up -d --scale urlshortener=3
```

### Stopping the Deployment

```bash
docker-compose -f deploy/docker/docker-compose.yml down
```

To remove volumes (database data will be lost):

```bash
docker-compose -f deploy/docker/docker-compose.yml down -v
```

## Kubernetes Deployment

### Step 1: Clone the Repository

```bash
git clone https://github.com/yourusername/ShortenerUrl.git
cd ShortenerUrl
```

### Step 2: Configure Kubernetes Secrets

Update the passwords and connection strings in the `mssql-secret.yaml` file:

```bash
# Create a sample secret file from the template
cp deploy/kubernetes/mssql-secret-template.yaml deploy/kubernetes/mssql-secret.yaml

# Edit the file and replace base64 encoded values for sa-password and connection-string
# You can encode values with:
echo -n 'YourStrongPassword123!' | base64
```

### Step 3: Apply Kubernetes Resources

Apply all resources using Kustomize:

```bash
kubectl apply -k deploy/kubernetes
```

### Step 4: Verify Deployment

1. Check pod status:
   ```bash
   kubectl get pods
   ```

2. Check services:
   ```bash
   kubectl get services
   ```

3. Check the ingress:
   ```bash
   kubectl get ingress
   ```

### Step 5: Configure DNS (Optional)

For a proper production setup, configure your DNS to point to the ingress IP address.

If you're using a local environment, add an entry to your hosts file:

```
127.0.0.1 urlshortener.local
```

### Step 6: Scale the Deployment (Optional)

```bash
kubectl scale deployment urlshortener-api --replicas=5
```

### Updating the Deployment

When you have new changes to deploy:

1. Build and push the new container image
2. Update the deployment:
   ```bash
   kubectl set image deployment/urlshortener-api urlshortener=urlshortener:latest
   ```

Or, if using Helm:
```bash
helm upgrade urlshortener ./deploy/helm/urlshortener
```

### Removing the Deployment

```bash
kubectl delete -k deploy/kubernetes
```

## Manual Deployment

### Step 1: Build the Application

```bash
dotnet restore
dotnet build -c Release
dotnet publish -c Release -o ./publish URLShortener.API/URLShortener.API.csproj
```

### Step 2: Configure Database

1. Create a SQL Server database
2. Run the database migrations:
   ```bash
   cd URLShortener.API
   dotnet ef database update
   ```

### Step 3: Configure Web Server

#### IIS Setup

1. Install the .NET Core Hosting Bundle
2. Create a new IIS website pointing to the publish directory
3. Configure the application pool to be No Managed Code
4. Make sure the application pool user has permissions to the directory

#### Nginx Setup (Linux)

1. Install Nginx
2. Configure a reverse proxy to the Kestrel server
3. Set up a service to run the application

Example Nginx configuration:

```nginx
server {
    listen 80;
    server_name your-domain.com;

    location / {
        proxy_pass http://localhost:5000;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection keep-alive;
        proxy_set_header Host $host;
        proxy_cache_bypass $http_upgrade;
    }
}
```

### Step 4: Configure SSL

Configure HTTPS for production environments using:
- IIS: Request and install a certificate
- Nginx: Use Let's Encrypt and Certbot

### Step 5: Start the Application

For standalone hosting:

```bash
cd publish
dotnet URLShortener.API.dll
```

## Troubleshooting

### Common Issues

#### Database Connection Failures

1. Verify SQL Server is running
2. Check connection string in configuration
3. Ensure firewall allows SQL Server connections
4. For Docker or Kubernetes, check network policies

#### API Not Responding

1. Check if service is running
2. Verify ports are correctly mapped
3. Check logs for exceptions
4. Verify resource constraints (CPU/Memory)

#### SSL Certificate Issues

1. Ensure certificates are properly installed
2. Check certificate expiration dates
3. Verify hostname matches certificate

### Viewing Logs

#### Docker Logs

```bash
docker-compose -f deploy/docker/docker-compose.yml logs -f
```

#### Kubernetes Logs

```bash
kubectl logs -l app=urlshortener
```

#### Application Logs

In the deployed application directory:
```
logs/application-{date}.log
```

## Maintenance

### Backup and Restore

#### Database Backup

```bash
# Docker environment
docker exec -it urlshortener-db /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P YourPassword -Q "BACKUP DATABASE URLShortenerDb TO DISK = N'/var/opt/mssql/data/URLShortenerDb.bak' WITH NOFORMAT, NOINIT, SKIP, NOREWIND, NOUNLOAD, STATS = 10"

# Direct SQL Server
sqlcmd -S localhost -U sa -P YourPassword -Q "BACKUP DATABASE URLShortenerDb TO DISK = N'C:\Backups\URLShortenerDb.bak' WITH NOFORMAT, NOINIT, SKIP, NOREWIND, NOUNLOAD, STATS = 10"
```

#### Database Restore

```bash
# Docker environment
docker cp URLShortenerDb.bak urlshortener-db:/var/opt/mssql/data/
docker exec -it urlshortener-db /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P YourPassword -Q "RESTORE DATABASE URLShortenerDb FROM DISK = N'/var/opt/mssql/data/URLShortenerDb.bak' WITH REPLACE"

# Direct SQL Server
sqlcmd -S localhost -U sa -P YourPassword -Q "RESTORE DATABASE URLShortenerDb FROM DISK = N'C:\Backups\URLShortenerDb.bak' WITH REPLACE"
```

### Database Migrations

When updating the database schema:

```bash
cd URLShortener.API
dotnet ef database update
```

In Docker:

```bash
docker exec -it urlshortener-api /bin/bash
cd /app
dotnet ef database update
```

## Security Considerations

### Production Checklist

1. ✅ Use HTTPS for all communication
2. ✅ Store secrets securely (Kubernetes secrets, environment variables)
3. ✅ Use strong SQL Server passwords
4. ✅ Implement rate limiting for API endpoints
5. ✅ Set up proper network security policies
6. ✅ Regularly update base images and dependencies
7. ✅ Use non-root users in containers
8. ✅ Configure proper security headers

### Certificate Management

Ensure SSL certificates are renewed before expiration:
- For Let's Encrypt, configure auto-renewal
- Set up monitoring for certificate expiration dates

## Monitoring

### Health Checks

The application provides a health endpoint at `/health` that returns the status of:
- API availability
- Database connectivity
- External dependencies

### Performance Monitoring

Consider setting up:
- Prometheus for metrics collection
- Grafana for visualization
- Application Insights for application performance monitoring

## Support

If you encounter issues during deployment, please:
1. Check the troubleshooting section above
2. Review the application logs
3. Contact support at support@yourdomain.com
4. File an issue on the GitHub repository 