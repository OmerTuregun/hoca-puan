namespace HocaPuan.Core.Constants;

/// <summary>
/// Yorum güncellik oylaması için eşik değerleri.
/// Tüm freshness mantığı bu sabitler üzerinden yürür — değiştirmek için tek yer burası.
/// </summary>
public static class FreshnessVotingConstants
{
    /// <summary>Oylamanın açılması için yorumun en az bu kadar eski olması gerekir.</summary>
    public static readonly TimeSpan MinAgeForVoting = TimeSpan.FromDays(365);

    /// <summary>Yüzde ve "eskimiş" bayrağı için gereken minimum oy sayısı.</summary>
    public const int MinVotesForDisplay = 3;

    /// <summary>
    /// "Geçerli değil" oylarının bu oranı aşması halinde yorum eskimiş sayılır (0–1 arası).
    /// Örnek: 0.5 → %50'den fazlası "hayır" derse eskimiş.
    /// </summary>
    public const double OutdatedThreshold = 0.5;
}
