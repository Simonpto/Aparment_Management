using Apartment_Manager.Api.Infrastructure;
using Apartment_Manager.Api.Services;
using Apartment_Manager.Shared.DTOs;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Apartment_Manager.Tests.Unit;

public class ReviewServiceTests
{
    private readonly IReviewRepository _repo = Substitute.For<IReviewRepository>();
    private readonly IReviewTokenRepository _tokenRepo = Substitute.For<IReviewTokenRepository>();
    private readonly ReviewService _sut;

    public ReviewServiceTests()
    {
        _sut = new ReviewService(_repo, _tokenRepo);
    }

    // ── SubmitAsync ──────────────────────────────────────────────────

    [Fact]
    public async Task Submit_InvalidTokenFormat_ThrowsInvalidOperationException()
    {
        var request = new CreateReviewRequest("not-a-guid", 5, "Great!", "Alice");

        await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.SubmitAsync(request));
    }

    [Fact]
    public async Task Submit_TokenNotFound_ThrowsInvalidOperationException()
    {
        var token = Guid.NewGuid();
        _tokenRepo.GetByTokenAsync(token).Returns((TokenData?)null);

        var request = new CreateReviewRequest(token.ToString(), 4, "Nice", "Bob");

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.SubmitAsync(request));
        Assert.Equal("Token not found.", ex.Message);
    }

    [Fact]
    public async Task Submit_TokenAlreadyUsed_ThrowsInvalidOperationException()
    {
        var token = Guid.NewGuid();
        var tokenData = new TokenData(Guid.NewGuid(), Guid.NewGuid(), token, DateTime.UtcNow.AddDays(7), Used: true);
        _tokenRepo.GetByTokenAsync(token).Returns(tokenData);

        var request = new CreateReviewRequest(token.ToString(), 4, "Nice", "Bob");

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.SubmitAsync(request));
        Assert.Equal("This review link has already been used.", ex.Message);
    }

    [Fact]
    public async Task Submit_TokenExpired_ThrowsInvalidOperationException()
    {
        var token = Guid.NewGuid();
        var tokenData = new TokenData(Guid.NewGuid(), Guid.NewGuid(), token, DateTime.UtcNow.AddDays(-1), Used: false);
        _tokenRepo.GetByTokenAsync(token).Returns(tokenData);

        var request = new CreateReviewRequest(token.ToString(), 4, "Nice", "Bob");

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.SubmitAsync(request));
        Assert.Equal("This review link has expired.", ex.Message);
    }

    [Fact]
    public async Task Submit_ValidToken_CreatesReviewAndMarksTokenUsed()
    {
        var token = Guid.NewGuid();
        var tokenId = Guid.NewGuid();
        var apartmentId = Guid.NewGuid();
        var tokenData = new TokenData(tokenId, apartmentId, token, DateTime.UtcNow.AddDays(7), Used: false);
        _tokenRepo.GetByTokenAsync(token).Returns(tokenData);

        var expectedReview = new ReviewDto(Guid.NewGuid(), apartmentId, 5, "Excellent!", "Carol", DateTime.UtcNow);
        _repo.CreateAsync(apartmentId, Arg.Any<CreateReviewRequest>()).Returns(expectedReview);

        var request = new CreateReviewRequest(token.ToString(), 5, "Excellent!", "Carol");
        var result = await _sut.SubmitAsync(request);

        Assert.Equal(expectedReview.Id, result.Id);
        await _tokenRepo.Received(1).MarkUsedAsync(tokenId);
    }

    [Fact]
    public async Task Submit_ValidToken_PassesCorrectApartmentIdToRepo()
    {
        var token = Guid.NewGuid();
        var apartmentId = Guid.NewGuid();
        var tokenData = new TokenData(Guid.NewGuid(), apartmentId, token, DateTime.UtcNow.AddDays(3), Used: false);
        _tokenRepo.GetByTokenAsync(token).Returns(tokenData);
        _repo.CreateAsync(apartmentId, Arg.Any<CreateReviewRequest>())
             .Returns(new ReviewDto(Guid.NewGuid(), apartmentId, 3, null, null, DateTime.UtcNow));

        await _sut.SubmitAsync(new CreateReviewRequest(token.ToString(), 3, null, null));

        await _repo.Received(1).CreateAsync(apartmentId, Arg.Any<CreateReviewRequest>());
    }

    // ── ApproveAsync / DeleteAsync ───────────────────────────────────

    [Fact]
    public async Task Approve_DelegatesToRepository()
    {
        var id = Guid.NewGuid();
        _repo.ApproveAsync(id).Returns(true);

        var result = await _sut.ApproveAsync(id);

        Assert.True(result);
        await _repo.Received(1).ApproveAsync(id);
    }

    [Fact]
    public async Task Delete_DelegatesToRepository()
    {
        var id = Guid.NewGuid();
        _repo.DeleteAsync(id).Returns(false);

        var result = await _sut.DeleteAsync(id);

        Assert.False(result);
        await _repo.Received(1).DeleteAsync(id);
    }
}
