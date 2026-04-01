using HocaPuan.Core.DTOs.Import;

namespace HocaPuan.Core.Models;

public class ImportJob
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N")[..8];
    public string Status { get; set; } = "running"; // running | done | failed
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
    public DateTime? FinishedAt { get; set; }
    public YokBulkImportResponseDto? Result { get; set; }
    public string? Error { get; set; }
}
