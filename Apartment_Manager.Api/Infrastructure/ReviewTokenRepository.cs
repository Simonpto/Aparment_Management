using Dapper;

namespace Apartment_Manager.Api.Infrastructure;

public class ReviewTokenRepository(DatabaseConnectionFactory db) : IReviewTokenRepository
{
    public async Task<Guid> CreateAsync(Guid apartmentId, TimeSpan expiresIn)
    {
        using var conn = db.Create();
        await conn.OpenAsync();
        return await conn.ExecuteScalarAsync<Guid>(
            """
            INSERT INTO review_tokens (apartment_id, expires_at)
            VALUES (@apartmentId, @expiresAt)
            RETURNING token
            """,
            new { apartmentId, expiresAt = DateTime.UtcNow.Add(expiresIn) });
    }

    public async Task<TokenData?> GetByTokenAsync(Guid token)
    {
        using var conn = db.Create();
        await conn.OpenAsync();
        return await conn.QueryFirstOrDefaultAsync<TokenData>(
            "SELECT id, apartment_id AS ApartmentId, token, expires_at AS ExpiresAt, used FROM review_tokens WHERE token = @token",
            new { token });
    }

    public async Task MarkUsedAsync(Guid id)
    {
        using var conn = db.Create();
        await conn.OpenAsync();
        await conn.ExecuteAsync("UPDATE review_tokens SET used = true WHERE id = @id", new { id });
    }
}
