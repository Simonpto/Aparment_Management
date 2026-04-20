using Apartment_Manager.Api.Infrastructure;
using Apartment_Manager.Api.Services;
using Apartment_Manager.Shared.DTOs;
using NSubstitute;

namespace Apartment_Manager.Tests.Unit;

public class ReviewTokenServiceTests
{
    private readonly IReviewTokenRepository _repo = Substitute.For<IReviewTokenRepository>();
    private readonly ReviewTokenService _sut;

    public ReviewTokenServiceTests()
    {
        _sut = new ReviewTokenService(_repo);
    }

    // ── ValidateAsync ────────────────────────────────────────────────

    [Fact]
    public async Task Validate_InvalidGuidFormat_ReturnsFalse()
    {
        var result = await _sut.ValidateAsync("not-a-guid");

        Assert.False(result.IsValid);
        Assert.Null(result.ApartmentId);
        await _repo.DidNotReceive().GetByTokenAsync(Arg.Any<Guid>());
    }

    [Fact]
    public async Task Validate_TokenNotFound_ReturnsFalse()
    {
        var token = Guid.NewGuid();
        _repo.GetByTokenAsync(token).Returns((TokenData?)null);

        var result = await _sut.ValidateAsync(token.ToString());

        Assert.False(result.IsValid);
        Assert.Null(result.ApartmentId);
    }

    [Fact]
    public async Task Validate_TokenAlreadyUsed_ReturnsFalse()
    {
        var token = Guid.NewGuid();
        _repo.GetByTokenAsync(token)
             .Returns(new TokenData(Guid.NewGuid(), Guid.NewGuid(), token, DateTime.UtcNow.AddDays(7), Used: true));

        var result = await _sut.ValidateAsync(token.ToString());

        Assert.False(result.IsValid);
    }

    [Fact]
    public async Task Validate_TokenExpired_ReturnsFalse()
    {
        var token = Guid.NewGuid();
        _repo.GetByTokenAsync(token)
             .Returns(new TokenData(Guid.NewGuid(), Guid.NewGuid(), token, DateTime.UtcNow.AddSeconds(-1), Used: false));

        var result = await _sut.ValidateAsync(token.ToString());

        Assert.False(result.IsValid);
    }

    [Fact]
    public async Task Validate_ValidToken_ReturnsTrueWithApartmentId()
    {
        var token = Guid.NewGuid();
        var apartmentId = Guid.NewGuid();
        _repo.GetByTokenAsync(token)
             .Returns(new TokenData(Guid.NewGuid(), apartmentId, token, DateTime.UtcNow.AddDays(7), Used: false));

        var result = await _sut.ValidateAsync(token.ToString());

        Assert.True(result.IsValid);
        Assert.Equal(apartmentId, result.ApartmentId);
    }

    // ── GenerateAsync ────────────────────────────────────────────────

    [Fact]
    public async Task Generate_ReturnsGuidStringFromRepo()
    {
        var apartmentId = Guid.NewGuid();
        var generatedToken = Guid.NewGuid();
        _repo.CreateAsync(apartmentId, Arg.Any<TimeSpan>()).Returns(generatedToken);

        var result = await _sut.GenerateAsync(new GenerateTokenRequest(apartmentId, ExpiryDays: 7));

        Assert.Equal(generatedToken.ToString(), result);
    }

    [Fact]
    public async Task Generate_PassesCorrectExpiryToRepo()
    {
        var apartmentId = Guid.NewGuid();
        _repo.CreateAsync(apartmentId, Arg.Any<TimeSpan>()).Returns(Guid.NewGuid());

        await _sut.GenerateAsync(new GenerateTokenRequest(apartmentId, ExpiryDays: 14));

        await _repo.Received(1).CreateAsync(apartmentId, TimeSpan.FromDays(14));
    }
}
