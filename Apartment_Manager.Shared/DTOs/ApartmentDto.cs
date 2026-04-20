namespace Apartment_Manager.Shared.DTOs;

public record ApartmentDto(
    Guid Id,
    string Title,
    string? Description,
    string? Location,
    List<string> Amenities,
    short? MaxGuests,
    short? Bedrooms,
    decimal? BasePricePerNight,
    List<ApartmentImageDto> Images
);

public record ApartmentImageDto(
    Guid Id,
    string ImageUrl,
    int DisplayOrder,
    bool IsPrimary
);

public record CreateApartmentRequest(
    string Title,
    string? Description,
    string? Location,
    List<string>? Amenities,
    short? MaxGuests,
    short? Bedrooms,
    decimal? BasePricePerNight
);

public record UpdateApartmentRequest(
    string? Title,
    string? Description,
    string? Location,
    List<string>? Amenities,
    short? MaxGuests,
    short? Bedrooms,
    decimal? BasePricePerNight
);
