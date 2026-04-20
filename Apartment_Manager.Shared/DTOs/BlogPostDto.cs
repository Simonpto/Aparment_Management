namespace Apartment_Manager.Shared.DTOs;

public record BlogPostDto(
    Guid Id,
    string Title,
    string Slug,
    string? Excerpt,
    string? ImageUrl,
    string? Category,
    DateTime? PublishedAt
);

public record BlogPostDetailDto(
    Guid Id,
    string Title,
    string Slug,
    string? Content,
    string? Excerpt,
    string? ImageUrl,
    string? Category,
    DateTime? PublishedAt
);

public record CreateBlogPostRequest(
    string Title,
    string Slug,
    string? Content,
    string? Excerpt,
    string? ImageUrl,
    string? Category
);

public record UpdateBlogPostRequest(
    string? Title,
    string? Content,
    string? Excerpt,
    string? ImageUrl,
    string? Category,
    bool? Published
);
