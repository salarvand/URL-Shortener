using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using URLShortener.Domain;
using URLShortener.Infrastructure.Data;

namespace URLShortener.Tests.Common
{
    public abstract class TestBase
    {
        protected IServiceProvider ServiceProvider { get; }

        protected TestBase()
        {
            var services = new ServiceCollection();
            ConfigureServices(services);
            ServiceProvider = services.BuildServiceProvider();
        }

        protected virtual void ConfigureServices(IServiceCollection services)
        {
            // Configure in-memory database
            services.AddDbContext<AppDbContext>(options =>
                options.UseInMemoryDatabase(Guid.NewGuid().ToString()));
        }

        protected AppDbContext GetDbContext()
        {
            return ServiceProvider.GetRequiredService<AppDbContext>();
        }

        protected void AddEntity<T>(T entity) where T : Entity
        {
            var context = GetDbContext();
            context.Add(entity);
            context.SaveChanges();
        }

        protected void AddEntities<T>(IEnumerable<T> entities) where T : Entity
        {
            var context = GetDbContext();
            context.AddRange(entities);
            context.SaveChanges();
        }
    }
} 