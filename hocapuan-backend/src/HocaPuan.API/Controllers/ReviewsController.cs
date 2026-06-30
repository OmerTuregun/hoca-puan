using System.Security.Claims;
using HocaPuan.API.Extensions;
using HocaPuan.Core.DTOs.Review;
using HocaPuan.Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace HocaPuan.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReviewsController : ControllerBase
{
    private readonly IReviewService _reviewService;
    public ReviewsController(IReviewService reviewService) => _reviewService = reviewService;

    private int CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    private bool IsAdmin => User.IsInRole("Admin") || User.IsInRole("Moderator");

    /// <summary>Giriş yapmış kullanıcının yorumları</summary>
    [HttpGet("my")]
    [Authorize]
    public async Task<IActionResult> GetMyReviews([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var result = await _reviewService.GetByUserAsync(CurrentUserId, page, pageSize);
        return Ok(result);
    }

    /// <summary>Bekleyen yorumlar (Admin)</summary>
    [HttpGet("pending")]
    [Authorize(Roles = "Admin,Moderator")]
    public async Task<IActionResult> GetPending([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var result = await _reviewService.GetPendingAsync(page, pageSize);
        return Ok(result);
    }

    private int? TryGetCurrentUserId()
    {
        var id = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return id != null ? int.Parse(id) : null;
    }

    /// <summary>Hocaya ait yorumları getir</summary>
    [HttpGet("professor/{professorId:int}")]
    public async Task<IActionResult> GetByProfessor(
        int professorId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string sortBy = "newest",
        [FromQuery] string? tag = null)
    {
        var result = await _reviewService.GetByProfessorAsync(
            professorId, page, pageSize, TryGetCurrentUserId(), sortBy, tag);
        return Ok(result);
    }

    /// <summary>Yorum detayını getir</summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _reviewService.GetByIdAsync(id);
        if (result == null) return NotFound();
        return Ok(result);
    }

    /// <summary>Yeni yorum ekle (Giriş gerekli)</summary>
    [HttpPost]
    [Authorize]
    [EnableRateLimiting(RateLimitingExtensions.CommentWritePolicy)]
    public async Task<IActionResult> Create([FromBody] CreateReviewDto dto)
    {
        try
        {
            var result = await _reviewService.CreateAsync(CurrentUserId, dto);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>Yorumu güncelle (sadece sahibi)</summary>
    [HttpPut("{id:int}")]
    [Authorize]
    [EnableRateLimiting(RateLimitingExtensions.CommentWritePolicy)]
    public async Task<IActionResult> UpdateReview(int id, [FromBody] UpdateReviewDto dto)
    {
        try
        {
            var result = await _reviewService.UpdateAsync(id, CurrentUserId, dto);
            if (result == null) return NotFound();
            return Ok(result);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>Yorumu sil</summary>
    [HttpDelete("{id:int}")]
    [Authorize]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var success = await _reviewService.DeleteAsync(id, CurrentUserId, IsAdmin);
            if (!success) return NotFound();
            return NoContent();
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    /// <summary>Yorum oyla (👍/👎)</summary>
    [HttpPost("{id:int}/vote")]
    [Authorize]
    [EnableRateLimiting(RateLimitingExtensions.VotePolicy)]
    public async Task<IActionResult> Vote(int id, [FromQuery] bool isUpvote)
    {
        try
        {
            var result = await _reviewService.VoteAsync(id, CurrentUserId, isUpvote);
            return Ok(result);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    /// <summary>Yorumu uygunsuz olarak bildir</summary>
    [HttpPost("{id:int}/report")]
    [Authorize]
    [EnableRateLimiting(RateLimitingExtensions.ReportWritePolicy)]
    public async Task<IActionResult> Report(int id)
    {
        try
        {
            var result = await _reviewService.ReportAsync(id, CurrentUserId);
            return Ok(result);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    /// <summary>Moderasyon — yorum onayla/reddet (Admin)</summary>
    [HttpPost("{id:int}/moderate")]
    [Authorize(Roles = "Admin,Moderator")]
    public async Task<IActionResult> Moderate(int id, [FromBody] ModerateReviewDto dto)
    {
        try
        {
            var result = await _reviewService.ModerateAsync(id, dto);
            return Ok(result);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    /// <summary>Yorum güncellik oyu — 1+ yıllık yorumlar için "hâlâ geçerli mi?"</summary>
    [HttpPost("{id:int}/freshness-vote")]
    [Authorize]
    [EnableRateLimiting(RateLimitingExtensions.VotePolicy)]
    public async Task<IActionResult> FreshnessVote(int id, [FromBody] FreshnessVoteDto dto)
    {
        try
        {
            var result = await _reviewService.VoteFreshnessAsync(id, CurrentUserId, dto.IsStillValid);
            return Ok(result);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
