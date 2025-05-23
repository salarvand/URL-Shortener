using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Linq;
using URLShortener.Application.Interfaces;
using URLShortener.Application.Services;
using URLShortener.Domain;
using URLShortener.Infrastructure.Data;
using URLShortener.Infrastructure.Repositories;
using URLShortener.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Add API Layer services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add Database services using in-memory database
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseInMemoryDatabase("URLShortenerDb"));

// Add Application Layer services
builder.Services.AddScoped<IShortUrlService, ShortUrlService>();
builder.Services.AddScoped<IValidationService, ValidationService>();

// Add Infrastructure Layer services
builder.Services.AddScoped<IShortUrlRepository, ShortUrlRepository>();
// Use the new efficient Base62 algorithm for generating short codes
builder.Services.AddSingleton<IShortCodeGenerator, Base62ShortCodeGenerator>();
builder.Services.AddSingleton<IUrlValidator, UrlValidator>();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(
        policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

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
