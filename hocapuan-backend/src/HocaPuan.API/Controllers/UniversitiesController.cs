using HocaPuan.Core.DTOs.University;
using HocaPuan.Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HocaPuan.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UniversitiesController : ControllerBase
{
    private readonly IUniversityService _universityService;
    public UniversitiesController(IUniversityService universityService) => _universityService = universityService;

    /// <summary>Tüm üniversiteleri listele (arama destekli)</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? search)
    {
        var result = await _universityService.GetAllAsync(search);
        return Ok(result);
    }

    /// <summary>Üniversite detayı</summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _universityService.GetByIdAsync(id);
        if (result == null) return NotFound();
        return Ok(result);
    }

    /// <summary>Fakülteleri getir</summary>
    [HttpGet("{id:int}/faculties")]
    public async Task<IActionResult> GetFaculties(int id)
    {
        var result = await _universityService.GetFacultiesAsync(id);
        return Ok(result);
    }

    /// <summary>Bölümleri getir</summary>
    [HttpGet("faculties/{facultyId:int}/departments")]
    public async Task<IActionResult> GetDepartments(int facultyId)
    {
        var result = await _universityService.GetDepartmentsAsync(facultyId);
        return Ok(result);
    }

    /// <summary>Üniversite ekle (Admin)</summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] CreateUniversityDto dto)
    {
        var result = await _universityService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>Üniversite güncelle (Admin)</summary>
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id, [FromBody] CreateUniversityDto dto)
    {
        try
        {
            var result = await _universityService.UpdateAsync(id, dto);
            return Ok(result);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    /// <summary>Üniversite sil (Admin)</summary>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var success = await _universityService.DeleteAsync(id);
        if (!success) return NotFound();
        return NoContent();
    }
}
