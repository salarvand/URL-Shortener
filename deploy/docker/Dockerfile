FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy csproj files and restore as distinct layers
COPY *.sln .
COPY URLShortener.API/*.csproj ./URLShortener.API/
COPY URLShortener.Application/*.csproj ./URLShortener.Application/
COPY URLShortener.Domain/*.csproj ./URLShortener.Domain/
COPY URLShortener.Infrastructure/*.csproj ./URLShortener.Infrastructure/
COPY URLShortener.Tests/*.csproj ./URLShortener.Tests/

# Restore packages
RUN dotnet restore

# Copy everything else and build the application
COPY . .
RUN dotnet publish -c Release -o out URLShortener.API/URLShortener.API.csproj

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/out .

# Expose the port the application will run on
EXPOSE 80
EXPOSE 443

# Start the application
ENTRYPOINT ["dotnet", "URLShortener.API.dll"] 