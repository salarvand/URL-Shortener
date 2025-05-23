using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MediatR;
using System;
using System.Linq;
using System.Reflection;
using URLShortener.API.Middleware;
using URLShortener.Application.Interfaces;
using URLShortener.Application.Services;
using URLShortener.Domain;
using URLShortener.Infrastructure.Data;
using URLShortener.Infrastructure.Repositories;
using URLShortener.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Add MediatR
builder.Services.AddMediatR(typeof(ShortUrlService).Assembly);

// Add API Layer services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add Database services using in-memory database
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseInMemoryDatabase("URLShortenerDb"));

// Add Application Layer services
builder.Services.AddScoped<IShortUrlService, ShortUrlService>();
builder.Services.AddScoped<IValidationService, ValidationService>();
// ValidationService will be registered by FluentValidation

// Add Infrastructure Layer services
builder.Services.AddScoped<IShortUrlRepository, ShortUrlRepository>();
// Use the new efficient Base62 algorithm for generating short codes
builder.Services.AddSingleton<IShortCodeGenerator, Base62ShortCodeGenerator>();
builder.Services.AddSingleton<IUrlValidator, UrlValidator>();

// Add security services
builder.Services.AddSingleton<IUrlScanningService, UrlScanningService>();
builder.Services.AddMemoryCache();
builder.Services.AddSingleton<IRateLimiter, InMemoryRateLimiter>();

// Add CORS with a more restrictive policy
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(
        policy =>
        {
            policy.WithOrigins("https://localhost:5001", "http://localhost:5000")
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});

// Add HTTPS redirection
builder.Services.AddHttpsRedirection(options =>
{
    options.HttpsPort = 5001;
    options.RedirectStatusCode = StatusCodes.Status307TemporaryRedirect;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    // In production, add HSTS
    app.UseHsts();
}

// Add security headers to all responses
app.UseMiddleware<SecurityHeadersMiddleware>();

app.UseHttpsRedirection();

// Add security middleware
app.UseMiddleware<UrlSecurityMiddleware>();
app.UseMiddleware<RateLimitingMiddleware>();

app.UseCors();

app.UseAuthorization();

app.MapControllers();

// Seed the database with some initial data
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<AppDbContext>();
    
    // Ensure the database is created
    context.Database.EnsureCreated();
    
    // Add sample data if none exists
    if (!context.ShortUrls.Any())
    {
        var now = DateTime.UtcNow;
        
        var samples = new[]
        {
            ShortUrl.Reconstitute(
                Guid.Parse("00000000-0000-0000-0000-000000000001"),
                "https://www.google.com",
                "google",
                now,
                5,
                true
            ),
            ShortUrl.Reconstitute(
                Guid.Parse("00000000-0000-0000-0000-000000000002"),
                "https://www.github.com",
                "github",
                now,
                3,
                true
            ),
            ShortUrl.Reconstitute(
                Guid.Parse("00000000-0000-0000-0000-000000000003"),
                "https://www.microsoft.com",
                "msft",
                now,
                1,
                true,
                now.AddDays(30)
            )
        };
        
        foreach (var sample in samples)
        {
            context.ShortUrls.Add(sample);
        }
        
        context.SaveChanges();
    }
}

app.Run();
