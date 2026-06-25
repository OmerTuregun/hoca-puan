using HocaPuan.Core.DTOs.Search;

namespace HocaPuan.Core.Interfaces.Services;

public interface ISearchService
{
    Task<List<SearchSuggestionDto>> GetSuggestionsAsync(string? query);
}
