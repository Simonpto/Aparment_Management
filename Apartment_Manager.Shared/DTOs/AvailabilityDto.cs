namespace Apartment_Manager.Shared.DTOs;

public record AvailabilityDto(
    Guid Id,
    DateOnly Date,
    bool IsAvailable,
    decimal? PriceOverride
);

public record UpdateAvailabilityRequest(
    DateOnly Date,
    bool IsAvailable,
    decimal? PriceOverride
);
