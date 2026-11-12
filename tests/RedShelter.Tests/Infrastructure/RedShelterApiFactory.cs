using System.Data.Common;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using RedShelter.Api.Infrastructure.Persistence;

namespace RedShelter.Tests.Infrastructure;

public class RedShelterApiFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            // Quitar la configuracion se sql server
            services.RemoveAll<RedShelterDbContext>();
            services.RemoveAll<DbContextOptions<RedShelterDbContext>>();
            services.RemoveAll<IDbContextOptionsConfiguration<RedShelterDbContext>>();

            // Configuracion de sqlito en memoria
            services.AddSingleton<DbConnection>(_ =>
            {
                var connection = new SqliteConnection("Data Source=:memory:");
                connection.Open();
                return connection;
            });

            services.AddDbContext<RedShelterDbContext>(
                (provider, options) =>
                {
                    var connection = provider
                        .GetRequiredService<DbConnection>();

                    options.UseSqlite(connection);
                });
        });
    }

    public async Task<HttpClient> CreateTestClientAsync()
    {
        var client = CreateClient();

        using var scope = Services.CreateScope();

        var context = scope.ServiceProvider
            .GetRequiredService<RedShelterDbContext>();

        await context.Database.EnsureCreatedAsync();

        return client;
    }
}