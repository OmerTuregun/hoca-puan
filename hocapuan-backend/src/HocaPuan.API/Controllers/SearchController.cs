using HocaPuan.Core.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace HocaPuan.API.Controllers;

[ApiController]
[Route("api/search")]
public class SearchController : ControllerBase
{
    private readonly ISearchService _searchService;

    public SearchController(ISearchService searchService) => _searchService = searchService;

    /// <summary>Navbar typeahead önerileri (hoca, üniversite, bölüm)</summary>
    [HttpGet("suggestions")]
    public async Task<IActionResult> GetSuggestions([FromQuery] string? query)
    {
        var result = await _searchService.GetSuggestionsAsync(query);
        return Ok(result);
    }
}
