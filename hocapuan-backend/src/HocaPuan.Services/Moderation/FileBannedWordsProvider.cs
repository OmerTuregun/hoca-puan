using System.Text.Json;
using HocaPuan.Core.Interfaces.Moderation;
using Microsoft.Extensions.Logging;

namespace HocaPuan.Services.Moderation;

/// <summary>
/// İçerik moderasyonu için tek otorite kelime kaynağı: banned-words.tr.json.
/// Mining/candidate script çıktıları bu provider üzerinden okunmaz.
/// </summary>
public class FileBannedWordsProvider : IBannedWordsProvider
{
    private static readonly Dictionary<string, string> CategoryDisplayNames = new(StringComparer.OrdinalIgnoreCase)
    {
        ["kufur_agir"] = "Küfür",
        ["kufur_orta"] = "Küfür",
        ["hakaret_kisilik"] = "Hakaret",
        ["hakaret_kucumseme"] = "Hakaret",
        ["argo_kucuk_dusurucu"] = "Hakaret",
        ["ayrimcilik_nefret"] = "Nefret",
        ["tehdit_siddet"] = "Tehdit",
    };

    private readonly IReadOnlyDictionary<string, IReadOnlyList<string>> _wordsByCategory;
    private readonly IReadOnlyDictionary<string, IReadOnlyList<string>> _rawWordsByCategory;
    private readonly ILogger<FileBannedWordsProvider> _logger;

    public FileBannedWordsProvider(ILogger<FileBannedWordsProvider> logger)
    {
        _logger = logger;
        (_wordsByCategory, _rawWordsByCategory) = LoadFromFile();
    }

    public IReadOnlyDictionary<string, IReadOnlyList<string>> GetWordsByCategory() => _wordsByCategory;

    public IReadOnlyDictionary<string, IReadOnlyList<string>> GetRawWordsByCategory() => _rawWordsByCategory;

    private (IReadOnlyDictionary<string, IReadOnlyList<string>>, IReadOnlyDictionary<string, IReadOnlyList<string>>) LoadFromFile()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Moderation", "banned-words.tr.json");
        if (!File.Exists(path))
        {
            _logger.LogError("Yasaklı kelime dosyası bulunamadı: {Path}", path);
            var empty = new Dictionary<string, IReadOnlyList<string>>();
            return (empty, empty);
        }

        var json = File.ReadAllText(path);
        var merged = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
        var raw = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

        using var doc = JsonDocument.Parse(json);
        foreach (var prop in doc.RootElement.EnumerateObject())
        {
            if (prop.Name.StartsWith("_", StringComparison.Ordinal) ||
                prop.Value.ValueKind != JsonValueKind.Array)
            {
                continue;
            }

            if (!raw.TryGetValue(prop.Name, out var rawList))
            {
                rawList = [];
                raw[prop.Name] = rawList;
            }

            var displayCategory = CategoryDisplayNames.GetValueOrDefault(prop.Name, prop.Name);
            if (!merged.TryGetValue(displayCategory, out var list))
            {
                list = [];
                merged[displayCategory] = list;
            }

            foreach (var item in prop.Value.EnumerateArray())
            {
                if (item.ValueKind != JsonValueKind.String) continue;
                var word = item.GetString()?.Trim();
                if (string.IsNullOrEmpty(word)) continue;
                if (!rawList.Contains(word, StringComparer.OrdinalIgnoreCase))
                    rawList.Add(word);
                if (!list.Contains(word, StringComparer.OrdinalIgnoreCase))
                    list.Add(word);
            }
        }

        var mergedResult = merged.ToDictionary(
            kv => kv.Key,
            kv => (IReadOnlyList<string>)kv.Value,
            StringComparer.OrdinalIgnoreCase);

        var rawResult = raw.ToDictionary(
            kv => kv.Key,
            kv => (IReadOnlyList<string>)kv.Value,
            StringComparer.OrdinalIgnoreCase);

        return (mergedResult, rawResult);
    }
}
