using Apartment_Manager.Shared.DTOs;
using Dapper;

namespace Apartment_Manager.Api.Infrastructure;

public class ReviewRepository(DatabaseConnectionFactory db) : IReviewRepository
{
    public async Task<List<ReviewDto>> GetApprovedByApartmentAsync(Guid apartmentId)
    {
        using var conn = db.Create();
        await conn.OpenAsync();
        var rows = await conn.QueryAsync<ReviewRow>(
            "SELECT id, apartment_id AS ApartmentId, rating, text, guest_name AS GuestName, created_at AS CreatedAt FROM reviews WHERE apartment_id = @apartmentId AND approved = true ORDER BY created_at DESC",
            new { apartmentId });
        return rows.Select(r => new ReviewDto(r.Id, r.ApartmentId, r.Rating, r.Text, r.GuestName, r.CreatedAt)).ToList();
    }

    public async Task<List<AdminReviewDto>> GetAllAsync()
    {
        using var conn = db.Create();
        await conn.OpenAsync();
        var rows = await conn.QueryAsync<ReviewAdminRow>(
            "SELECT id, apartment_id AS ApartmentId, rating, text, guest_name AS GuestName, created_at AS CreatedAt, approved FROM reviews ORDER BY created_at DESC");
        return rows.Select(r => new AdminReviewDto(r.Id, r.ApartmentId, r.Rating, r.Text, r.GuestName, r.CreatedAt, r.Approved)).ToList();
    }

    public async Task<ReviewDto> CreateAsync(Guid apartmentId, CreateReviewRequest r)
    {
        using var conn = db.Create();
        await conn.OpenAsync();
        var id = await conn.ExecuteScalarAsync<Guid>(
            """
            INSERT INTO reviews (apartment_id, rating, text, guest_name)
            VALUES (@apartmentId, @rating, @text, @guestName)
            RETURNING id
            """,
            new { apartmentId, rating = r.Rating, text = r.Text, guestName = r.GuestName });
        var row = await conn.QueryFirstAsync<ReviewRow>(
            "SELECT id, apartment_id AS ApartmentId, rating, text, guest_name AS GuestName, created_at AS CreatedAt FROM reviews WHERE id = @id", new { id });
        return new ReviewDto(row.Id, row.ApartmentId, row.Rating, row.Text, row.GuestName, row.CreatedAt);
    }

    public async Task<bool> ApproveAsync(Guid id)
    {
        using var conn = db.Create();
        await conn.OpenAsync();
        var affected = await conn.ExecuteAsync("UPDATE reviews SET approved = true WHERE id = @id", new { id });
        return affected > 0;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        using var conn = db.Create();
        await conn.OpenAsync();
        var affected = await conn.ExecuteAsync("DELETE FROM reviews WHERE id = @id", new { id });
        return affected > 0;
    }

    private record ReviewRow(Guid Id, Guid ApartmentId, short Rating, string? Text, string? GuestName, DateTime CreatedAt);
    private record ReviewAdminRow(Guid Id, Guid ApartmentId, short Rating, string? Text, string? GuestName, DateTime CreatedAt, bool Approved);
}
