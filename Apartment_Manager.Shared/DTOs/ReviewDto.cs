namespace Apartment_Manager.Shared.DTOs;

public record ReviewDto(
    Guid Id,
    Guid ApartmentId,
    short Rating,
    string? Text,
    string? GuestName,
    DateTime CreatedAt
);

public record AdminReviewDto(
    Guid Id,
    Guid ApartmentId,
    short Rating,
    string? Text,
    string? GuestName,
    DateTime CreatedAt,
    bool Approved
);

public record CreateReviewRequest(
    string Token,
    short Rating,
    string? Text,
    string? GuestName
);

public record GenerateTokenRequest(
    Guid ApartmentId,
    int ExpiryDays = 7
);

public record TokenValidationResult(
    bool IsValid,
    Guid? ApartmentId
);
