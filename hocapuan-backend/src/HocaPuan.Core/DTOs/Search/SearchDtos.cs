namespace HocaPuan.Core.DTOs.Search;

public class SearchSuggestionDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string? Context { get; set; }
    public int? UniversityId { get; set; }
}
