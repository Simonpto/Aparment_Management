namespace Apartment_Manager.Api.Infrastructure;

public record TokenData(Guid Id, Guid ApartmentId, Guid Token, DateTime ExpiresAt, bool Used);
