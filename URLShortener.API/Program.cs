using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using URLShortener.Application.Interfaces;
using URLShortener.Application.Services;
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

// Add Infrastructure Layer services
builder.Services.AddScoped<IShortUrlRepository, ShortUrlRepository>();
builder.Services.AddSingleton<IShortCodeGenerator, ShortCodeGenerator>();
builder.Services.AddSingleton<IUrlValidator, UrlValidator>();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
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
app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();

// Seed the database with some initial data (optional)
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<AppDbContext>();
    
    // Ensure the database is created
    context.Database.EnsureCreated();
}

app.Run();
