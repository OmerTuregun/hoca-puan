namespace HocaPuan.Core.Interfaces.Moderation;

/// <summary>Kategori → yasaklı kelime kökleri. İleride DB implementasyonu ile değiştirilebilir.</summary>
public interface IBannedWordsProvider
{
    /// <summary>Görüntüleme adlarıyla birleştirilmiş kategoriler (geriye dönük uyumluluk).</summary>
    IReadOnlyDictionary<string, IReadOnlyList<string>> GetWordsByCategory();

    /// <summary>JSON dosyasındaki ham kategori anahtarları (kufur_agir, kufur_orta, …).</summary>
    IReadOnlyDictionary<string, IReadOnlyList<string>> GetRawWordsByCategory();
}
