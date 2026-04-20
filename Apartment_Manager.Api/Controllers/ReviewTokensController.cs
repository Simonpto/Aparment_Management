using Apartment_Manager.Api.Services;
using Apartment_Manager.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Apartment_Manager.Api.Controllers;

[ApiController]
[Route("api/review-tokens")]
public class ReviewTokensController(IReviewTokenService service) : ControllerBase
{
    [HttpGet("validate/{token}")]
    public async Task<IActionResult> Validate(string token) =>
        Ok(await service.ValidateAsync(token));

    [HttpPost, Authorize]
    public async Task<IActionResult> Generate([FromBody] GenerateTokenRequest request)
    {
        var token = await service.GenerateAsync(request);
        return new JsonResult(token);
    }
}
