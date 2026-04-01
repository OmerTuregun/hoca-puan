namespace HocaPuan.Core.DTOs.Import;

public class YokScrapeRequestDto
{
    // Form field/value pairs captured from browser network tab.
    public Dictionary<string, string> FormData { get; set; } = new();
}

public class YokScrapeResponseDto
{
    public int PostStatusCode { get; set; }
    public string? RedirectLocation { get; set; }
    public int? FinalStatusCode { get; set; }
    public string? FinalUrl { get; set; }
    public int HtmlLength { get; set; }
    public string HtmlPreview { get; set; } = string.Empty;
}

public class YokBulkImportRequestDto
{
    // Empty means full import using all universities in DB.
    public List<int> UniversityIds { get; set; } = new();
    public int MaxPerUniversity { get; set; } = 200;
    // Optional template for YOK form fields captured from preview endpoint.
    public Dictionary<string, string>? FormDataTemplate { get; set; }
}

public class YokBulkImportResponseDto
{
    public int UniversitiesProcessed { get; set; }
    public int ParsedCandidates { get; set; }
    public int InsertedProfessors { get; set; }
    public int SkippedDuplicates { get; set; }
    public int TotalNotes { get; set; }
    public bool NotesTruncated { get; set; }
    public List<string> Notes { get; set; } = new();
}

public class YokUniversityPreviewDto
{
    public string YokName { get; set; } = string.Empty;
    public string BirimToken { get; set; } = string.Empty;
}

public class YokBrowserDebugDto
{
    public string FinalUrl { get; set; } = string.Empty;
    public bool IsBlocked { get; set; }
    public int HtmlLength { get; set; }
    public string HtmlPreview { get; set; } = string.Empty;
    public int HrefLinksFound { get; set; }
    public int OnclickLinksFound { get; set; }
    public List<string> SampleLinkTexts { get; set; } = new();
    public List<string> SampleHrefs { get; set; } = new();
}
