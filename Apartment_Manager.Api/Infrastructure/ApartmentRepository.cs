using System.Text.Json;
using Apartment_Manager.Shared.DTOs;
using Dapper;

namespace Apartment_Manager.Api.Infrastructure;

public class ApartmentRepository(DatabaseConnectionFactory db)
{
    private const string SelectApartment =
        """
        SELECT id, title, description, location, amenities,
               max_guests AS MaxGuests, bedrooms,
               base_price_per_night AS BasePricePerNight
        FROM apartments
        """;

    private const string SelectImages =
        """
        SELECT id, apartment_id AS ApartmentId, image_url AS ImageUrl,
               display_order AS DisplayOrder, is_primary AS IsPrimary
        FROM apartment_images
        """;

    public async Task<List<ApartmentDto>> GetAllAsync()
    {
        using var conn = db.Create();
        await conn.OpenAsync();

        var rows = (await conn.QueryAsync<ApartmentRow>($"{SelectApartment} ORDER BY created_at DESC")).ToList();

        var ids = rows.Select(r => r.Id).ToArray();
        var images = ids.Length > 0
            ? await conn.QueryAsync<ImageRow>($"{SelectImages} WHERE apartment_id = ANY(@ids) ORDER BY display_order", new { ids })
            : Enumerable.Empty<ImageRow>();

        var imageMap = images.GroupBy(i => i.ApartmentId).ToDictionary(g => g.Key, g => g.ToList());
        return rows.Select(r => MapDto(r, imageMap.GetValueOrDefault(r.Id, []))).ToList();
    }

    public async Task<ApartmentDto?> GetByIdAsync(Guid id)
    {
        using var conn = db.Create();
        await conn.OpenAsync();

        var row = await conn.QueryFirstOrDefaultAsync<ApartmentRow>($"{SelectApartment} WHERE id = @id", new { id });
        if (row is null) return null;

        var images = await conn.QueryAsync<ImageRow>($"{SelectImages} WHERE apartment_id = @id ORDER BY display_order", new { id });
        return MapDto(row, images.ToList());
    }

    public async Task<ApartmentDto> CreateAsync(CreateApartmentRequest r)
    {
        using var conn = db.Create();
        await conn.OpenAsync();

        var amenities = JsonSerializer.Serialize(r.Amenities ?? []);
        var id = await conn.ExecuteScalarAsync<Guid>(
            """
            INSERT INTO apartments (title, description, location, amenities, max_guests, bedrooms, base_price_per_night)
            VALUES (@title, @description, @location, @amenities::jsonb, @maxGuests, @bedrooms, @basePrice)
            RETURNING id
            """,
            new { title = r.Title, description = r.Description, location = r.Location, amenities, maxGuests = r.MaxGuests, bedrooms = r.Bedrooms, basePrice = r.BasePricePerNight });

        return (await GetByIdAsync(id))!;
    }

    public async Task<bool> UpdateAsync(Guid id, UpdateApartmentRequest r)
    {
        using var conn = db.Create();
        await conn.OpenAsync();

        var amenities = r.Amenities != null ? JsonSerializer.Serialize(r.Amenities) : null;
        var affected = await conn.ExecuteAsync(
            """
            UPDATE apartments SET
                title                = COALESCE(@title, title),
                description          = COALESCE(@description, description),
                location             = COALESCE(@location, location),
                amenities            = COALESCE(@amenities::jsonb, amenities),
                max_guests           = COALESCE(@maxGuests, max_guests),
                bedrooms             = COALESCE(@bedrooms, bedrooms),
                base_price_per_night = COALESCE(@basePrice, base_price_per_night),
                updated_at           = NOW()
            WHERE id = @id
            """,
            new { id, title = r.Title, description = r.Description, location = r.Location, amenities, maxGuests = r.MaxGuests, bedrooms = r.Bedrooms, basePrice = r.BasePricePerNight });

        return affected > 0;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        using var conn = db.Create();
        await conn.OpenAsync();
        var affected = await conn.ExecuteAsync("DELETE FROM apartments WHERE id = @id", new { id });
        return affected > 0;
    }

    public async Task<List<AvailabilityDto>> GetAvailabilityAsync(Guid id)
    {
        using var conn = db.Create();
        await conn.OpenAsync();
        var rows = await conn.QueryAsync<AvailabilityRow>(
            """
            SELECT id, date, is_available AS IsAvailable, price_override AS PriceOverride
            FROM availability WHERE apartment_id = @id ORDER BY date
            """,
            new { id });
        return rows.Select(r => new AvailabilityDto(r.Id, r.Date, r.IsAvailable, r.PriceOverride)).ToList();
    }

    public async Task UpsertAvailabilityAsync(Guid apartmentId, UpdateAvailabilityRequest r)
    {
        using var conn = db.Create();
        await conn.OpenAsync();
        await conn.ExecuteAsync(
            """
            INSERT INTO availability (apartment_id, date, is_available, price_override)
            VALUES (@apartmentId, @date, @isAvailable, @priceOverride)
            ON CONFLICT (apartment_id, date) DO UPDATE SET
                is_available   = EXCLUDED.is_available,
                price_override = EXCLUDED.price_override
            """,
            new { apartmentId, date = r.Date, isAvailable = r.IsAvailable, priceOverride = r.PriceOverride });
    }

    private static ApartmentDto MapDto(ApartmentRow r, List<ImageRow> images)
    {
        var amenities = string.IsNullOrEmpty(r.Amenities)
            ? new List<string>()
            : JsonSerializer.Deserialize<List<string>>(r.Amenities) ?? [];

        return new ApartmentDto(
            r.Id, r.Title, r.Description, r.Location, amenities,
            r.MaxGuests, r.Bedrooms, r.BasePricePerNight,
            images.Select(i => new ApartmentImageDto(i.Id, i.ImageUrl, i.DisplayOrder, i.IsPrimary)).ToList()
        );
    }

    private record ApartmentRow(Guid Id, string Title, string? Description, string? Location,
        string? Amenities, short? MaxGuests, short? Bedrooms, decimal? BasePricePerNight);

    private record ImageRow(Guid Id, Guid ApartmentId, string ImageUrl, int DisplayOrder, bool IsPrimary);

    private record AvailabilityRow(Guid Id, DateOnly Date, bool IsAvailable, decimal? PriceOverride);
}
