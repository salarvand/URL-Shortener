# URLShortener Deployment Guide

This guide explains how to deploy the URLShortener application using Docker or Kubernetes.

## Docker Deployment

### Prerequisites
- Docker installed on your system
- Docker Compose installed on your system

### Deployment Steps

1. Navigate to the project root directory:
   ```
   cd /path/to/ShortenerUrl
   ```

2. Build and run the containers:
   ```
   docker-compose -f deploy/docker/docker-compose.yml up -d
   ```

3. The application will be available at:
   - http://localhost:5000
   - https://localhost:5001 (for HTTPS)

### Stopping the Application
```
docker-compose -f deploy/docker/docker-compose.yml down
```

To remove volumes as well:
```
docker-compose -f deploy/docker/docker-compose.yml down -v
```

## Kubernetes Deployment

### Prerequisites
- Kubernetes cluster up and running
- kubectl configured to communicate with your cluster
- kustomize installed (comes with recent kubectl versions)

### Deployment Steps

1. Navigate to the kubernetes directory:
   ```
   cd /path/to/ShortenerUrl/deploy/kubernetes
   ```

2. Apply all resources using kustomize:
   ```
   kubectl apply -k .
   ```

3. Check the status of deployments:
   ```
   kubectl get deployments
   kubectl get pods
   kubectl get services
   kubectl get ingress
   ```

4. The application will be available at the ingress address. Add the following entry to your hosts file:
   ```
   127.0.0.1 urlshortener.local
   ```

   Then access http://urlshortener.local

### Removing the Application
```
kubectl delete -k .
```

## Configuration

### Database Connection
- The Docker deployment uses a SQL Server container.
- The Kubernetes deployment uses secrets for SQL Server credentials.

### Environment Variables
Both deployments use the following environment variables:
- `ASPNETCORE_ENVIRONMENT`: Set to `Production`.
- `ConnectionStrings__DefaultConnection`: The database connection string.

## Scaling

### Docker Compose
Modify the `docker-compose.yml` file to change the number of replicas:
```yaml
urlshortener:
  deploy:
    replicas: 3
```

### Kubernetes
```
kubectl scale deployment urlshortener-api --replicas=3
```

## Troubleshooting

### Docker
- Check logs: `docker-compose -f deploy/docker/docker-compose.yml logs`
- Access container: `docker exec -it urlshortener-api /bin/bash`

### Kubernetes
- Check pod logs: `kubectl logs -l app=urlshortener`
- Describe resources: `kubectl describe pod <pod-name>` 