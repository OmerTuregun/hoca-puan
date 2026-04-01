using HocaPuan.Core.DTOs.Import;

namespace HocaPuan.Core.Interfaces.Services;

public interface IYokPlaywrightScraperService
{
    Task<YokBrowserDebugDto> DebugUniversityListPageAsync(CancellationToken cancellationToken = default);
    Task<YokBrowserDebugDto> DebugProfessorDetailAsync(string detailUrl, CancellationToken cancellationToken = default);
    Task<List<YokUniversityPreviewDto>> PreviewUniversityListAsync(CancellationToken cancellationToken = default);
    Task<YokBulkImportResponseDto> ImportProfessorsAsync(YokBulkImportRequestDto request, CancellationToken cancellationToken = default);
}
