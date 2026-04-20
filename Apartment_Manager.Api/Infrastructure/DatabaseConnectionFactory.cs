using Npgsql;

namespace Apartment_Manager.Api.Infrastructure;

public class DatabaseConnectionFactory
{
    private readonly string _connectionString;

    public DatabaseConnectionFactory(IConfiguration config)
    {
        _connectionString = config.GetConnectionString("Supabase")
            ?? throw new InvalidOperationException("Connection string 'Supabase' not found.");
    }

    public DatabaseConnectionFactory(string connectionString)
    {
        _connectionString = connectionString;
    }

    public NpgsqlConnection Create() => new(_connectionString);
}
