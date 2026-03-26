using CoreBackendApp.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using Xunit;

namespace CoreBackendApp.IntegrationTests;

public abstract class BaseIntegrationTest : IClassFixture<WebApplicationFactory<Api.Program>>
{
    protected readonly WebApplicationFactory<Api.Program> Factory;
    protected readonly HttpClient Client;

    protected BaseIntegrationTest(WebApplicationFactory<Api.Program> factory)
    {
        Factory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");

            builder.ConfigureAppConfiguration((context, config) =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Jwt:Key"] = "SuperSecretTestKeyThatIsVeryLong123!",
                    ["Jwt:Issuer"] = "CoreBackendApp",
                    ["Jwt:Audience"] = "CoreBackendApp",
                    ["Jwt:AccessTokenExpirationMinutes"] = "60"
                });
            });
        });

        // Ensure database is clean and seeded
        using (var scope = Factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<CoreDbContext>();
            db.Database.EnsureDeleted();
            db.Database.EnsureCreated();
            CoreDbSeeder.SeedAsync(db).GetAwaiter().GetResult();
        }

        Client = Factory.CreateClient();
    }
}
