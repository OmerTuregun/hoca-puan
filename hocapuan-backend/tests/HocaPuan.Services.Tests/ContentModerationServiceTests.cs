using HocaPuan.Core.Interfaces.Moderation;
using HocaPuan.Services.Moderation;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace HocaPuan.Services.Tests;

public class ContentModerationServiceTests
{
    private readonly ContentModerationService _sut = CreateService();

    private static ContentModerationService CreateService()
    {
        var provider = new FileBannedWordsProvider(NullLogger<FileBannedWordsProvider>.Instance);
        return new ContentModerationService(provider, NullLogger<ContentModerationService>.Instance);
    }

    [Fact]
    public void Moderate_CleanText_IsAllowed()
    {
        var result = _sut.Moderate("Hocanın ders anlatımı çok iyiydi, sınavlar adildi.");

        Assert.True(result.IsAllowed);
        Assert.False(result.RequiresManualReview);
        Assert.Empty(result.MatchedCategories);
    }

    [Fact]
    public void Moderate_ExplicitProfanity_IsRejected()
    {
        var result = _sut.Moderate("Bu hoca için siktir git demek istemem ama kötüydü.");

        Assert.False(result.IsAllowed);
        Assert.False(result.RequiresManualReview);
        Assert.Contains("Küfür", result.MatchedCategories);
        Assert.NotNull(result.RejectionReason);
    }

    [Fact]
    public void Moderate_ObfuscatedProfanity_IsRejected()
    {
        var result = _sut.Moderate("Ders çok kötüydü s*i*k*t*i*r.");

        Assert.False(result.IsAllowed);
        Assert.False(result.RequiresManualReview);
        Assert.NotEmpty(result.MatchedCategories);
    }

    [Fact]
    public void Moderate_ValidTcKimlikNo_IsRejected()
    {
        var result = _sut.Moderate("Numaram 10000000146 ile ulaşın.");

        Assert.False(result.IsAllowed);
        Assert.Contains("KisiselVeri", result.MatchedCategories);
        Assert.Contains("kişisel veri", result.RejectionReason, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Moderate_InvalidTcChecksum_IsAllowed()
    {
        var result = _sut.Moderate("Rastgele sayı 12345678901 ama geçersiz TC.");

        Assert.True(result.IsAllowed);
        Assert.False(result.RequiresManualReview);
    }

    [Fact]
    public void Moderate_PhoneNumber_IsRejected()
    {
        var result = _sut.Moderate("Beni arayın: 0532 123 45 67");

        Assert.False(result.IsAllowed);
        Assert.Contains("KisiselVeri", result.MatchedCategories);
    }

    [Fact]
    public void Moderate_LegitimateWordWithEmbeddedSubstring_IsAllowed()
    {
        var resultKlasik = _sut.Moderate("Bu ders klasik müzik tarihi üzerineydi.");
        var resultKonsiktirme = _sut.Moderate("Konsantre olmak için konsiktirme tekniği anlattı.");

        Assert.True(resultKlasik.IsAllowed);
        Assert.True(resultKonsiktirme.IsAllowed);
    }

    [Fact]
    public void Moderate_Email_IsRejected()
    {
        var result = _sut.Moderate("Bana student@uni.edu.tr adresinden yazın.");

        Assert.False(result.IsAllowed);
        Assert.Contains("KisiselVeri", result.MatchedCategories);
    }

    [Fact]
    public void Moderate_MultiWordPhrase_RequiresManualReview()
    {
        var result = _sut.Moderate("Bu hoca gerçekten cahilin önde gideni.");

        Assert.True(result.IsAllowed);
        Assert.True(result.RequiresManualReview);
        Assert.Contains("Kişilik hakareti", result.ManualReviewReasons);
    }

    [Fact]
    public void Moderate_ThreatPhrase_IsRejected()
    {
        var result = _sut.Moderate("Seni öldürürüm diye tehdit etti.");

        Assert.False(result.IsAllowed);
        Assert.Contains("Tehdit", result.MatchedCategories);
    }

    [Fact]
    public void Moderate_MildInsultLan_RequiresManualReview()
    {
        var result = _sut.Moderate("Lan bu ne biçim ders.");

        Assert.True(result.IsAllowed);
        Assert.True(result.RequiresManualReview);
        Assert.Contains("Orta düzey küfür", result.ManualReviewReasons);
    }

    [Fact]
    public void Moderate_KufurOrta_RequiresManualReview()
    {
        var result = _sut.Moderate("Lan bu ne biçim ders.");

        Assert.True(result.RequiresManualReview);
        Assert.DoesNotContain(result.ManualReviewReasons, r => r.Contains("redd", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Moderate_HakaretKisilik_RequiresManualReview()
    {
        var result = _sut.Moderate("Bu hoca tam bir yalancı çıktı.");

        Assert.True(result.IsAllowed);
        Assert.True(result.RequiresManualReview);
        Assert.Contains("Kişilik hakareti", result.ManualReviewReasons);
    }

    [Fact]
    public void Moderate_ExcessiveCaps_RequiresManualReview()
    {
        var result = _sut.Moderate("BU HOCA GERÇEKTEN ÇOK KÖTÜ BİR DERS VERİYOR");

        Assert.True(result.IsAllowed);
        Assert.True(result.RequiresManualReview);
        Assert.Contains("Aşırı büyük harf kullanımı", result.ManualReviewReasons);
    }

    [Fact]
    public void Moderate_ShortCapsText_IsAllowed()
    {
        var result = _sut.Moderate("ÇOK KÖTÜ");

        Assert.True(result.IsAllowed);
        Assert.False(result.RequiresManualReview);
    }

    [Fact]
    public void Moderate_ExcessivePunctuation_RequiresManualReview()
    {
        var result = _sut.Moderate("Bu çok kötüydü!!!");

        Assert.True(result.IsAllowed);
        Assert.True(result.RequiresManualReview);
        Assert.Contains("Aşırı ünlem/soru işareti", result.ManualReviewReasons);
    }

    [Fact]
    public void Moderate_KufurAgir_IsRejected_NotPending()
    {
        var result = _sut.Moderate("Bu hoca için siktir git demek istemem ama kötüydü.");

        Assert.False(result.IsAllowed);
        Assert.False(result.RequiresManualReview);
    }

    [Fact]
    public void Moderate_FuzzyGerizekali_RequiresManualReview()
    {
        var result = _sut.Moderate("Bu hoca tam bir gerzekalı çıktı.");

        Assert.True(result.RequiresManualReview);
        Assert.Contains("Orta düzey küfür", result.ManualReviewReasons);
    }

    [Fact]
    public void Moderate_FuzzyAptal_RequiresManualReview()
    {
        var result = _sut.Moderate("Bu aptl hoca ders anlatmıyor.");

        Assert.True(result.RequiresManualReview);
        Assert.Contains("Orta düzey küfür", result.ManualReviewReasons);
    }

    [Fact]
    public void Moderate_FuzzySikerTypo_IsRejected()
    {
        // "sier" → "siker" (1 harf eksik)
        var result = _sut.Moderate("Bu ne biçim sier davranış.");

        Assert.False(result.IsAllowed);
        Assert.Contains("Küfür", result.MatchedCategories);
    }

    [Fact]
    public void Moderate_ShortProfanity_NoFuzzyMatch_ExactOnly()
    {
        // "sik" 3 karakter — fuzzy devreye girmez; tam eşleşme kelime sınırıyla çalışır
        var exactMatch = _sut.Moderate("Bu ders sik kadar zordu.");
        var fuzzyOnly = _sut.Moderate("Bu ders si kadar zordu.");

        Assert.False(exactMatch.IsAllowed);
        Assert.True(fuzzyOnly.IsAllowed);
    }

    [Fact]
    public void Moderate_UnrelatedWord_NoFuzzyFalsePositive()
    {
        var result = _sut.Moderate("Merhaba, bugün hava çok güzel ve ders gayet verimli geçti.");

        Assert.True(result.IsAllowed);
        Assert.False(result.RequiresManualReview);
        Assert.Empty(result.MatchedCategories);
    }

    [Theory]
    [InlineData("Hocanın anlatımı çok akıcıydı, sınavlar adildi.")]
    [InlineData("Laboratuvar çalışmaları faydalı oldu, ödevler makul seviyedeydi.")]
    [InlineData("Üniversite kampüsü güzel, kütüphane sessiz ve verimli.")]
    public void Moderate_NormalSentences_NoFalsePositive(string text)
    {
        var result = _sut.Moderate(text);

        Assert.True(result.IsAllowed);
        Assert.False(result.RequiresManualReview);
    }

    [Fact]
    public void Moderate_LongText_FuzzyMatchingCompletesQuickly()
    {
        const string word = "verimli ";
        var longText = string.Concat(Enumerable.Repeat(word, 220)) +
                       "sonunda gerzekalı bir davranış gösterdi.";

        var sw = System.Diagnostics.Stopwatch.StartNew();
        var result = _sut.Moderate(longText);
        sw.Stop();

        Assert.True(result.RequiresManualReview);
        Assert.True(sw.ElapsedMilliseconds < 3000, $"Fuzzy matching took {sw.ElapsedMilliseconds}ms");
    }

    [Theory]
    [InlineData("Bu hoca skyor gibi davranıyor.")]
    [InlineData("Bu ne biçim sikior davranış.")]
    public void Moderate_VulgarRootConjugation_IsRejected(string text)
    {
        var result = _sut.Moderate(text);

        Assert.False(result.IsAllowed);
        Assert.Contains("Küfür", result.MatchedCategories);
    }

    [Theory]
    [InlineData("Osmanlı döneminde sikke para birimi kullanılırdı.")]
    [InlineData("Polonyalı komutan Sikorski hakkında makale okuduk.")]
    [InlineData("Fizik dersinde vektörleri işledik.")]
    public void Moderate_VulgarRootExceptions_AreAllowed(string text)
    {
        var result = _sut.Moderate(text);

        Assert.True(result.IsAllowed);
        Assert.False(result.RequiresManualReview);
    }

    [Theory]
    [InlineData("Hocanın anlatımı çok akıcıydı, sınavlar adildi.")]
    [InlineData("Laboratuvar çalışmaları faydalı oldu.")]
    public void Moderate_CleanCommentsWithRootLikeSubstrings_AreAllowed(string text)
    {
        var result = _sut.Moderate(text);

        Assert.True(result.IsAllowed);
        Assert.False(result.RequiresManualReview);
    }
}
