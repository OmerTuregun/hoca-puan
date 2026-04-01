using HocaPuan.Core.DTOs.Import;

namespace HocaPuan.Core.Interfaces.Services;

public interface IYokScraperService
{
    Task<YokScrapeResponseDto> ScrapePreviewAsync(YokScrapeRequestDto request, CancellationToken cancellationToken = default);
    Task<YokBulkImportResponseDto> ImportProfessorsAsync(YokBulkImportRequestDto request, CancellationToken cancellationToken = default);
}
