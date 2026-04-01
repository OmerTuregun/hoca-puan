using System.Security.Claims;
using HocaPuan.Core.DTOs.Professor;
using HocaPuan.Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HocaPuan.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProfessorsController : ControllerBase
{
    private readonly IProfessorService _professorService;
    public ProfessorsController(IProfessorService professorService) => _professorService = professorService;

    /// <summary>Hoca ara / listele</summary>
    [HttpGet]
    public async Task<IActionResult> Search(
        [FromQuery] ProfessorSearchDto dto,
        [FromQuery(Name = "search")] string? search)
    {
        if (string.IsNullOrWhiteSpace(dto.Query) && !string.IsNullOrWhiteSpace(search))
            dto.Query = search;

        var result = await _professorService.SearchAsync(dto);
        return Ok(result);
    }

    /// <summary>Hoca detayını getir</summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _professorService.GetByIdAsync(id);
        if (result == null) return NotFound(new { message = "Hoca bulunamadı." });
        return Ok(result);
    }

    /// <summary>Yeni hoca ekle (Admin)</summary>
    [HttpPost]
    [Authorize(Roles = "Admin,Moderator")]
    public async Task<IActionResult> Create([FromBody] CreateProfessorDto dto)
    {
        var result = await _professorService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>Hoca güncelle (Admin)</summary>
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin,Moderator")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateProfessorDto dto)
    {
        try
        {
            var result = await _professorService.UpdateAsync(id, dto);
            return Ok(result);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = "Hoca bulunamadı." });
        }
    }

    /// <summary>Hoca sil (Admin)</summary>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var success = await _professorService.DeleteAsync(id);
        if (!success) return NotFound(new { message = "Hoca bulunamadı." });
        return NoContent();
    }
}
