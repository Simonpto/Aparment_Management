using Apartment_Manager.Api.Infrastructure;
using Dapper;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Testcontainers.PostgreSql;

namespace Apartment_Manager.Tests.Integration;

public class ApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _db = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine")
        .Build();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration(config =>
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Supabase"] = _db.GetConnectionString(),
                ["Admin:Password"] = "test-admin-password",
                ["BlazorOrigin"] = "https://localhost"
            }));

        builder.ConfigureServices(services =>
        {
            // Replace the singleton factory with one pointing at the test container
            var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DatabaseConnectionFactory));
            if (descriptor != null) services.Remove(descriptor);
            services.AddSingleton(_ => new DatabaseConnectionFactory(
                new ConfigurationBuilder()
                    .AddInMemoryCollection(new Dictionary<string, string?> { ["ConnectionStrings:Supabase"] = _db.GetConnectionString() })
                    .Build()));
        });
    }

    public async Task InitializeAsync()
    {
        await _db.StartAsync();
        DapperConfig.RegisterTypeHandlers();
        await RunMigrationAsync();
    }

    public new async Task DisposeAsync() => await _db.StopAsync();

    public async Task ResetAsync()
    {
        using var conn = new NpgsqlConnection(_db.GetConnectionString());
        await conn.OpenAsync();
        await conn.ExecuteAsync(
            "TRUNCATE apartments, blog_posts, reviews, review_tokens CASCADE");
    }

    public string GetConnectionString() => _db.GetConnectionString();

    private async Task RunMigrationAsync()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir != null && !File.Exists(Path.Combine(dir.FullName, "Apartment_Manager.sln")))
            dir = dir.Parent;

        var sql = await File.ReadAllTextAsync(
            Path.Combine(dir!.FullName, "supabase", "migrations", "001_initial_schema.sql"));

        using var conn = new NpgsqlConnection(_db.GetConnectionString());
        await conn.OpenAsync();
        await conn.ExecuteAsync(sql);
    }
}
