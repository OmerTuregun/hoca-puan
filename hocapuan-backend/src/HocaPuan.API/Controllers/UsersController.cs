using System.Security.Claims;
using HocaPuan.Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HocaPuan.API.Controllers;

[ApiController]
[Route("api/users")]
public class UsersController : ControllerBase
{
    private readonly IReviewService _reviewService;

    public UsersController(IReviewService reviewService)
    {
        _reviewService = reviewService;
    }

    private int CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    /// <summary>Giriş yapmış kullanıcının katkı geçmişi (tüm yorumlar + toplam faydalı oy)</summary>
    [HttpGet("me/contributions")]
    [Authorize]
    public async Task<IActionResult> GetMyContributions(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var result = await _reviewService.GetContributionHistoryAsync(CurrentUserId, page, pageSize);
        return Ok(result);
    }
}
