using HocaPuan.Core.Entities;

namespace HocaPuan.Data.Seed;

/// <summary>
/// YÖK Atlas'tan otomatik üretilmiş — 27.03.2026
/// 219 üniversite | 1841 fakülte
/// </summary>
public static class DatabaseSeeder
{
    public static async Task SeedAsync(AppDbContext context)
    {
        await SeedUniversitiesAsync(context);
        await SeedAdminUserAsync(context);
    }

    private static async Task SeedUniversitiesAsync(AppDbContext context)
    {
        if (context.Universities.Any()) return;

        var universities = new List<University>
        {
            new() { Name = "Abdullah Gül Üniversitesi", ShortName = "AGÜ", City = "Kayseri", Type = UniversityType.Devlet, Website = "https://www.agu.edu.tr", EmailDomain = "agu.edu.tr" },
            new() { Name = "Acıbadem Mehmet Ali Aydınlar Üniversitesi", ShortName = "AMAAÜ", City = "Aydın", Type = UniversityType.Vakif, Website = null, EmailDomain = null },
            new() { Name = "Adana Alparslan Türkeş Bilim ve Teknoloji Üniversitesi", ShortName = "ATÜ", City = "Adana", Type = UniversityType.Devlet, Website = "https://www.atu.edu.tr", EmailDomain = "atu.edu.tr" },
            new() { Name = "Adana Bilim ve Teknoloji Üniversitesi", ShortName = "ABTS", City = "Adana", Type = UniversityType.Devlet, Website = "https://www.atu.edu.tr", EmailDomain = "atu.edu.tr" },
            new() { Name = "Adıyaman Üniversitesi", ShortName = "ADYÜ", City = "Adıyaman", Type = UniversityType.Devlet, Website = "https://www.adiyaman.edu.tr", EmailDomain = "adiyaman.edu.tr" },
            new() { Name = "Afyon Kocatepe Üniversitesi", ShortName = "AKÜ", City = "Afyonkarahisar", Type = UniversityType.Devlet, Website = "https://www.aku.edu.tr", EmailDomain = "aku.edu.tr" },
            new() { Name = "Afyonkarahisar Sağlık Bilimleri Üniversitesi", ShortName = "AKSÜ", City = "Afyonkarahisar", Type = UniversityType.Devlet, Website = "https://www.afsu.edu.tr", EmailDomain = "afsu.edu.tr" },
            new() { Name = "Ağrı İbrahim Çeçen Üniversitesi", ShortName = "AİÇÜ", City = "Ağrı", Type = UniversityType.Devlet, Website = "https://www.agri.edu.tr", EmailDomain = "agri.edu.tr" },
            new() { Name = "Akdeniz Üniversitesi", ShortName = "AÜ", City = "Antalya", Type = UniversityType.Devlet, Website = "https://www.akdeniz.edu.tr", EmailDomain = "akdeniz.edu.tr" },
            new() { Name = "Aksaray Üniversitesi", ShortName = "AKSÜ", City = "Aksaray", Type = UniversityType.Devlet, Website = "https://www.aksaray.edu.tr", EmailDomain = "aksaray.edu.tr" },
            new() { Name = "Alanya Alaaddin Keykubat Üniversitesi", ShortName = "ALKÜ", City = "Antalya", Type = UniversityType.Devlet, Website = "https://www.alku.edu.tr", EmailDomain = "alku.edu.tr" },
            new() { Name = "Alanya Haedreddin Barbaros Üniversitesi", ShortName = "AHBÜ", City = "Antalya", Type = UniversityType.Vakif, Website = null, EmailDomain = null },
            new() { Name = "Alanya Üniversitesi", ShortName = "AÜ", City = "Antalya", Type = UniversityType.Vakif, Website = null, EmailDomain = null },
            new() { Name = "Altınbaş Üniversitesi", ShortName = "AÜ", City = "İstanbul", Type = UniversityType.Vakif, Website = null, EmailDomain = null },
            new() { Name = "Amasya Üniversitesi", ShortName = "AÜ", City = "Amasya", Type = UniversityType.Devlet, Website = "https://www.amasya.edu.tr", EmailDomain = "amasya.edu.tr" },
            new() { Name = "Anadolu Üniversitesi", ShortName = "AÜ", City = "Eskişehir", Type = UniversityType.Devlet, Website = "https://www.anadolu.edu.tr", EmailDomain = "anadolu.edu.tr" },
            new() { Name = "Ankara Bilim Üniversitesi", ShortName = "ABÜ", City = "Ankara", Type = UniversityType.Vakif, Website = null, EmailDomain = null },
            new() { Name = "Ankara Hacı Bayram Veli Üniversitesi", ShortName = "AHBVÜ", City = "Ankara", Type = UniversityType.Devlet, Website = "https://www.ahbv.edu.tr", EmailDomain = "ahbv.edu.tr" },
            new() { Name = "Ankara Medipol Üniversitesi", ShortName = "AMÜ", City = "Ankara", Type = UniversityType.Vakif, Website = null, EmailDomain = null },
            new() { Name = "Ankara Müzik ve Güzel Sanatlar Üniversitesi", ShortName = "AMGÜ", City = "Ankara", Type = UniversityType.Devlet, Website = "https://www.amgu.edu.tr", EmailDomain = "amgu.edu.tr" },
            new() { Name = "Ankara Sosyal Bilimler Üniversitesi", ShortName = "ASBÜ", City = "Ankara", Type = UniversityType.Devlet, Website = "https://www.asbu.edu.tr", EmailDomain = "asbu.edu.tr" },
            new() { Name = "Ankara Uluslararası Atatürk Alevi Bektaşi Üniversitesi", ShortName = "AUAABÜ", City = "Ankara", Type = UniversityType.Vakif, Website = null, EmailDomain = null },
            new() { Name = "Ankara Üniversitesi", ShortName = "AÜ", City = "Ankara", Type = UniversityType.Devlet, Website = "https://www.ankara.edu.tr", EmailDomain = "ankara.edu.tr" },
            new() { Name = "Ankara Yıldırım Beyazıt Üniversitesi", ShortName = "AYBÜ", City = "Ankara", Type = UniversityType.Devlet, Website = "https://www.aybu.edu.tr", EmailDomain = "aybu.edu.tr" },
            new() { Name = "Antalya Belek Üniversitesi", ShortName = "ABÜ", City = "Antalya", Type = UniversityType.Vakif, Website = null, EmailDomain = null },
            new() { Name = "Antalya Bilim Üniversitesi", ShortName = "ABÜ", City = "Antalya", Type = UniversityType.Vakif, Website = null, EmailDomain = null },
            new() { Name = "Ardahan Üniversitesi", ShortName = "AÜ", City = "Ardahan", Type = UniversityType.Devlet, Website = "https://www.ardahan.edu.tr", EmailDomain = "ardahan.edu.tr" },
            new() { Name = "Artvin Çoruh Üniversitesi", ShortName = "AÇÜ", City = "Artvin", Type = UniversityType.Devlet, Website = "https://www.artvin.edu.tr", EmailDomain = "artvin.edu.tr" },
            new() { Name = "Atatürk Üniversitesi", ShortName = "AÜ", City = "Erzurum", Type = UniversityType.Devlet, Website = "https://www.atauni.edu.tr", EmailDomain = "atauni.edu.tr" },
            new() { Name = "Atılım Üniversitesi", ShortName = "AÜ", City = "Ankara", Type = UniversityType.Vakif, Website = null, EmailDomain = null },
            new() { Name = "Avrasya Üniversitesi", ShortName = "AÜ", City = "Trabzon", Type = UniversityType.Vakif, Website = null, EmailDomain = null },
            new() { Name = "Aydın Adnan Menderes Üniversitesi", ShortName = "AAMÜ", City = "Aydın", Type = UniversityType.Devlet, Website = null, EmailDomain = null },
            new() { Name = "Bahçeşehir Üniversitesi", ShortName = "BAU", City = "İstanbul", Type = UniversityType.Vakif, Website = "https://www.bau.edu.tr", EmailDomain = "bau.edu.tr" },
            new() { Name = "Balıkesır Üniversitesi", ShortName = "BÜ", City = "Balıkesir", Type = UniversityType.Devlet, Website = "https://www.balikesir.edu.tr", EmailDomain = "balikesir.edu.tr" },
            new() { Name = "Bandırma Onyedi Eylül Üniversitesi", ShortName = "BANDÜ", City = "Balıkesir", Type = UniversityType.Devlet, Website = "https://www.bandirma.edu.tr", EmailDomain = "bandirma.edu.tr" },
            new() { Name = "Bartın Üniversitesi", ShortName = "BARÜ", City = "Bartın", Type = UniversityType.Devlet, Website = "https://www.bartin.edu.tr", EmailDomain = "bartin.edu.tr" },
            new() { Name = "Başkent Üniversitesi", ShortName = "BÜ", City = "Ankara", Type = UniversityType.Vakif, Website = null, EmailDomain = null },
            new() { Name = "Batman Üniversitesi", ShortName = "BÜ", City = "Batman", Type = UniversityType.Devlet, Website = "https://www.batman.edu.tr", EmailDomain = "batman.edu.tr" },
            new() { Name = "Bayburt Üniversitesi", ShortName = "BAYBÜ", City = "Bayburt", Type = UniversityType.Devlet, Website = "https://www.bayburt.edu.tr", EmailDomain = "bayburt.edu.tr" },
            new() { Name = "Beykentüniversitesi", ShortName = "B", City = "İstanbul", Type = UniversityType.Vakif, Website = null, EmailDomain = null },
            new() { Name = "Beykoz Üniversitesi", ShortName = "BÜ", City = "İstanbul", Type = UniversityType.Vakif, Website = null, EmailDomain = null },
            new() { Name = "Bezm-i Âlem Vakıf Üniversitesi", ShortName = "BÂVÜ", City = "Bilinmiyor", Type = UniversityType.Vakif, Website = null, EmailDomain = null },
            new() { Name = "Bezmiâlem Vakıf Üniversitesi", ShortName = "BVÜ", City = "İstanbul", Type = UniversityType.Vakif, Website = "https://www.bezmialem.edu.tr", EmailDomain = "bezmialem.edu.tr" },
            new() { Name = "Bilecik Şeyh Edebali Üniversitesi", ShortName = "BŞEÜ", City = "Bilecik", Type = UniversityType.Devlet, Website = "https://www.bilecik.edu.tr", EmailDomain = "bilecik.edu.tr" },
            new() { Name = "Bilkent Üniversitesi", ShortName = "BILKENT", City = "Ankara", Type = UniversityType.Vakif, Website = "https://www.bilkent.edu.tr", EmailDomain = "bilkent.edu.tr" },
            new() { Name = "Bingöl Üniversitesi", ShortName = "BÜ", City = "Bingöl", Type = UniversityType.Devlet, Website = "https://www.bingol.edu.tr", EmailDomain = "bingol.edu.tr" },
            new() { Name = "Biruni Üniversitesi", ShortName = "BÜ", City = "İstanbul", Type = UniversityType.Vakif, Website = null, EmailDomain = null },
            new() { Name = "Bitlis Eren Üniversitesi", ShortName = "BEÜ", City = "Bitlis", Type = UniversityType.Devlet, Website = "https://www.beu.edu.tr", EmailDomain = "beu.edu.tr" },
            new() { Name = "Boğaziçi Üniversitesi", ShortName = "BOUN", City = "İstanbul", Type = UniversityType.Devlet, Website = "https://www.boun.edu.tr", EmailDomain = "boun.edu.tr" },
            new() { Name = "Bolu Abant İzzet Baysal Üniversitesi", ShortName = "BAIBU", City = "Bolu", Type = UniversityType.Devlet, Website = "https://www.ibu.edu.tr", EmailDomain = "ibu.edu.tr" },
            new() { Name = "Bursa Teknik Üniversitesi", ShortName = "BTÜ", City = "Bursa", Type = UniversityType.Devlet, Website = "https://www.btu.edu.tr", EmailDomain = "btu.edu.tr" },
            new() { Name = "Bursa Uludağ Üniversitesi", ShortName = "BUÜ", City = "Bursa", Type = UniversityType.Devlet, Website = "https://www.uludag.edu.tr", EmailDomain = "uludag.edu.tr" },
            new() { Name = "Cumhuriyet Üniversitesi", ShortName = "CÜ", City = "Sivas", Type = UniversityType.Devlet, Website = "https://www.cumhuriyet.edu.tr", EmailDomain = "cumhuriyet.edu.tr" },
            new() { Name = "Çağ Üniversitesi", ShortName = "ÇÜ", City = "Mersin", Type = UniversityType.Vakif, Website = null, EmailDomain = null },
            new() { Name = "Çanakkale Onsekiz Mart Üniversitesi", ShortName = "ÇOMÜ", City = "Çanakkale", Type = UniversityType.Devlet, Website = "https://www.comu.edu.tr", EmailDomain = "comu.edu.tr" },
            new() { Name = "Çankaya Üniversitesi", ShortName = "ÇÜ", City = "Ankara", Type = UniversityType.Vakif, Website = null, EmailDomain = null },
            new() { Name = "Çankırı Karatekin Üniversitesi", ShortName = "ÇKÜ", City = "Çankırı", Type = UniversityType.Devlet, Website = "https://www.karatekin.edu.tr", EmailDomain = "karatekin.edu.tr" },
            new() { Name = "Çukurova Üniversitesi", ShortName = "ÇÜ", City = "Adana", Type = UniversityType.Devlet, Website = "https://www.cu.edu.tr", EmailDomain = "cu.edu.tr" },
            new() { Name = "Demiroğlu Bilim Üniversitesi", ShortName = "DBÜ", City = "İstanbul", Type = UniversityType.Vakif, Website = null, EmailDomain = null },
            new() { Name = "Dicle Üniversitesi", ShortName = "DÜ", City = "Diyarbakır", Type = UniversityType.Devlet, Website = "https://www.dicle.edu.tr", EmailDomain = "dicle.edu.tr" },
            new() { Name = "Doğu Akdeniz Üniversitesi", ShortName = "DAÜ", City = "Gazimağusa", Type = UniversityType.Vakif, Website = null, EmailDomain = null },
            new() { Name = "Doğu Anadolu Üniversitesi", ShortName = "DAÜ", City = "Bilinmiyor", Type = UniversityType.Vakif, Website = null, EmailDomain = null },
            new() { Name = "Doğuş Üniversitesi", ShortName = "DU", City = "İstanbul", Type = UniversityType.Vakif, Website = "https://www.dogus.edu.tr", EmailDomain = "dogus.edu.tr" },
            new() { Name = "Dokuz Eylül Üniversitesi", ShortName = "DEÜ", City = "İzmir", Type = UniversityType.Devlet, Website = "https://www.deu.edu.tr", EmailDomain = "deu.edu.tr" },
            new() { Name = "Dünya Göz Hastaneler Grubu Üniversitesi", ShortName = "DGHGÜ", City = "Bilinmiyor", Type = UniversityType.Vakif, Website = null, EmailDomain = null },
            new() { Name = "Düzce Üniversitesi", ShortName = "DÜ", City = "Düzce", Type = UniversityType.Devlet, Website = "https://www.duzce.edu.tr", EmailDomain = "duzce.edu.tr" },
            new() { Name = "Ege Üniversitesi", ShortName = "EÜ", City = "İzmir", Type = UniversityType.Devlet, Website = "https://www.ege.edu.tr", EmailDomain = "ege.edu.tr" },
            new() { Name = "Ercıyes Üniversitesi", ShortName = "ERÜ", City = "Kayseri", Type = UniversityType.Devlet, Website = "https://www.erciyes.edu.tr", EmailDomain = "erciyes.edu.tr" },
            new() { Name = "Erzincan Binali Yıldırım Üniversitesi", ShortName = "EBYU", City = "Erzincan", Type = UniversityType.Devlet, Website = "https://www.erzincan.edu.tr", EmailDomain = "erzincan.edu.tr" },
            new() { Name = "Erzurum Teknik Üniversitesi", ShortName = "ETÜ", City = "Erzurum", Type = UniversityType.Devlet, Website = "https://www.erzurum.edu.tr", EmailDomain = "erzurum.edu.tr" },
            new() { Name = "Eskişehir Osmangazi Üniversitesi", ShortName = "ESOGÜ", City = "Eskişehir", Type = UniversityType.Devlet, Website = "https://www.ogu.edu.tr", EmailDomain = "ogu.edu.tr" },
            new() { Name = "Eskişehir Teknik Üniversitesi", ShortName = "ESTÜ", City = "Eskişehir", Type = UniversityType.Devlet, Website = "https://www.eskisehir.edu.tr", EmailDomain = "eskisehir.edu.tr" },
            new() { Name = "Fatih Sultan Mehmet Vakıf Üniversitesi", ShortName = "FSMVÜ", City = "İstanbul", Type = UniversityType.Vakif, Website = null, EmailDomain = null },
            new() { Name = "Fenerbahçe Üniversitesi", ShortName = "FBÜ", City = "İstanbul", Type = UniversityType.Vakif, Website = "https://www.fenerbahce.edu.tr", EmailDomain = "fenerbahce.edu.tr" },
            new() { Name = "Fırat Üniversitesi", ShortName = "FÜ", City = "Elazığ", Type = UniversityType.Devlet, Website = "https://www.firat.edu.tr", EmailDomain = "firat.edu.tr" },
            new() { Name = "Galatasaray Üniversitesi", ShortName = "GSÜ", City = "İstanbul", Type = UniversityType.Devlet, Website = "https://www.gsu.edu.tr", EmailDomain = "gsu.edu.tr" },
            new() { Name = "Gazi Üniversitesi", ShortName = "GÜ", City = "Ankara", Type = UniversityType.Devlet, Website = "https://www.gazi.edu.tr", EmailDomain = "gazi.edu.tr" },
            new() { Name = "Gaziantep İslam Bilim ve Teknoloji Üniversitesi", ShortName = "GİBTÜ", City = "Gaziantep", Type = UniversityType.Devlet, Website = "https://www.gibtu.edu.tr", EmailDomain = "gibtu.edu.tr" },
            new() { Name = "Gaziantep Üniversitesi", ShortName = "GAÜN", City = "Gaziantep", Type = UniversityType.Devlet, Website = "https://www.gantep.edu.tr", EmailDomain = "gantep.edu.tr" },
            new() { Name = "Gebze Teknik Üniversitesi", ShortName = "GTÜ", City = "Kocaeli", Type = UniversityType.Devlet, Website = "https://www.gtu.edu.tr", EmailDomain = "gtu.edu.tr" },
            new() { Name = "Giresun Üniversitesi", ShortName = "GRÜ", City = "Giresun", Type = UniversityType.Devlet, Website = "https://www.giresun.edu.tr", EmailDomain = "giresun.edu.tr" },
            new() { Name = "Girne Amerikan Üniversitesi", ShortName = "GAÜ", City = "KKTC", Type = UniversityType.Vakif, Website = null, EmailDomain = null },
            new() { Name = "Gümüşhane Üniversitesi", ShortName = "GÜ", City = "Gümüşhane", Type = UniversityType.Devlet, Website = "https://www.gumushane.edu.tr", EmailDomain = "gumushane.edu.tr" },
            new() { Name = "Hacettepe Üniversitesi", ShortName = "HÜ", City = "Ankara", Type = UniversityType.Devlet, Website = "https://www.hacettepe.edu.tr", EmailDomain = "hacettepe.edu.tr" },
            new() { Name = "Hakkari Üniversitesi", ShortName = "HÜ", City = "Hakkari", Type = UniversityType.Devlet, Website = "https://www.hakkari.edu.tr", EmailDomain = "hakkari.edu.tr" },
            new() { Name = "Haliç Üniversitesi", ShortName = "HÜ", City = "İstanbul", Type = UniversityType.Vakif, Website = null, EmailDomain = null },
            new() { Name = "Harran Üniversitesi", ShortName = "HRÜ", City = "Şanlıurfa", Type = UniversityType.Devlet, Website = "https://www.harran.edu.tr", EmailDomain = "harran.edu.tr" },
            new() { Name = "Hasan Kalyoncu Üniversitesi", ShortName = "HKÜ", City = "Gaziantep", Type = UniversityType.Vakif, Website = null, EmailDomain = null },
            new() { Name = "Hitit Üniversitesi", ShortName = "HİTÜ", City = "Çorum", Type = UniversityType.Devlet, Website = "https://www.hitit.edu.tr", EmailDomain = "hitit.edu.tr" },
            new() { Name = "Iğdır Üniversitesi", ShortName = "IÜ", City = "Iğdır", Type = UniversityType.Devlet, Website = "https://www.igdir.edu.tr", EmailDomain = "igdir.edu.tr" },
            new() { Name = "Inonu Üniversitesi", ShortName = "İÜ", City = "Malatya", Type = UniversityType.Devlet, Website = "https://www.inonu.edu.tr", EmailDomain = "inonu.edu.tr" },
            new() { Name = "Işık Üniversitesi", ShortName = "IÜ", City = "İstanbul", Type = UniversityType.Vakif, Website = null, EmailDomain = null },
            new() { Name = "İbn Haldun Üniversitesi", ShortName = "İHÜ", City = "İstanbul", Type = UniversityType.Vakif, Website = null, EmailDomain = null },
            new() { Name = "İhsan Doğramacı Bilkent Üniversitesi", ShortName = "BİLKENT", City = "Ankara", Type = UniversityType.Vakif, Website = "https://www.bilkent.edu.tr", EmailDomain = "bilkent.edu.tr" },
            new() { Name = "İnönü Üniversitesi", ShortName = "İÜ", City = "Malatya", Type = UniversityType.Devlet, Website = "https://www.inonu.edu.tr", EmailDomain = "inonu.edu.tr" },
            new() { Name = "İskele Üniversitesi", ShortName = "İÜ", City = "Bilinmiyor", Type = UniversityType.Vakif, Website = null, EmailDomain = null },
            new() { Name = "İskenderun Teknik Üniversitesi", ShortName = "İSTE", City = "Hatay", Type = UniversityType.Devlet, Website = "https://www.iste.edu.tr", EmailDomain = "iste.edu.tr" },
            new() { Name = "İstanbul 29 Mayıs Üniversitesi", ShortName = "İMÜ", City = "İstanbul", Type = UniversityType.Vakif, Website = null, EmailDomain = null },
            new() { Name = "İstanbul Arel Üniversitesi", ShortName = "AREL", City = "İstanbul", Type = UniversityType.Vakif, Website = "https://www.arel.edu.tr", EmailDomain = "arel.edu.tr" },
            new() { Name = "İstanbul Atlas Üniversitesi", ShortName = "ATLAS", City = "İstanbul", Type = UniversityType.Vakif, Website = "https://www.atlas.edu.tr", EmailDomain = "atlas.edu.tr" },
            new() { Name = "İstanbul Aydın Üniversitesi", ShortName = "IAU", City = "İstanbul", Type = UniversityType.Vakif, Website = "https://www.aydin.edu.tr", EmailDomain = "aydin.edu.tr" },
            new() { Name = "İstanbul Beykent Üniversitesi", ShortName = "BEYKENT", City = "İstanbul", Type = UniversityType.Vakif, Website = "https://www.beykent.edu.tr", EmailDomain = "beykent.edu.tr" },
            new() { Name = "İstanbul Bilgi Üniversitesi", ShortName = "BİLGİ", City = "İstanbul", Type = UniversityType.Vakif, Website = "https://www.bilgi.edu.tr", EmailDomain = "bilgi.edu.tr" },
            new() { Name = "İstanbul Bilim Üniversitesi", ShortName = "İBÜ", City = "İstanbul", Type = UniversityType.Vakif, Website = "https://www.istanbul.edu.tr", EmailDomain = "istanbul.edu.tr" },
            new() { Name = "İstanbul Esenyurt Üniversitesi", ShortName = "İEÜ", City = "İstanbul", Type = UniversityType.Vakif, Website = null, EmailDomain = null },
            new() { Name = "İstanbul Galatasaray Üniversitesi", ShortName = "GSÜ", City = "İstanbul", Type = UniversityType.Devlet, Website = "https://www.gsu.edu.tr", EmailDomain = "gsu.edu.tr" },
            new() { Name = "İstanbul Gelişim Üniversitesi", ShortName = "IGÜ", City = "İstanbul", Type = UniversityType.Vakif, Website = "https://www.gelisim.edu.tr", EmailDomain = "gelisim.edu.tr" },
            new() { Name = "İstanbul Kent Üniversitesi", ShortName = "KENT", City = "İstanbul", Type = UniversityType.Vakif, Website = "https://www.kent.edu.tr", EmailDomain = "kent.edu.tr" },
            new() { Name = "İstanbul Kültür Üniversitesi", ShortName = "İKÜ", City = "İstanbul", Type = UniversityType.Vakif, Website = null, EmailDomain = null },
            new() { Name = "İstanbul Medeniyet Üniversitesi", ShortName = "İMÜ", City = "İstanbul", Type = UniversityType.Devlet, Website = "https://www.medeniyet.edu.tr", EmailDomain = "medeniyet.edu.tr" },
            new() { Name = "İstanbul Medipol Üniversitesi", ShortName = "MEDİPOL", City = "İstanbul", Type = UniversityType.Vakif, Website = "https://www.medipol.edu.tr", EmailDomain = "medipol.edu.tr" },
            new() { Name = "İstanbul Okan Üniversitesi", ShortName = "OKAN", City = "İstanbul", Type = UniversityType.Vakif, Website = "https://www.okan.edu.tr", EmailDomain = "okan.edu.tr" },
            new() { Name = "İstanbul Rumelı Üniversitesi", ShortName = "İRÜ", City = "İstanbul", Type = UniversityType.Vakif, Website = null, EmailDomain = null },
            new() { Name = "İstanbul Sabahattın Zaim Üniversitesi", ShortName = "İZÜ", City = "İstanbul", Type = UniversityType.Vakif, Website = "https://www.izu.edu.tr", EmailDomain = "izu.edu.tr" },
            new() { Name = "İstanbul Sabahattin Zaim Üniversitesi", ShortName = "İSZÜ", City = "İstanbul", Type = UniversityType.Vakif, Website = null, EmailDomain = null },
            new() { Name = "İstanbul Teknik Üniversitesi", ShortName = "İTÜ", City = "İstanbul", Type = UniversityType.Devlet, Website = "https://www.itu.edu.tr", EmailDomain = "itu.edu.tr" },
            new() { Name = "İstanbul Ticaret Üniversitesi", ShortName = "İTİCÜ", City = "İstanbul", Type = UniversityType.Vakif, Website = "https://www.ticaret.edu.tr", EmailDomain = "ticaret.edu.tr" },
            new() { Name = "İstanbul Üniversitesi", ShortName = "İÜ", City = "İstanbul", Type = UniversityType.Devlet, Website = "https://www.istanbul.edu.tr", EmailDomain = "istanbul.edu.tr" },
            new() { Name = "İstanbul Üniversitesi-cerrahpaşa", ShortName = "İÜC", City = "İstanbul", Type = UniversityType.Devlet, Website = "https://www.iuc.edu.tr", EmailDomain = "iuc.edu.tr" },
            new() { Name = "İstanbul Yeni Yüzyıl Üniversitesi", ShortName = "İYYÜ", City = "İstanbul", Type = UniversityType.Vakif, Website = "https://www.yeniyuzyil.edu.tr", EmailDomain = "yeniyuzyil.edu.tr" },
            new() { Name = "İzmir Bakırçay Üniversitesi", ShortName = "ISUBÜ", City = "İzmir", Type = UniversityType.Devlet, Website = "https://www.bakircay.edu.tr", EmailDomain = "bakircay.edu.tr" },
            new() { Name = "İzmir Demokrasi Üniversitesi", ShortName = "İDÜ", City = "İzmir", Type = UniversityType.Devlet, Website = "https://www.idu.edu.tr", EmailDomain = "idu.edu.tr" },
            new() { Name = "İzmir Ekonomi Üniversitesi", ShortName = "İEÜ", City = "İzmir", Type = UniversityType.Vakif, Website = "https://www.ieu.edu.tr", EmailDomain = "ieu.edu.tr" },
            new() { Name = "İzmir Katip Çelebi Üniversitesi", ShortName = "İKÇÜ", City = "İzmir", Type = UniversityType.Devlet, Website = "https://www.ikcu.edu.tr", EmailDomain = "ikcu.edu.tr" },
            new() { Name = "İzmir Tin Üniversitesi", ShortName = "İTÜ", City = "İzmir", Type = UniversityType.Vakif, Website = null, EmailDomain = null },
            new() { Name = "İzmir Yüksek Teknoloji Enstitüsü", ShortName = "İYTE", City = "İzmir", Type = UniversityType.Devlet, Website = "https://www.iyte.edu.tr", EmailDomain = "iyte.edu.tr" },
            new() { Name = "Kadir Has Üniversitesi", ShortName = "KHÜ", City = "Bilinmiyor", Type = UniversityType.Vakif, Website = null, EmailDomain = null },
            new() { Name = "Kafkas Üniversitesi", ShortName = "KAÜ", City = "Kars", Type = UniversityType.Devlet, Website = "https://www.kafkas.edu.tr", EmailDomain = "kafkas.edu.tr" },
            new() { Name = "Kahramanmaraş İstiklal Üniversitesi", ShortName = "KİÜ", City = "Kahramanmaraş", Type = UniversityType.Devlet, Website = "https://www.istiklal.edu.tr", EmailDomain = "istiklal.edu.tr" },
            new() { Name = "Kahramanmaraş Sütçü İmam Üniversitesi", ShortName = "KSÜ", City = "Kahramanmaraş", Type = UniversityType.Devlet, Website = "https://www.ksu.edu.tr", EmailDomain = "ksu.edu.tr" },
            new() { Name = "Kapadokya Üniversitesi", ShortName = "KÜ", City = "Bilinmiyor", Type = UniversityType.Vakif, Website = null, EmailDomain = null },
            new() { Name = "Karabük Üniversitesi", ShortName = "KBÜ", City = "Karabük", Type = UniversityType.Devlet, Website = "https://www.karabuk.edu.tr", EmailDomain = "karabuk.edu.tr" },
            new() { Name = "Karadeniz Teknik Üniversitesi", ShortName = "KTÜ", City = "Trabzon", Type = UniversityType.Devlet, Website = "https://www.ktu.edu.tr", EmailDomain = "ktu.edu.tr" },
            new() { Name = "Karamanoğlu Mehmetbey Üniversitesi", ShortName = "KMÜ", City = "Karaman", Type = UniversityType.Devlet, Website = "https://www.kmu.edu.tr", EmailDomain = "kmu.edu.tr" },
            new() { Name = "Kastamonu Üniversitesi", ShortName = "KASTÜ", City = "Kastamonu", Type = UniversityType.Devlet, Website = "https://www.kastamonu.edu.tr", EmailDomain = "kastamonu.edu.tr" },
            new() { Name = "Kıbrıs İlim Üniversitesi", ShortName = "KİÜ", City = "KKTC", Type = UniversityType.Vakif, Website = null, EmailDomain = null },
            new() { Name = "Kıbrıs Sağlık ve Toplum Bilimleri Üniversitesi", ShortName = "KSVTBÜ", City = "KKTC", Type = UniversityType.Vakif, Website = null, EmailDomain = null },
            new() { Name = "Kıbrıs Türk Üniversitesi", ShortName = "KTÜ", City = "KKTC", Type = UniversityType.Vakif, Website = null, EmailDomain = null },
            new() { Name = "Kırşehir Ahi Evran Üniversitesi", ShortName = "KAEÜ", City = "Kırşehir", Type = UniversityType.Devlet, Website = null, EmailDomain = null },
            new() { Name = "Kilis 7 Aralık Üniversitesi", ShortName = "KİLİS", City = "Kilis", Type = UniversityType.Devlet, Website = "https://www.kilis.edu.tr", EmailDomain = "kilis.edu.tr" },
            new() { Name = "Kocaeli Üniversitesi", ShortName = "KOU", City = "Kocaeli", Type = UniversityType.Devlet, Website = "https://www.kocaeli.edu.tr", EmailDomain = "kocaeli.edu.tr" },
            new() { Name = "Koç Üniversitesi", ShortName = "KU", City = "İstanbul", Type = UniversityType.Vakif, Website = "https://www.ku.edu.tr", EmailDomain = "ku.edu.tr" },
            new() { Name = "Konya Gıda ve Tarım Üniversitesi", ShortName = "KGVTÜ", City = "Bilinmiyor", Type = UniversityType.Vakif, Website = null, EmailDomain = null },
            new() { Name = "Konya Teknik Üniversitesi", ShortName = "KTÜN", City = "Konya", Type = UniversityType.Devlet, Website = "https://www.ktun.edu.tr", EmailDomain = "ktun.edu.tr" },
            new() { Name = "Kto Karatay Üniversitesi", ShortName = "KKÜ", City = "Konya", Type = UniversityType.Vakif, Website = null, EmailDomain = null },
            new() { Name = "Küçük Kaynarca Üniversitesi", ShortName = "KKÜ", City = "İstanbul", Type = UniversityType.Vakif, Website = "https://www.ucak.edu.tr", EmailDomain = "ucak.edu.tr" },
            new() { Name = "Kütahya Dumlupınar Üniversitesi", ShortName = "DPÜ", City = "Kütahya", Type = UniversityType.Devlet, Website = "https://www.dpu.edu.tr", EmailDomain = "dpu.edu.tr" },
            new() { Name = "Kütahya Sağlık Bilimleri Üniversitesi", ShortName = "KSBU", City = "Kütahya", Type = UniversityType.Devlet, Website = "https://www.ksbu.edu.tr", EmailDomain = "ksbu.edu.tr" },
            new() { Name = "Lefke Avrupa Üniversitesi", ShortName = "LAÜ", City = "KKTC", Type = UniversityType.Vakif, Website = null, EmailDomain = null },
            new() { Name = "Lefkoşa Üniversitesi", ShortName = "LÜ", City = "KKTC", Type = UniversityType.Vakif, Website = null, EmailDomain = null },
            new() { Name = "Lokman Hekim Üniversitesi", ShortName = "LHÜ", City = "Ankara", Type = UniversityType.Vakif, Website = null, EmailDomain = null },
            new() { Name = "Magosa Üniversitesi", ShortName = "MÜ", City = "KKTC", Type = UniversityType.Vakif, Website = null, EmailDomain = null },
            new() { Name = "Malatya Turgut Özal Üniversitesi", ShortName = "MTÜ", City = "Malatya", Type = UniversityType.Devlet, Website = "https://www.malatya.edu.tr", EmailDomain = "malatya.edu.tr" },
            new() { Name = "Manisa Celal Bayar Üniversitesi", ShortName = "CBÜ", City = "Manisa", Type = UniversityType.Devlet, Website = "https://www.cbu.edu.tr", EmailDomain = "cbu.edu.tr" },
            new() { Name = "Mardin Artuklu Üniversitesi", ShortName = "MAÜ", City = "Mardin", Type = UniversityType.Devlet, Website = "https://www.artuklu.edu.tr", EmailDomain = "artuklu.edu.tr" },
            new() { Name = "Marmara Üniversitesi", ShortName = "MÜ", City = "İstanbul", Type = UniversityType.Devlet, Website = "https://www.marmara.edu.tr", EmailDomain = "marmara.edu.tr" },
            new() { Name = "Mef Üniversitesi", ShortName = "MÜ", City = "İstanbul", Type = UniversityType.Vakif, Website = null, EmailDomain = null },
            new() { Name = "Melikşah Üniversitesi", ShortName = "MÜ", City = "Kayseri", Type = UniversityType.Vakif, Website = "https://www.meliksah.edu.tr", EmailDomain = "meliksah.edu.tr" },
            new() { Name = "Mersin Uluslararası Üniversitesi", ShortName = "MUÜ", City = "Bilinmiyor", Type = UniversityType.Vakif, Website = null, EmailDomain = null },
            new() { Name = "Mersin Üniversitesi", ShortName = "MEÜ", City = "Mersin", Type = UniversityType.Devlet, Website = "https://www.mersin.edu.tr", EmailDomain = "mersin.edu.tr" },
            new() { Name = "Milli Savunma Üniversitesi", ShortName = "MSÜ", City = "İstanbul", Type = UniversityType.Devlet, Website = "https://www.msu.edu.tr", EmailDomain = "msu.edu.tr" },
            new() { Name = "Mimar Sinan Güzel Sanatlar Üniversitesi", ShortName = "MSGSÜ", City = "İstanbul", Type = UniversityType.Devlet, Website = "https://www.msgsu.edu.tr", EmailDomain = "msgsu.edu.tr" },
            new() { Name = "Muğla Sıtkı Koçman Üniversitesi", ShortName = "MŞÜ", City = "Muğla", Type = UniversityType.Devlet, Website = "https://www.mu.edu.tr", EmailDomain = "mu.edu.tr" },
            new() { Name = "Munzur Üniversitesi", ShortName = "MÜ", City = "Tunceli", Type = UniversityType.Devlet, Website = "https://www.munzur.edu.tr", EmailDomain = "munzur.edu.tr" },
            new() { Name = "Mustafa Kemal Üniversitesi", ShortName = "MKÜ", City = "Hatay", Type = UniversityType.Devlet, Website = "https://www.mku.edu.tr", EmailDomain = "mku.edu.tr" },
            new() { Name = "Muş Alparslan Üniversitesi", ShortName = "MAÜ", City = "Muş", Type = UniversityType.Devlet, Website = "https://www.alparslan.edu.tr", EmailDomain = "alparslan.edu.tr" },
            new() { Name = "Necmettin Erbakan Üniversitesi", ShortName = "NEÜ", City = "Konya", Type = UniversityType.Devlet, Website = "https://www.erbakan.edu.tr", EmailDomain = "erbakan.edu.tr" },
            new() { Name = "Nevşehir Hacı Bektaş Veli Üniversitesi", ShortName = "NEVÜ", City = "Nevşehir", Type = UniversityType.Devlet, Website = "https://www.nevsehir.edu.tr", EmailDomain = "nevsehir.edu.tr" },
            new() { Name = "Niğde Ömer Halisdemir Üniversitesi", ShortName = "NÖHÜ", City = "Niğde", Type = UniversityType.Devlet, Website = "https://www.ohu.edu.tr", EmailDomain = "ohu.edu.tr" },
            new() { Name = "Nişantaşı Üniversitesi", ShortName = "NÜ", City = "İstanbul", Type = UniversityType.Vakif, Website = null, EmailDomain = null },
            new() { Name = "Nuh Naci Yazgan Üniversitesi", ShortName = "NNY", City = "Kayseri", Type = UniversityType.Vakif, Website = "https://www.nny.edu.tr", EmailDomain = "nny.edu.tr" },
            new() { Name = "Ondokuz Mayıs Üniversitesi", ShortName = "OMÜ", City = "Samsun", Type = UniversityType.Devlet, Website = "https://www.omu.edu.tr", EmailDomain = "omu.edu.tr" },
            new() { Name = "Ordu Üniversitesi", ShortName = "ODÜ", City = "Ordu", Type = UniversityType.Devlet, Website = "https://www.odu.edu.tr", EmailDomain = "odu.edu.tr" },
            new() { Name = "Orta Doğu Teknik Üniversitesi", ShortName = "ODTÜ", City = "Ankara", Type = UniversityType.Devlet, Website = "https://www.metu.edu.tr", EmailDomain = "metu.edu.tr" },
            new() { Name = "Osmaniye Korkut Ata Üniversitesi", ShortName = "OKÜ", City = "Osmaniye", Type = UniversityType.Devlet, Website = "https://www.osmaniye.edu.tr", EmailDomain = "osmaniye.edu.tr" },
            new() { Name = "Özyeğın Üniversitesi", ShortName = "ÖZÜ", City = "İstanbul", Type = UniversityType.Vakif, Website = "https://www.ozyegin.edu.tr", EmailDomain = "ozyegin.edu.tr" },
            new() { Name = "Pamukkale Üniversitesi", ShortName = "PAÜ", City = "Denizli", Type = UniversityType.Devlet, Website = "https://www.pau.edu.tr", EmailDomain = "pau.edu.tr" },
            new() { Name = "Piri Reis Üniversitesi", ShortName = "PRÜ", City = "İstanbul", Type = UniversityType.Vakif, Website = "https://www.pirireis.edu.tr", EmailDomain = "pirireis.edu.tr" },
            new() { Name = "Rauf Denktaş Üniversitesi", ShortName = "RDÜ", City = "KKTC", Type = UniversityType.Vakif, Website = null, EmailDomain = null },
            new() { Name = "Recep Tayyip Erdoğan Üniversitesi", ShortName = "RTEÜ", City = "Rize", Type = UniversityType.Devlet, Website = "https://www.erdogan.edu.tr", EmailDomain = "erdogan.edu.tr" },
            new() { Name = "Refahiye Ertekın Üniversitesi", ShortName = "REÜ", City = "Erzincan", Type = UniversityType.Devlet, Website = "https://www.refahiye.edu.tr", EmailDomain = "refahiye.edu.tr" },
            new() { Name = "Sabancı Üniversitesi", ShortName = "SU", City = "İstanbul", Type = UniversityType.Vakif, Website = "https://www.sabanciuniv.edu", EmailDomain = "sabanciuniv.edu" },
            new() { Name = "Sakarya Uygulamalı Bilimler Üniversitesi", ShortName = "SUBÜ", City = "Sakarya", Type = UniversityType.Devlet, Website = "https://www.subu.edu.tr", EmailDomain = "subu.edu.tr" },
            new() { Name = "Sakarya Üniversitesi", ShortName = "SAÜ", City = "Sakarya", Type = UniversityType.Devlet, Website = "https://www.sakarya.edu.tr", EmailDomain = "sakarya.edu.tr" },
            new() { Name = "Samsun Üniversitesi", ShortName = "SÜ", City = "Samsun", Type = UniversityType.Devlet, Website = "https://www.samsun.edu.tr", EmailDomain = "samsun.edu.tr" },
            new() { Name = "Sanko Üniversitesi", ShortName = "SÜ", City = "Gaziantep", Type = UniversityType.Vakif, Website = null, EmailDomain = null },
            new() { Name = "Selçuk Üniversitesi", ShortName = "SÜ", City = "Konya", Type = UniversityType.Devlet, Website = "https://www.selcuk.edu.tr", EmailDomain = "selcuk.edu.tr" },
            new() { Name = "Siirt Üniversitesi", ShortName = "SİÜ", City = "Siirt", Type = UniversityType.Devlet, Website = "https://www.siirt.edu.tr", EmailDomain = "siirt.edu.tr" },
            new() { Name = "Sinop Üniversitesi", ShortName = "SİNOP", City = "Sinop", Type = UniversityType.Devlet, Website = "https://www.sinop.edu.tr", EmailDomain = "sinop.edu.tr" },
            new() { Name = "Sivas Bilim ve Teknoloji Üniversitesi", ShortName = "SİVBTÜ", City = "Sivas", Type = UniversityType.Devlet, Website = "https://www.sivas.edu.tr", EmailDomain = "sivas.edu.tr" },
            new() { Name = "Sivas Cumhuriyet Üniversitesi", ShortName = "CÜ", City = "Sivas", Type = UniversityType.Devlet, Website = "https://www.cumhuriyet.edu.tr", EmailDomain = "cumhuriyet.edu.tr" },
            new() { Name = "Süleyman Demirel Üniversitesi", ShortName = "SDÜ", City = "Isparta", Type = UniversityType.Devlet, Website = "https://www.sdu.edu.tr", EmailDomain = "sdu.edu.tr" },
            new() { Name = "Şeyh Edebali Üniversitesi", ShortName = "BŞEÜ", City = "Bilecik", Type = UniversityType.Devlet, Website = "https://www.bilecik.edu.tr", EmailDomain = "bilecik.edu.tr" },
            new() { Name = "Şırnak Üniversitesi", ShortName = "ŞRÜ", City = "Şırnak", Type = UniversityType.Devlet, Website = "https://www.sirnak.edu.tr", EmailDomain = "sirnak.edu.tr" },
            new() { Name = "Ted Üniversitesi", ShortName = "TÜ", City = "Ankara", Type = UniversityType.Vakif, Website = null, EmailDomain = null },
            new() { Name = "Tekirdağ Namık Kemal Üniversitesi", ShortName = "NKÜ", City = "Tekirdağ", Type = UniversityType.Devlet, Website = "https://www.nku.edu.tr", EmailDomain = "nku.edu.tr" },
            new() { Name = "Tokat Gaziosmanpaşa Üniversitesi", ShortName = "GOP", City = "Tokat", Type = UniversityType.Devlet, Website = "https://www.gop.edu.tr", EmailDomain = "gop.edu.tr" },
            new() { Name = "Toros Üniversitesi", ShortName = "TÜ", City = "Mersin", Type = UniversityType.Vakif, Website = null, EmailDomain = null },
            new() { Name = "Trakya Üniversitesi", ShortName = "TÜ", City = "Bilinmiyor", Type = UniversityType.Vakif, Website = null, EmailDomain = null },
            new() { Name = "Tuncelı Üniversitesi", ShortName = "TÜ", City = "Tunceli", Type = UniversityType.Devlet, Website = "https://www.munzur.edu.tr", EmailDomain = "munzur.edu.tr" },
            new() { Name = "Türk-alman Üniversitesi", ShortName = "TAÜ", City = "İstanbul", Type = UniversityType.Devlet, Website = "https://www.tau.edu.tr", EmailDomain = "tau.edu.tr" },
            new() { Name = "Türk-japon Bilim ve Teknoloji Üniversitesi", ShortName = "TJBTÜ", City = "İstanbul", Type = UniversityType.Devlet, Website = "https://www.tjstu.edu.tr", EmailDomain = "tjstu.edu.tr" },
            new() { Name = "Türkiye Sağlık Bilimleri Üniversitesi", ShortName = "TSBU", City = "İstanbul", Type = UniversityType.Devlet, Website = "https://www.sbu.edu.tr", EmailDomain = "sbu.edu.tr" },
            new() { Name = "Ufuk Üniversitesi", ShortName = "UÜ", City = "Ankara", Type = UniversityType.Vakif, Website = null, EmailDomain = null },
            new() { Name = "Uluslararası Kıbrıs Üniversitesi", ShortName = "UKÜ", City = "KKTC", Type = UniversityType.Vakif, Website = null, EmailDomain = null },
            new() { Name = "Uluslararası Orta Doğu Üniversitesi", ShortName = "UODÜ", City = "Bilinmiyor", Type = UniversityType.Vakif, Website = null, EmailDomain = null },
            new() { Name = "Uşak Üniversitesi", ShortName = "UŞÜ", City = "Uşak", Type = UniversityType.Devlet, Website = "https://www.usak.edu.tr", EmailDomain = "usak.edu.tr" },
            new() { Name = "Üsküdar Üniversitesi", ShortName = "ÜÜ", City = "İstanbul", Type = UniversityType.Vakif, Website = null, EmailDomain = null },
            new() { Name = "Van Yüzüncü Yıl Üniversitesi", ShortName = "YYÜ", City = "Van", Type = UniversityType.Devlet, Website = "https://www.yyu.edu.tr", EmailDomain = "yyu.edu.tr" },
            new() { Name = "Yakın Doğu Üniversitesi", ShortName = "YDÜ", City = "KKTC", Type = UniversityType.Vakif, Website = null, EmailDomain = null },
            new() { Name = "Yalova Üniversitesi", ShortName = "YÜ", City = "Yalova", Type = UniversityType.Devlet, Website = "https://www.yalova.edu.tr", EmailDomain = "yalova.edu.tr" },
            new() { Name = "Yaşar Üniversitesi", ShortName = "YÜ", City = "İzmir", Type = UniversityType.Vakif, Website = null, EmailDomain = null },
            new() { Name = "Yeditepe Üniversitesi", ShortName = "YÜ", City = "İstanbul", Type = UniversityType.Vakif, Website = null, EmailDomain = null },
            new() { Name = "Yıldırım Beyazıt Üniversitesi", ShortName = "YBÜ", City = "Bilinmiyor", Type = UniversityType.Vakif, Website = null, EmailDomain = null },
            new() { Name = "Yıldız Teknik Üniversitesi", ShortName = "YTÜ", City = "İstanbul", Type = UniversityType.Devlet, Website = "https://www.yildiz.edu.tr", EmailDomain = "yildiz.edu.tr" },
            new() { Name = "Yozgat Bozok Üniversitesi", ShortName = "BOZOK", City = "Yozgat", Type = UniversityType.Devlet, Website = "https://www.bozok.edu.tr", EmailDomain = "bozok.edu.tr" },
            new() { Name = "Yüksek İhtisas Üniversitesi", ShortName = "YİÜ", City = "Ankara", Type = UniversityType.Vakif, Website = null, EmailDomain = null },
            new() { Name = "Zonguldak Bülent Ecevit Üniversitesi", ShortName = "BEÜ", City = "Zonguldak", Type = UniversityType.Devlet, Website = "https://www.beun.edu.tr", EmailDomain = "beun.edu.tr" },
        };

        await context.Universities.AddRangeAsync(universities);
        await context.SaveChangesAsync();

        var uniLookup = universities.ToDictionary(u => u.Name!, u => u);
        var faculties  = new List<Faculty>();

        // Abdullah Gül Üniversitesi
        if (uniLookup.TryGetValue("Abdullah Gül Üniversitesi", out var u1065))
        {
            faculties.Add(new Faculty { Name = "Mimarlık Fakültesi", University = u1065 });
            faculties.Add(new Faculty { Name = "Mühendislik Fakültesi", University = u1065 });
            faculties.Add(new Faculty { Name = "Yaşam ve Doğa Bilimleri Fakültesi", University = u1065 });
            faculties.Add(new Faculty { Name = "Yönetim Bilimleri Fakültesi", University = u1065 });
            faculties.Add(new Faculty { Name = "İnsan ve Toplum Bilimleri Fakültesi", University = u1065 });
        }

        // Acıbadem Mehmet Ali Aydınlar Üniversitesi
        if (uniLookup.TryGetValue("Acıbadem Mehmet Ali Aydınlar Üniversitesi", out var u2001))
        {
            faculties.Add(new Faculty { Name = "Eczacılık Fakültesi", University = u2001 });
            faculties.Add(new Faculty { Name = "Mühendislik ve Doğa Bilimleri Fakültesi", University = u2001 });
            faculties.Add(new Faculty { Name = "Sağlık Bilimleri Fakültesi", University = u2001 });
            faculties.Add(new Faculty { Name = "Tıp Fakültesi", University = u2001 });
            faculties.Add(new Faculty { Name = "İnsan ve Toplum Bilimleri Fakültesi", University = u2001 });
        }

        // Adana Alparslan Türkeş Bilim ve Teknoloji Üniversitesi
        if (uniLookup.TryGetValue("Adana Alparslan Türkeş Bilim ve Teknoloji Üniversitesi", out var u1104))
        {
            faculties.Add(new Faculty { Name = "Bilgisayar ve Bilişim Fakültesi", University = u1104 });
            faculties.Add(new Faculty { Name = "Havacılık ve Uzay Bilimleri Fakültesi", University = u1104 });
            faculties.Add(new Faculty { Name = "Mimarlık ve Tasarım Fakültesi", University = u1104 });
            faculties.Add(new Faculty { Name = "Mühendislik Fakültesi", University = u1104 });
            faculties.Add(new Faculty { Name = "İktisadi, İdari ve Sosyal Bilimler Fakültesi", University = u1104 });
        }

        // Adıyaman Üniversitesi
        if (uniLookup.TryGetValue("Adıyaman Üniversitesi", out var u1002))
        {
            faculties.Add(new Faculty { Name = "Diş Hekimliği Fakültesi", University = u1002 });
            faculties.Add(new Faculty { Name = "Eczacılık Fakültesi", University = u1002 });
            faculties.Add(new Faculty { Name = "Eğitim Fakültesi", University = u1002 });
            faculties.Add(new Faculty { Name = "Fen-Edebiyat Fakültesi", University = u1002 });
            faculties.Add(new Faculty { Name = "Mühendislik Fakültesi", University = u1002 });
            faculties.Add(new Faculty { Name = "Sağlık Bilimleri Fakültesi", University = u1002 });
            faculties.Add(new Faculty { Name = "Turizm Fakültesi", University = u1002 });
            faculties.Add(new Faculty { Name = "Tıp Fakültesi", University = u1002 });
            faculties.Add(new Faculty { Name = "İktisadi ve İdari Bilimler Fakültesi", University = u1002 });
            faculties.Add(new Faculty { Name = "İlahiyat Fakültesi", University = u1002 });
        }

        // Afyon Kocatepe Üniversitesi
        if (uniLookup.TryGetValue("Afyon Kocatepe Üniversitesi", out var u1004))
        {
            faculties.Add(new Faculty { Name = "Bolvadin Uygulamalı Bilimler Fakültesi", University = u1004 });
            faculties.Add(new Faculty { Name = "Dinar Uygulamalı Bilimler Yüksekokulu", University = u1004 });
            faculties.Add(new Faculty { Name = "Eğitim Fakültesi", University = u1004 });
            faculties.Add(new Faculty { Name = "Fen-Edebiyat Fakültesi", University = u1004 });
            faculties.Add(new Faculty { Name = "Güzel Sanatlar Fakültesi", University = u1004 });
            faculties.Add(new Faculty { Name = "Hukuk Fakültesi", University = u1004 });
            faculties.Add(new Faculty { Name = "Mühendislik Fakültesi", University = u1004 });
            faculties.Add(new Faculty { Name = "Sandıklı Uygulamalı Bilimler Yüksekokulu", University = u1004 });
            faculties.Add(new Faculty { Name = "Teknoloji Fakültesi", University = u1004 });
            faculties.Add(new Faculty { Name = "Turizm Fakültesi", University = u1004 });
            faculties.Add(new Faculty { Name = "Veteriner Fakültesi", University = u1004 });
            faculties.Add(new Faculty { Name = "İktisadi ve İdari Bilimler Fakültesi", University = u1004 });
            faculties.Add(new Faculty { Name = "İlahiyat Fakültesi", University = u1004 });
        }

        // Afyonkarahisar Sağlık Bilimleri Üniversitesi
        if (uniLookup.TryGetValue("Afyonkarahisar Sağlık Bilimleri Üniversitesi", out var u1126))
        {
            faculties.Add(new Faculty { Name = "Diş Hekimliği Fakültesi", University = u1126 });
            faculties.Add(new Faculty { Name = "Eczacılık Fakültesi", University = u1126 });
            faculties.Add(new Faculty { Name = "Sağlık Bilimleri Fakültesi", University = u1126 });
            faculties.Add(new Faculty { Name = "Tıp Fakültesi", University = u1126 });
        }

        // Ağrı İbrahim Çeçen Üniversitesi
        if (uniLookup.TryGetValue("Ağrı İbrahim Çeçen Üniversitesi", out var u1005))
        {
            faculties.Add(new Faculty { Name = "Eczacılık Fakültesi", University = u1005 });
            faculties.Add(new Faculty { Name = "Eğitim Fakültesi", University = u1005 });
            faculties.Add(new Faculty { Name = "Fen-Edebiyat Fakültesi", University = u1005 });
            faculties.Add(new Faculty { Name = "Patnos Sosyal Hizmetler Yüksekokulu", University = u1005 });
            faculties.Add(new Faculty { Name = "Sağlık Bilimleri Fakültesi", University = u1005 });
            faculties.Add(new Faculty { Name = "Spor Bilimleri Fakültesi", University = u1005 });
            faculties.Add(new Faculty { Name = "Turizm İşletmeciliği ve Otelcilik Yüksekokulu", University = u1005 });
            faculties.Add(new Faculty { Name = "Tıp Fakültesi", University = u1005 });
            faculties.Add(new Faculty { Name = "İktisadi ve İdari Bilimler Fakültesi", University = u1005 });
            faculties.Add(new Faculty { Name = "İslami İlimler Fakültesi", University = u1005 });
        }

        // Akdeniz Üniversitesi
        if (uniLookup.TryGetValue("Akdeniz Üniversitesi", out var u1007))
        {
            faculties.Add(new Faculty { Name = "Diş Hekimliği Fakültesi", University = u1007 });
            faculties.Add(new Faculty { Name = "Edebiyat Fakültesi", University = u1007 });
            faculties.Add(new Faculty { Name = "Eğitim Fakültesi", University = u1007 });
            faculties.Add(new Faculty { Name = "Fen Fakültesi", University = u1007 });
            faculties.Add(new Faculty { Name = "Hemşirelik Fakültesi", University = u1007 });
            faculties.Add(new Faculty { Name = "Hukuk Fakültesi", University = u1007 });
            faculties.Add(new Faculty { Name = "Kemer Denizcilik Fakültesi", University = u1007 });
            faculties.Add(new Faculty { Name = "Kumluca Sağlık Bilimleri Fakültesi", University = u1007 });
            faculties.Add(new Faculty { Name = "Manavgat Sosyal ve Beşeri Bilimler Fakültesi", University = u1007 });
            faculties.Add(new Faculty { Name = "Manavgat Turizm Fakültesi", University = u1007 });
            faculties.Add(new Faculty { Name = "Manavgat Yabancı Diller Fakültesi", University = u1007 });
            faculties.Add(new Faculty { Name = "Mimarlık Fakültesi", University = u1007 });
            faculties.Add(new Faculty { Name = "Mühendislik Fakültesi", University = u1007 });
            faculties.Add(new Faculty { Name = "Sağlık Bilimleri Fakültesi", University = u1007 });
            faculties.Add(new Faculty { Name = "Serik İşletme Fakültesi", University = u1007 });
            faculties.Add(new Faculty { Name = "Spor Bilimleri Fakültesi", University = u1007 });
            faculties.Add(new Faculty { Name = "Su Ürünleri Fakültesi", University = u1007 });
            faculties.Add(new Faculty { Name = "Turizm Fakültesi", University = u1007 });
            faculties.Add(new Faculty { Name = "Tıp Fakültesi", University = u1007 });
            faculties.Add(new Faculty { Name = "Uygulamalı Bilimler Fakültesi", University = u1007 });
            faculties.Add(new Faculty { Name = "Ziraat Fakültesi", University = u1007 });
            faculties.Add(new Faculty { Name = "İktisadi ve İdari Bilimler Fakültesi", University = u1007 });
            faculties.Add(new Faculty { Name = "İlahiyat Fakültesi", University = u1007 });
            faculties.Add(new Faculty { Name = "İletişim Fakültesi", University = u1007 });
        }

        // Aksaray Üniversitesi
        if (uniLookup.TryGetValue("Aksaray Üniversitesi", out var u1008))
        {
            faculties.Add(new Faculty { Name = "Diş Hekimliği Fakültesi", University = u1008 });
            faculties.Add(new Faculty { Name = "Eğitim Fakültesi", University = u1008 });
            faculties.Add(new Faculty { Name = "Fen-Edebiyat Fakültesi", University = u1008 });
            faculties.Add(new Faculty { Name = "Mimarlık ve Tasarım Fakültesi", University = u1008 });
            faculties.Add(new Faculty { Name = "Mühendislik Fakültesi", University = u1008 });
            faculties.Add(new Faculty { Name = "Sağlık Bilimleri Fakültesi", University = u1008 });
            faculties.Add(new Faculty { Name = "Spor Bilimleri Fakültesi", University = u1008 });
            faculties.Add(new Faculty { Name = "Turizm Fakültesi", University = u1008 });
            faculties.Add(new Faculty { Name = "Tıp Fakültesi", University = u1008 });
            faculties.Add(new Faculty { Name = "Veteriner Fakültesi", University = u1008 });
            faculties.Add(new Faculty { Name = "İktisadi ve İdari Bilimler Fakültesi", University = u1008 });
            faculties.Add(new Faculty { Name = "İletişim Fakültesi", University = u1008 });
            faculties.Add(new Faculty { Name = "İslami İlimler Fakültesi", University = u1008 });
        }

        // Alanya Alaaddin Keykubat Üniversitesi
        if (uniLookup.TryGetValue("Alanya Alaaddin Keykubat Üniversitesi", out var u1105))
        {
            faculties.Add(new Faculty { Name = "Diş Hekimliği Fakültesi", University = u1105 });
            faculties.Add(new Faculty { Name = "Eğitim Fakültesi", University = u1105 });
            faculties.Add(new Faculty { Name = "Gazipaşa Havacılık ve Uzay Bilimleri Fakültesi", University = u1105 });
            faculties.Add(new Faculty { Name = "Rafet Kayış Mühendislik Fakültesi", University = u1105 });
            faculties.Add(new Faculty { Name = "Sanat, Tasarım ve Mimarlık Fakültesi", University = u1105 });
            faculties.Add(new Faculty { Name = "Sağlık Bilimleri Fakültesi", University = u1105 });
            faculties.Add(new Faculty { Name = "Turizm Fakültesi", University = u1105 });
            faculties.Add(new Faculty { Name = "Tıp Fakültesi", University = u1105 });
            faculties.Add(new Faculty { Name = "İktisadi, İdari ve Sosyal Bilimler Fakültesi", University = u1105 });
        }

        // Alanya Haedreddin Barbaros Üniversitesi
        if (uniLookup.TryGetValue("Alanya Haedreddin Barbaros Üniversitesi", out var u2083))
        {
            faculties.Add(new Faculty { Name = "Mühendislik ve Mimarlık Fakültesi", University = u2083 });
            faculties.Add(new Faculty { Name = "Sosyal ve Beşeri Bilimler Fakültesi", University = u2083 });
            faculties.Add(new Faculty { Name = "Tarım ve Doğa Bilimleri Fakültesi", University = u2083 });
        }

        // Alanya Üniversitesi
        if (uniLookup.TryGetValue("Alanya Üniversitesi", out var u2101))
        {
            faculties.Add(new Faculty { Name = "Mühendislik ve Doğa Bilimleri Fakültesi", University = u2101 });
            faculties.Add(new Faculty { Name = "Sanat ve Tasarım Fakültesi", University = u2101 });
            faculties.Add(new Faculty { Name = "Sağlık Bilimleri Fakültesi", University = u2101 });
            faculties.Add(new Faculty { Name = "İktisadi, İdari ve Sosyal Bilimler Fakültesi", University = u2101 });
        }

        // Altınbaş Üniversitesi
        if (uniLookup.TryGetValue("Altınbaş Üniversitesi", out var u2029))
        {
            faculties.Add(new Faculty { Name = "Diş Hekimliği Fakültesi", University = u2029 });
            faculties.Add(new Faculty { Name = "Eczacılık Fakültesi", University = u2029 });
            faculties.Add(new Faculty { Name = "Hukuk Fakültesi", University = u2029 });
            faculties.Add(new Faculty { Name = "Mühendislik ve Mimarlık Fakültesi", University = u2029 });
            faculties.Add(new Faculty { Name = "Sağlık Bilimleri Fakültesi", University = u2029 });
            faculties.Add(new Faculty { Name = "Tıp Fakültesi", University = u2029 });
            faculties.Add(new Faculty { Name = "Uygulamalı Bilimler Fakültesi", University = u2029 });
            faculties.Add(new Faculty { Name = "İktisadi, İdari ve Sosyal Bilimler Fakültesi", University = u2029 });
            faculties.Add(new Faculty { Name = "İletişim Fakültesi", University = u2029 });
            faculties.Add(new Faculty { Name = "İşletme Fakültesi", University = u2029 });
        }

        // Amasya Üniversitesi
        if (uniLookup.TryGetValue("Amasya Üniversitesi", out var u1009))
        {
            faculties.Add(new Faculty { Name = "Eğitim Fakültesi", University = u1009 });
            faculties.Add(new Faculty { Name = "Fen-Edebiyat Fakültesi", University = u1009 });
            faculties.Add(new Faculty { Name = "Hattat Hamdullah Güzel Sanatlar Fakültesi", University = u1009 });
            faculties.Add(new Faculty { Name = "Merzifon İktisadi ve İdari Bilimler Fakültesi", University = u1009 });
            faculties.Add(new Faculty { Name = "Mimarlık Fakültesi", University = u1009 });
            faculties.Add(new Faculty { Name = "Mühendislik Fakültesi", University = u1009 });
            faculties.Add(new Faculty { Name = "Sağlık Bilimleri Fakültesi", University = u1009 });
            faculties.Add(new Faculty { Name = "Tıp Fakültesi", University = u1009 });
            faculties.Add(new Faculty { Name = "İlahiyat Fakültesi", University = u1009 });
        }

        // Anadolu Üniversitesi
        if (uniLookup.TryGetValue("Anadolu Üniversitesi", out var u1010))
        {
            faculties.Add(new Faculty { Name = "Açıköğretim Fakültesi", University = u1010 });
            faculties.Add(new Faculty { Name = "Eczacılık Fakültesi", University = u1010 });
            faculties.Add(new Faculty { Name = "Edebiyat Fakültesi", University = u1010 });
            faculties.Add(new Faculty { Name = "Eğitim Fakültesi", University = u1010 });
            faculties.Add(new Faculty { Name = "Güzel Sanatlar Fakültesi", University = u1010 });
            faculties.Add(new Faculty { Name = "Hukuk Fakültesi", University = u1010 });
            faculties.Add(new Faculty { Name = "Sağlık Bilimleri Fakültesi", University = u1010 });
            faculties.Add(new Faculty { Name = "Turizm Fakültesi", University = u1010 });
            faculties.Add(new Faculty { Name = "İktisadi ve İdari Bilimler Fakültesi", University = u1010 });
            faculties.Add(new Faculty { Name = "İletişim Bilimleri Fakültesi", University = u1010 });
        }

        // Ankara Bilim Üniversitesi
        if (uniLookup.TryGetValue("Ankara Bilim Üniversitesi", out var u2095))
        {
            faculties.Add(new Faculty { Name = "Güzel Sanatlar ve Tasarım Fakültesi", University = u2095 });
            faculties.Add(new Faculty { Name = "Hukuk Fakültesi", University = u2095 });
            faculties.Add(new Faculty { Name = "Mühendislik ve Mimarlık Fakültesi", University = u2095 });
            faculties.Add(new Faculty { Name = "İnsan ve Toplum Bilimleri Fakültesi", University = u2095 });
        }

        // Ankara Hacı Bayram Veli Üniversitesi
        if (uniLookup.TryGetValue("Ankara Hacı Bayram Veli Üniversitesi", out var u1117))
        {
            faculties.Add(new Faculty { Name = "Edebiyat Fakültesi", University = u1117 });
            faculties.Add(new Faculty { Name = "Finansal Bilimler Fakültesi", University = u1117 });
            faculties.Add(new Faculty { Name = "Güzel Sanatlar Fakültesi", University = u1117 });
            faculties.Add(new Faculty { Name = "Hukuk Fakültesi", University = u1117 });
            faculties.Add(new Faculty { Name = "Polatlı Fen-Edebiyat Fakültesi", University = u1117 });
            faculties.Add(new Faculty { Name = "Sanat ve Tasarım Fakültesi", University = u1117 });
            faculties.Add(new Faculty { Name = "Tapu Kadastro Yüksekokulu", University = u1117 });
            faculties.Add(new Faculty { Name = "Turizm Fakültesi", University = u1117 });
            faculties.Add(new Faculty { Name = "İktisadi ve İdari Bilimler Fakültesi", University = u1117 });
            faculties.Add(new Faculty { Name = "İlahiyat Fakültesi", University = u1117 });
            faculties.Add(new Faculty { Name = "İletişim Fakültesi", University = u1117 });
        }

        // Ankara Medipol Üniversitesi
        if (uniLookup.TryGetValue("Ankara Medipol Üniversitesi", out var u2092))
        {
            faculties.Add(new Faculty { Name = "Diş Hekimliği Fakültesi", University = u2092 });
            faculties.Add(new Faculty { Name = "Eczacılık Fakültesi", University = u2092 });
            faculties.Add(new Faculty { Name = "Güzel Sanatlar Tasarım ve Mimarlık Fakültesi", University = u2092 });
            faculties.Add(new Faculty { Name = "Hukuk Fakültesi", University = u2092 });
            faculties.Add(new Faculty { Name = "Mühendislik ve Doğa Bilimleri Fakültesi", University = u2092 });
            faculties.Add(new Faculty { Name = "Sağlık Bilimleri Fakültesi", University = u2092 });
            faculties.Add(new Faculty { Name = "Siyasal Bilgiler Fakültesi", University = u2092 });
            faculties.Add(new Faculty { Name = "Tıp Fakültesi", University = u2092 });
            faculties.Add(new Faculty { Name = "İktisadi, İdari ve Sosyal Bilimler Fakültesi", University = u2092 });
            faculties.Add(new Faculty { Name = "İletişim Fakültesi", University = u2092 });
        }

        // Ankara Müzik ve Güzel Sanatlar Üniversitesi
        if (uniLookup.TryGetValue("Ankara Müzik ve Güzel Sanatlar Üniversitesi", out var u1128))
        {
            faculties.Add(new Faculty { Name = "Sanat ve Tasarım Fakültesi", University = u1128 });
        }

        // Ankara Sosyal Bilimler Üniversitesi
        if (uniLookup.TryGetValue("Ankara Sosyal Bilimler Üniversitesi", out var u1109))
        {
            faculties.Add(new Faculty { Name = "Hukuk Fakültesi", University = u1109 });
            faculties.Add(new Faculty { Name = "Siyasal Bilgiler Fakültesi", University = u1109 });
            faculties.Add(new Faculty { Name = "Sosyal ve Beşeri Bilimler Fakültesi", University = u1109 });
            faculties.Add(new Faculty { Name = "Yabancı Diller Fakültesi", University = u1109 });
            faculties.Add(new Faculty { Name = "İlahiyat Fakültesi", University = u1109 });
            faculties.Add(new Faculty { Name = "İletişim Fakültesi", University = u1109 });
        }

        // Ankara Uluslararası Atatürk Alevi Bektaşi Üniversitesi
        if (uniLookup.TryGetValue("Ankara Uluslararası Atatürk Alevi Bektaşi Üniversitesi", out var u2104))
        {
            faculties.Add(new Faculty { Name = "Beden Eğitimi ve Spor Yüksekokulu", University = u2104 });
            faculties.Add(new Faculty { Name = "Diş Hekimliği Fakültesi", University = u2104 });
            faculties.Add(new Faculty { Name = "Mühendislik-Mimarlık Fakültesi", University = u2104 });
            faculties.Add(new Faculty { Name = "Sanat ve Tasarım Fakültesi", University = u2104 });
            faculties.Add(new Faculty { Name = "Sağlık Bilimleri Fakültesi", University = u2104 });
            faculties.Add(new Faculty { Name = "Sivil Havacılık Yüksekokulu", University = u2104 });
            faculties.Add(new Faculty { Name = "Tıp Fakültesi", University = u2104 });
            faculties.Add(new Faculty { Name = "Uygulamalı Bilimler Yüksekokulu", University = u2104 });
            faculties.Add(new Faculty { Name = "İktisadi, İdari ve Sosyal Bilimler Fakültesi", University = u2104 });
        }

        // Ankara Üniversitesi
        if (uniLookup.TryGetValue("Ankara Üniversitesi", out var u1011))
        {
            faculties.Add(new Faculty { Name = "Açık ve Uzaktan Eğitim Fakültesi", University = u1011 });
            faculties.Add(new Faculty { Name = "Dil ve Tarih Coğrafya Fakültesi", University = u1011 });
            faculties.Add(new Faculty { Name = "Diş Hekimliği Fakültesi", University = u1011 });
            faculties.Add(new Faculty { Name = "Eczacılık Fakültesi", University = u1011 });
            faculties.Add(new Faculty { Name = "Eğitim Bilimleri Fakültesi", University = u1011 });
            faculties.Add(new Faculty { Name = "Fen Fakültesi", University = u1011 });
            faculties.Add(new Faculty { Name = "Güzel Sanatlar Fakültesi", University = u1011 });
            faculties.Add(new Faculty { Name = "Hemşirelik Fakültesi", University = u1011 });
            faculties.Add(new Faculty { Name = "Hukuk Fakültesi", University = u1011 });
            faculties.Add(new Faculty { Name = "Mühendislik Fakültesi", University = u1011 });
            faculties.Add(new Faculty { Name = "Sağlık Bilimleri Fakültesi", University = u1011 });
            faculties.Add(new Faculty { Name = "Siyasal Bilgiler Fakültesi", University = u1011 });
            faculties.Add(new Faculty { Name = "Spor Bilimleri Fakültesi", University = u1011 });
            faculties.Add(new Faculty { Name = "Tıp Fakültesi", University = u1011 });
            faculties.Add(new Faculty { Name = "Uygulamalı Bilimler Fakültesi", University = u1011 });
            faculties.Add(new Faculty { Name = "Veteriner Fakültesi", University = u1011 });
            faculties.Add(new Faculty { Name = "Ziraat Fakültesi", University = u1011 });
            faculties.Add(new Faculty { Name = "İlahiyat Fakültesi", University = u1011 });
            faculties.Add(new Faculty { Name = "İletişim Fakültesi", University = u1011 });
        }

        // Ankara Yıldırım Beyazıt Üniversitesi
        if (uniLookup.TryGetValue("Ankara Yıldırım Beyazıt Üniversitesi", out var u1100))
        {
            faculties.Add(new Faculty { Name = "Diş Hekimliği Fakültesi", University = u1100 });
            faculties.Add(new Faculty { Name = "Havacılık ve Uzay Bilimleri Fakültesi", University = u1100 });
            faculties.Add(new Faculty { Name = "Hukuk Fakültesi", University = u1100 });
            faculties.Add(new Faculty { Name = "Mimarlık ve Güzel Sanatlar Fakültesi", University = u1100 });
            faculties.Add(new Faculty { Name = "Mühendislik ve Doğa Bilimleri Fakültesi", University = u1100 });
            faculties.Add(new Faculty { Name = "Sağlık Bilimleri Fakültesi", University = u1100 });
            faculties.Add(new Faculty { Name = "Siyasal Bilgiler Fakültesi", University = u1100 });
            faculties.Add(new Faculty { Name = "Spor Bilimleri Fakültesi", University = u1100 });
            faculties.Add(new Faculty { Name = "Tıp Fakültesi", University = u1100 });
            faculties.Add(new Faculty { Name = "İlahiyat Fakültesi", University = u1100 });
            faculties.Add(new Faculty { Name = "İletişim Fakültesi", University = u1100 });
            faculties.Add(new Faculty { Name = "İnsan ve Toplum Bilimleri Fakültesi", University = u1100 });
            faculties.Add(new Faculty { Name = "İşletme Fakültesi", University = u1100 });
            faculties.Add(new Faculty { Name = "Şereflikoçhisar Uygulamalı Bilimler Fakültesi", University = u1100 });
        }

        // Antalya Belek Üniversitesi
        if (uniLookup.TryGetValue("Antalya Belek Üniversitesi", out var u2102))
        {
            faculties.Add(new Faculty { Name = "Mühendislik-Mimarlık Fakültesi", University = u2102 });
            faculties.Add(new Faculty { Name = "Sanat ve Tasarım Fakültesi", University = u2102 });
            faculties.Add(new Faculty { Name = "İktisadi ve İdari Bilimler Fakültesi", University = u2102 });
            faculties.Add(new Faculty { Name = "İnsani Bilimler Fakültesi", University = u2102 });
        }

        // Antalya Bilim Üniversitesi
        if (uniLookup.TryGetValue("Antalya Bilim Üniversitesi", out var u2064))
        {
            faculties.Add(new Faculty { Name = "Diş Hekimliği Fakültesi", University = u2064 });
            faculties.Add(new Faculty { Name = "Güzel Sanatlar ve Mimarlık Fakültesi", University = u2064 });
            faculties.Add(new Faculty { Name = "Hukuk Fakültesi", University = u2064 });
            faculties.Add(new Faculty { Name = "Mühendislik ve Doğa Bilimleri Fakültesi", University = u2064 });
            faculties.Add(new Faculty { Name = "Sağlık Bilimleri Fakültesi", University = u2064 });
            faculties.Add(new Faculty { Name = "Sivil Havacılık Yüksekokulu", University = u2064 });
            faculties.Add(new Faculty { Name = "Turizm Fakültesi", University = u2064 });
            faculties.Add(new Faculty { Name = "İktisadi, İdari ve Sosyal Bilimler Fakültesi", University = u2064 });
        }

        // Ardahan Üniversitesi
        if (uniLookup.TryGetValue("Ardahan Üniversitesi", out var u1012))
        {
            faculties.Add(new Faculty { Name = "Mühendislik Fakültesi", University = u1012 });
            faculties.Add(new Faculty { Name = "Sağlık Bilimleri Fakültesi", University = u1012 });
            faculties.Add(new Faculty { Name = "Spor Bilimleri Fakültesi", University = u1012 });
            faculties.Add(new Faculty { Name = "Turizm İşletmeciliği ve Otelcilik Yüksekokulu", University = u1012 });
            faculties.Add(new Faculty { Name = "İktisadi ve İdari Bilimler Fakültesi", University = u1012 });
            faculties.Add(new Faculty { Name = "İlahiyat Fakültesi", University = u1012 });
            faculties.Add(new Faculty { Name = "İnsani Bilimler ve Edebiyat Fakültesi", University = u1012 });
        }

        // Artvin Çoruh Üniversitesi
        if (uniLookup.TryGetValue("Artvin Çoruh Üniversitesi", out var u1013))
        {
            faculties.Add(new Faculty { Name = "Eğitim Fakültesi", University = u1013 });
            faculties.Add(new Faculty { Name = "Fen-Edebiyat Fakültesi", University = u1013 });
            faculties.Add(new Faculty { Name = "Hopa İktisadi ve İdari Bilimler Fakültesi", University = u1013 });
            faculties.Add(new Faculty { Name = "Mühendislik Fakültesi", University = u1013 });
            faculties.Add(new Faculty { Name = "Orman Fakültesi", University = u1013 });
            faculties.Add(new Faculty { Name = "Sağlık Bilimleri Fakültesi", University = u1013 });
            faculties.Add(new Faculty { Name = "Spor Bilimleri Fakültesi", University = u1013 });
            faculties.Add(new Faculty { Name = "Turizm Fakültesi", University = u1013 });
            faculties.Add(new Faculty { Name = "İşletme Fakültesi", University = u1013 });
        }

        // Atatürk Üniversitesi
        if (uniLookup.TryGetValue("Atatürk Üniversitesi", out var u1014))
        {
            faculties.Add(new Faculty { Name = "Açık ve Uzaktan Öğretim Fakültesi", University = u1014 });
            faculties.Add(new Faculty { Name = "Diş Hekimliği Fakültesi", University = u1014 });
            faculties.Add(new Faculty { Name = "Eczacılık Fakültesi", University = u1014 });
            faculties.Add(new Faculty { Name = "Edebiyat Fakültesi", University = u1014 });
            faculties.Add(new Faculty { Name = "Fen Fakültesi", University = u1014 });
            faculties.Add(new Faculty { Name = "Güzel Sanatlar Fakültesi", University = u1014 });
            faculties.Add(new Faculty { Name = "Hemşirelik Fakültesi", University = u1014 });
            faculties.Add(new Faculty { Name = "Hukuk Fakültesi", University = u1014 });
            faculties.Add(new Faculty { Name = "Kazım Karabekir Eğitim Fakültesi", University = u1014 });
            faculties.Add(new Faculty { Name = "Mimarlık ve Tasarım Fakültesi", University = u1014 });
            faculties.Add(new Faculty { Name = "Mühendislik Fakültesi", University = u1014 });
            faculties.Add(new Faculty { Name = "Oltu Beşeri ve Sosyal Bilimler Fakültesi", University = u1014 });
            faculties.Add(new Faculty { Name = "Sağlık Bilimleri Fakültesi", University = u1014 });
            faculties.Add(new Faculty { Name = "Spor Bilimleri Fakültesi", University = u1014 });
            faculties.Add(new Faculty { Name = "Su Ürünleri Fakültesi", University = u1014 });
            faculties.Add(new Faculty { Name = "Turizm Fakültesi", University = u1014 });
            faculties.Add(new Faculty { Name = "Tıp Fakültesi", University = u1014 });
            faculties.Add(new Faculty { Name = "Veteriner Fakültesi", University = u1014 });
            faculties.Add(new Faculty { Name = "Ziraat Fakültesi", University = u1014 });
            faculties.Add(new Faculty { Name = "İktisadi ve İdari Bilimler Fakültesi", University = u1014 });
            faculties.Add(new Faculty { Name = "İlahiyat Fakültesi", University = u1014 });
            faculties.Add(new Faculty { Name = "İletişim Fakültesi", University = u1014 });
        }

        // Atılım Üniversitesi
        if (uniLookup.TryGetValue("Atılım Üniversitesi", out var u2002))
        {
            faculties.Add(new Faculty { Name = "Fen-Edebiyat Fakültesi", University = u2002 });
            faculties.Add(new Faculty { Name = "Güzel Sanatlar Tasarım ve Mimarlık Fakültesi", University = u2002 });
            faculties.Add(new Faculty { Name = "Hukuk Fakültesi", University = u2002 });
            faculties.Add(new Faculty { Name = "Mühendislik Fakültesi", University = u2002 });
            faculties.Add(new Faculty { Name = "Sağlık Bilimleri Fakültesi", University = u2002 });
            faculties.Add(new Faculty { Name = "Sivil Havacılık Yüksekokulu", University = u2002 });
            faculties.Add(new Faculty { Name = "Tıp Fakültesi", University = u2002 });
            faculties.Add(new Faculty { Name = "İşletme Fakültesi", University = u2002 });
        }

        // Avrasya Üniversitesi
        if (uniLookup.TryGetValue("Avrasya Üniversitesi", out var u2003))
        {
            faculties.Add(new Faculty { Name = "Fen-Edebiyat Fakültesi", University = u2003 });
            faculties.Add(new Faculty { Name = "Mühendislik ve Mimarlık Fakültesi", University = u2003 });
            faculties.Add(new Faculty { Name = "Sağlık Bilimleri Fakültesi", University = u2003 });
            faculties.Add(new Faculty { Name = "Spor Bilimleri Fakültesi", University = u2003 });
            faculties.Add(new Faculty { Name = "Uygulamalı Bilimler Yüksekokulu", University = u2003 });
            faculties.Add(new Faculty { Name = "İktisadi ve İdari Bilimler Fakültesi", University = u2003 });
            faculties.Add(new Faculty { Name = "İletişim Fakültesi", University = u2003 });
        }

        // Aydın Adnan Menderes Üniversitesi
        if (uniLookup.TryGetValue("Aydın Adnan Menderes Üniversitesi", out var u1003))
        {
            faculties.Add(new Faculty { Name = "Diş Hekimliği Fakültesi", University = u1003 });
            faculties.Add(new Faculty { Name = "Eğitim Fakültesi", University = u1003 });
            faculties.Add(new Faculty { Name = "Fen Fakültesi", University = u1003 });
            faculties.Add(new Faculty { Name = "Hemşirelik Fakültesi", University = u1003 });
            faculties.Add(new Faculty { Name = "Mühendislik Fakültesi", University = u1003 });
            faculties.Add(new Faculty { Name = "Nazilli İktisadi ve İdari Bilimler Fakültesi", University = u1003 });
            faculties.Add(new Faculty { Name = "Sağlık Bilimleri Fakültesi", University = u1003 });
            faculties.Add(new Faculty { Name = "Siyasal Bilgiler Fakültesi", University = u1003 });
            faculties.Add(new Faculty { Name = "Spor Bilimleri Fakültesi", University = u1003 });
            faculties.Add(new Faculty { Name = "Söke İşletme Fakültesi", University = u1003 });
            faculties.Add(new Faculty { Name = "Tıp Fakültesi", University = u1003 });
            faculties.Add(new Faculty { Name = "Veteriner Fakültesi", University = u1003 });
            faculties.Add(new Faculty { Name = "Ziraat Fakültesi", University = u1003 });
            faculties.Add(new Faculty { Name = "İlahiyat Fakültesi", University = u1003 });
            faculties.Add(new Faculty { Name = "İletişim Fakültesi", University = u1003 });
            faculties.Add(new Faculty { Name = "İnsan ve Toplum Bilimleri Fakültesi", University = u1003 });
        }

        // Bahçeşehir Üniversitesi
        if (uniLookup.TryGetValue("Bahçeşehir Üniversitesi", out var u1015))
        {
            faculties.Add(new Faculty { Name = "Burhaniye Uygulamalı Bilimler Fakültesi", University = u1015 });
            faculties.Add(new Faculty { Name = "Edremit Sivil Havacılık Yüksekokulu", University = u1015 });
            faculties.Add(new Faculty { Name = "Fen-Edebiyat Fakültesi", University = u1015 });
            faculties.Add(new Faculty { Name = "Hukuk Fakültesi", University = u1015 });
            faculties.Add(new Faculty { Name = "Mimarlık Fakültesi", University = u1015 });
            faculties.Add(new Faculty { Name = "Mühendislik Fakültesi", University = u1015 });
            faculties.Add(new Faculty { Name = "Necatibey Eğitim Fakültesi", University = u1015 });
            faculties.Add(new Faculty { Name = "Sağlık Bilimleri Fakültesi", University = u1015 });
            faculties.Add(new Faculty { Name = "Spor Bilimleri Fakültesi", University = u1015 });
            faculties.Add(new Faculty { Name = "Turizm Fakültesi", University = u1015 });
            faculties.Add(new Faculty { Name = "Tıp Fakültesi", University = u1015 });
            faculties.Add(new Faculty { Name = "Veteriner Fakültesi", University = u1015 });
            faculties.Add(new Faculty { Name = "İktisadi ve İdari Bilimler Fakültesi", University = u1015 });
            faculties.Add(new Faculty { Name = "İlahiyat Fakültesi", University = u1015 });
        }

        // Balıkesır Üniversitesi
        if (uniLookup.TryGetValue("Balıkesır Üniversitesi", out var u1016))
        {
            faculties.Add(new Faculty { Name = "Bartın Orman Fakültesi", University = u1016 });
            faculties.Add(new Faculty { Name = "Edebiyat Fakültesi", University = u1016 });
            faculties.Add(new Faculty { Name = "Eğitim Fakültesi", University = u1016 });
            faculties.Add(new Faculty { Name = "Fen Fakültesi", University = u1016 });
            faculties.Add(new Faculty { Name = "Mühendislik, Mimarlık ve Tasarım Fakültesi", University = u1016 });
            faculties.Add(new Faculty { Name = "Sağlık Bilimleri Fakültesi", University = u1016 });
            faculties.Add(new Faculty { Name = "Spor Bilimleri Fakültesi", University = u1016 });
            faculties.Add(new Faculty { Name = "İktisadi ve İdari Bilimler Fakültesi", University = u1016 });
            faculties.Add(new Faculty { Name = "İlahiyat Fakültesi", University = u1016 });
        }

        // Bandırma Onyedi Eylül Üniversitesi
        if (uniLookup.TryGetValue("Bandırma Onyedi Eylül Üniversitesi", out var u1017))
        {
            faculties.Add(new Faculty { Name = "Diş Hekimliği Fakültesi", University = u1017 });
            faculties.Add(new Faculty { Name = "Fen-Edebiyat Fakültesi", University = u1017 });
            faculties.Add(new Faculty { Name = "Mühendislik-Mimarlık Fakültesi", University = u1017 });
            faculties.Add(new Faculty { Name = "Sağlık Bilimleri Fakültesi", University = u1017 });
            faculties.Add(new Faculty { Name = "Spor Bilimleri Fakültesi", University = u1017 });
            faculties.Add(new Faculty { Name = "Turizm Fakültesi", University = u1017 });
            faculties.Add(new Faculty { Name = "İktisadi ve İdari Bilimler Fakültesi", University = u1017 });
            faculties.Add(new Faculty { Name = "İslami İlimler Fakültesi", University = u1017 });
        }

        // Bartın Üniversitesi
        if (uniLookup.TryGetValue("Bartın Üniversitesi", out var u1018))
        {
            faculties.Add(new Faculty { Name = "Eğitim Fakültesi", University = u1018 });
            faculties.Add(new Faculty { Name = "Mühendislik Fakültesi", University = u1018 });
            faculties.Add(new Faculty { Name = "Sanat ve Tasarım Fakültesi", University = u1018 });
            faculties.Add(new Faculty { Name = "Sağlık Bilimleri Fakültesi", University = u1018 });
            faculties.Add(new Faculty { Name = "Spor Bilimleri Fakültesi", University = u1018 });
            faculties.Add(new Faculty { Name = "Uygulamalı Bilimler Fakültesi", University = u1018 });
            faculties.Add(new Faculty { Name = "İktisadi ve İdari Bilimler Fakültesi", University = u1018 });
            faculties.Add(new Faculty { Name = "İlahiyat Fakültesi", University = u1018 });
            faculties.Add(new Faculty { Name = "İnsan ve Toplum Bilimleri Fakültesi", University = u1018 });
        }

        // Başkent Üniversitesi
        if (uniLookup.TryGetValue("Başkent Üniversitesi", out var u2006))
        {
            faculties.Add(new Faculty { Name = "Diş Hekimliği Fakültesi", University = u2006 });
            faculties.Add(new Faculty { Name = "Eczacılık Fakültesi", University = u2006 });
            faculties.Add(new Faculty { Name = "Eğitim Fakültesi", University = u2006 });
            faculties.Add(new Faculty { Name = "Fen-Edebiyat Fakültesi", University = u2006 });
            faculties.Add(new Faculty { Name = "Güzel Sanatlar Tasarım ve Mimarlık Fakültesi", University = u2006 });
            faculties.Add(new Faculty { Name = "Hukuk Fakültesi", University = u2006 });
            faculties.Add(new Faculty { Name = "Mühendislik Fakültesi", University = u2006 });
            faculties.Add(new Faculty { Name = "Sağlık Bilimleri Fakültesi", University = u2006 });
            faculties.Add(new Faculty { Name = "Ticari Bilimler Fakültesi", University = u2006 });
            faculties.Add(new Faculty { Name = "Tıp Fakültesi", University = u2006 });
            faculties.Add(new Faculty { Name = "İktisadi ve İdari Bilimler Fakültesi", University = u2006 });
            faculties.Add(new Faculty { Name = "İletişim Fakültesi", University = u2006 });
        }

        // Batman Üniversitesi
        if (uniLookup.TryGetValue("Batman Üniversitesi", out var u1019))
        {
            faculties.Add(new Faculty { Name = "Diş Hekimliği Fakültesi", University = u1019 });
            faculties.Add(new Faculty { Name = "Fen Fakültesi", University = u1019 });
            faculties.Add(new Faculty { Name = "Güzel Sanatlar ve Tasarım Fakültesi", University = u1019 });
            faculties.Add(new Faculty { Name = "Mühendislik Fakültesi", University = u1019 });
            faculties.Add(new Faculty { Name = "Sağlık Bilimleri Fakültesi", University = u1019 });
            faculties.Add(new Faculty { Name = "Tıp Fakültesi", University = u1019 });
            faculties.Add(new Faculty { Name = "Uygulamalı Bilimler Fakültesi", University = u1019 });
            faculties.Add(new Faculty { Name = "Ziraat ve Doğa Bilimleri Fakültesi", University = u1019 });
            faculties.Add(new Faculty { Name = "İktisadi ve İdari Bilimler Fakültesi", University = u1019 });
            faculties.Add(new Faculty { Name = "İlahiyat Fakültesi", University = u1019 });
            faculties.Add(new Faculty { Name = "İnsan ve Toplum Bilimleri Fakültesi", University = u1019 });
        }

        // Bayburt Üniversitesi
        if (uniLookup.TryGetValue("Bayburt Üniversitesi", out var u1020))
        {
            faculties.Add(new Faculty { Name = "Diş Hekimliği Fakültesi", University = u1020 });
            faculties.Add(new Faculty { Name = "Fen-Edebiyat Fakültesi", University = u1020 });
            faculties.Add(new Faculty { Name = "Fizik Tedavi ve Rehabilitasyon Fakültesi", University = u1020 });
            faculties.Add(new Faculty { Name = "Mühendislik-Mimarlık Fakültesi", University = u1020 });
            faculties.Add(new Faculty { Name = "Sağlık Bilimleri Fakültesi", University = u1020 });
            faculties.Add(new Faculty { Name = "Spor Bilimleri Fakültesi", University = u1020 });
            faculties.Add(new Faculty { Name = "Veteriner Fakültesi", University = u1020 });
            faculties.Add(new Faculty { Name = "Ziraat Fakültesi", University = u1020 });
            faculties.Add(new Faculty { Name = "İktisadi ve İdari Bilimler Fakültesi", University = u1020 });
            faculties.Add(new Faculty { Name = "İlahiyat Fakültesi", University = u1020 });
        }

        // Beykentüniversitesi
        if (uniLookup.TryGetValue("Beykentüniversitesi", out var u2084))
        {
            faculties.Add(new Faculty { Name = "Diş Hekimliği Fakültesi", University = u2084 });
            faculties.Add(new Faculty { Name = "Eczacılık Fakültesi", University = u2084 });
            faculties.Add(new Faculty { Name = "Güzel Sanatlar Tasarım ve Mimarlık Fakültesi", University = u2084 });
            faculties.Add(new Faculty { Name = "Mühendislik ve Doğa Bilimleri Fakültesi", University = u2084 });
            faculties.Add(new Faculty { Name = "Sağlık Bilimleri Fakültesi", University = u2084 });
            faculties.Add(new Faculty { Name = "Tıp Fakültesi", University = u2084 });
            faculties.Add(new Faculty { Name = "İktisadi, İdari ve Sosyal Bilimler Fakültesi", University = u2084 });
            faculties.Add(new Faculty { Name = "İletişim Fakültesi", University = u2084 });
            faculties.Add(new Faculty { Name = "İnsan ve Toplum Bilimleri Fakültesi", University = u2084 });
        }

        // Beykoz Üniversitesi
        if (uniLookup.TryGetValue("Beykoz Üniversitesi", out var u2008))
        {
            faculties.Add(new Faculty { Name = "Mühendislik ve Mimarlık Fakültesi", University = u2008 });
            faculties.Add(new Faculty { Name = "Sanat ve Tasarım Fakültesi", University = u2008 });
            faculties.Add(new Faculty { Name = "Sivil Havacılık Yüksekokulu", University = u2008 });
            faculties.Add(new Faculty { Name = "Sosyal Bilimler Fakültesi", University = u2008 });
            faculties.Add(new Faculty { Name = "İşletme ve Yönetim Bilimleri Fakültesi", University = u2008 });
        }

        // Bezm-i Âlem Vakıf Üniversitesi
        if (uniLookup.TryGetValue("Bezm-i Âlem Vakıf Üniversitesi", out var u2009))
        {
            faculties.Add(new Faculty { Name = "Diş Hekimliği Fakültesi", University = u2009 });
            faculties.Add(new Faculty { Name = "Eczacılık Fakültesi", University = u2009 });
            faculties.Add(new Faculty { Name = "Sağlık Bilimleri Fakültesi", University = u2009 });
            faculties.Add(new Faculty { Name = "Tıp Fakültesi", University = u2009 });
        }

        // Bezmiâlem Vakıf Üniversitesi
        if (uniLookup.TryGetValue("Bezmiâlem Vakıf Üniversitesi", out var u1021))
        {
            faculties.Add(new Faculty { Name = "Fen-Edebiyat Fakültesi", University = u1021 });
            faculties.Add(new Faculty { Name = "Güzel Sanatlar Fakültesi", University = u1021 });
            faculties.Add(new Faculty { Name = "Mühendislik-Mimarlık Fakültesi", University = u1021 });
            faculties.Add(new Faculty { Name = "Sağlık Bilimleri Fakültesi", University = u1021 });
            faculties.Add(new Faculty { Name = "İktisadi ve İdari Bilimler Fakültesi", University = u1021 });
            faculties.Add(new Faculty { Name = "İlahiyat Fakültesi", University = u1021 });
        }

        // Bilecik Şeyh Edebali Üniversitesi
        if (uniLookup.TryGetValue("Bilecik Şeyh Edebali Üniversitesi", out var u1022))
        {
            faculties.Add(new Faculty { Name = "Eğitim Fakültesi", University = u1022 });
            faculties.Add(new Faculty { Name = "Fen Fakültesi", University = u1022 });
            faculties.Add(new Faculty { Name = "Hukuk Fakültesi", University = u1022 });
            faculties.Add(new Faculty { Name = "Mühendislik Fakültesi", University = u1022 });
            faculties.Add(new Faculty { Name = "İktisadi ve İdari Bilimler Fakültesi", University = u1022 });
            faculties.Add(new Faculty { Name = "İnsan ve Toplum Bilimleri Fakültesi", University = u1022 });
        }

        // Bilkent Üniversitesi
        if (uniLookup.TryGetValue("Bilkent Üniversitesi", out var u1023))
        {
            faculties.Add(new Faculty { Name = "Akdağmadeni Sağlık Yüksekokulu", University = u1023 });
            faculties.Add(new Faculty { Name = "Diş Hekimliği Fakültesi", University = u1023 });
            faculties.Add(new Faculty { Name = "Eğitim Fakültesi", University = u1023 });
            faculties.Add(new Faculty { Name = "Fen-Edebiyat Fakültesi", University = u1023 });
            faculties.Add(new Faculty { Name = "Hukuk Fakültesi", University = u1023 });
            faculties.Add(new Faculty { Name = "Mühendislik-Mimarlık Fakültesi", University = u1023 });
            faculties.Add(new Faculty { Name = "Sarıkaya Fizyoterapi ve Rehabilitasyon Yüksekokulu", University = u1023 });
            faculties.Add(new Faculty { Name = "Sağlık Bilimleri Fakültesi", University = u1023 });
            faculties.Add(new Faculty { Name = "Spor Bilimleri Fakültesi", University = u1023 });
            faculties.Add(new Faculty { Name = "Tıp Fakültesi", University = u1023 });
            faculties.Add(new Faculty { Name = "Veteriner Fakültesi", University = u1023 });
            faculties.Add(new Faculty { Name = "Ziraat Fakültesi", University = u1023 });
            faculties.Add(new Faculty { Name = "İktisadi ve İdari Bilimler Fakültesi", University = u1023 });
            faculties.Add(new Faculty { Name = "İlahiyat Fakültesi", University = u1023 });
            faculties.Add(new Faculty { Name = "İletişim Fakültesi", University = u1023 });
        }

        // Bingöl Üniversitesi
        if (uniLookup.TryGetValue("Bingöl Üniversitesi", out var u1024))
        {
            faculties.Add(new Faculty { Name = "Denizcilik Fakültesi", University = u1024 });
            faculties.Add(new Faculty { Name = "Mimarlık ve Tasarım Fakültesi", University = u1024 });
            faculties.Add(new Faculty { Name = "Mühendislik ve Doğa Bilimleri Fakültesi", University = u1024 });
            faculties.Add(new Faculty { Name = "Orman Fakültesi", University = u1024 });
            faculties.Add(new Faculty { Name = "İnsan ve Toplum Bilimleri Fakültesi", University = u1024 });
        }

        // Biruni Üniversitesi
        if (uniLookup.TryGetValue("Biruni Üniversitesi", out var u2076))
        {
            faculties.Add(new Faculty { Name = "Diş Hekimliği Fakültesi", University = u2076 });
            faculties.Add(new Faculty { Name = "Eczacılık Fakültesi", University = u2076 });
            faculties.Add(new Faculty { Name = "Eğitim Fakültesi", University = u2076 });
            faculties.Add(new Faculty { Name = "Mühendislik ve Doğa Bilimleri Fakültesi", University = u2076 });
            faculties.Add(new Faculty { Name = "Sağlık Bilimleri Fakültesi", University = u2076 });
            faculties.Add(new Faculty { Name = "Tıp Fakültesi", University = u2076 });
            faculties.Add(new Faculty { Name = "Uygulamalı Bilimler Fakültesi", University = u2076 });
        }

        // Bitlis Eren Üniversitesi
        if (uniLookup.TryGetValue("Bitlis Eren Üniversitesi", out var u1025))
        {
            faculties.Add(new Faculty { Name = "Bilgisayar ve Bilişim Bilimleri Fakültesi", University = u1025 });
            faculties.Add(new Faculty { Name = "Güzel Sanatlar Tasarım ve Mimarlık Fakültesi", University = u1025 });
            faculties.Add(new Faculty { Name = "Hasan Ferdi Turgutlu Teknoloji Fakültesi", University = u1025 });
            faculties.Add(new Faculty { Name = "Mühendislik ve Doğa Bilimleri Fakültesi", University = u1025 });
            faculties.Add(new Faculty { Name = "Salihli İktisadi ve İdari Bilimler Fakültesi", University = u1025 });
            faculties.Add(new Faculty { Name = "Sağlık Bilimleri Fakültesi", University = u1025 });
            faculties.Add(new Faculty { Name = "Spor Bilimleri Fakültesi", University = u1025 });
            faculties.Add(new Faculty { Name = "Tütün Eksperliği Yüksekokulu", University = u1025 });
            faculties.Add(new Faculty { Name = "Tıp Fakültesi", University = u1025 });
            faculties.Add(new Faculty { Name = "Uygulamalı Bilimler Fakültesi", University = u1025 });
            faculties.Add(new Faculty { Name = "İktisadi ve İdari Bilimler Fakültesi", University = u1025 });
            faculties.Add(new Faculty { Name = "İlahiyat Fakültesi", University = u1025 });
            faculties.Add(new Faculty { Name = "İletişim Fakültesi", University = u1025 });
            faculties.Add(new Faculty { Name = "İnsan ve Toplum Bilimleri Fakültesi", University = u1025 });
            faculties.Add(new Faculty { Name = "İşletme Fakültesi", University = u1025 });
        }

        // Boğaziçi Üniversitesi
        if (uniLookup.TryGetValue("Boğaziçi Üniversitesi", out var u1026))
        {
            faculties.Add(new Faculty { Name = "Diş Hekimliği Fakültesi", University = u1026 });
            faculties.Add(new Faculty { Name = "Eczacılık Fakültesi", University = u1026 });
            faculties.Add(new Faculty { Name = "Edebiyat Fakültesi", University = u1026 });
            faculties.Add(new Faculty { Name = "Eğitim Fakültesi", University = u1026 });
            faculties.Add(new Faculty { Name = "Fen Fakültesi", University = u1026 });
            faculties.Add(new Faculty { Name = "Hukuk Fakültesi", University = u1026 });
            faculties.Add(new Faculty { Name = "Mimarlık, Güzel Sanatlar ve Tasarım Fakültesi", University = u1026 });
            faculties.Add(new Faculty { Name = "Mühendislik Fakültesi", University = u1026 });
            faculties.Add(new Faculty { Name = "Sağlık Bilimleri Fakültesi", University = u1026 });
            faculties.Add(new Faculty { Name = "Spor Bilimleri Fakültesi", University = u1026 });
            faculties.Add(new Faculty { Name = "Suşehri Sağlık Yüksekokulu", University = u1026 });
            faculties.Add(new Faculty { Name = "Teknoloji Fakültesi", University = u1026 });
            faculties.Add(new Faculty { Name = "Turizm Fakültesi", University = u1026 });
            faculties.Add(new Faculty { Name = "Tıp Fakültesi", University = u1026 });
            faculties.Add(new Faculty { Name = "Veteriner Fakültesi", University = u1026 });
            faculties.Add(new Faculty { Name = "Zara Veysel Dursun Uygulamalı Bilimler Yüksekokulu", University = u1026 });
            faculties.Add(new Faculty { Name = "İktisadi ve İdari Bilimler Fakültesi", University = u1026 });
            faculties.Add(new Faculty { Name = "İlahiyat Fakültesi", University = u1026 });
            faculties.Add(new Faculty { Name = "İletişim Fakültesi", University = u1026 });
            faculties.Add(new Faculty { Name = "Şarkışla Uygulamalı Bilimler Yüksekokulu", University = u1026 });
        }

        // Bolu Abant İzzet Baysal Üniversitesi
        if (uniLookup.TryGetValue("Bolu Abant İzzet Baysal Üniversitesi", out var u1027))
        {
            faculties.Add(new Faculty { Name = "Biga Uygulamalı Bilimler Fakültesi", University = u1027 });
            faculties.Add(new Faculty { Name = "Biga İktisadi ve İdari Bilimler Fakültesi", University = u1027 });
            faculties.Add(new Faculty { Name = "Deniz Bilimleri ve Teknolojisi Fakültesi", University = u1027 });
            faculties.Add(new Faculty { Name = "Diş Hekimliği Fakültesi", University = u1027 });
            faculties.Add(new Faculty { Name = "Eğitim Fakültesi", University = u1027 });
            faculties.Add(new Faculty { Name = "Fen Fakültesi", University = u1027 });
            faculties.Add(new Faculty { Name = "Gökçeada Uygulamalı Bilimler Yüksekokulu", University = u1027 });
            faculties.Add(new Faculty { Name = "Mimarlık ve Tasarım Fakültesi", University = u1027 });
            faculties.Add(new Faculty { Name = "Mühendislik Fakültesi", University = u1027 });
            faculties.Add(new Faculty { Name = "Sağlık Bilimleri Fakültesi", University = u1027 });
            faculties.Add(new Faculty { Name = "Siyasal Bilgiler Fakültesi", University = u1027 });
            faculties.Add(new Faculty { Name = "Spor Bilimleri Fakültesi", University = u1027 });
            faculties.Add(new Faculty { Name = "Turizm Fakültesi", University = u1027 });
            faculties.Add(new Faculty { Name = "Tıp Fakültesi", University = u1027 });
            faculties.Add(new Faculty { Name = "Ziraat Fakültesi", University = u1027 });
            faculties.Add(new Faculty { Name = "Çan Uygulamalı Bilimler Fakültesi", University = u1027 });
            faculties.Add(new Faculty { Name = "Çanakkale Uygulamalı Bilimler Fakültesi", University = u1027 });
            faculties.Add(new Faculty { Name = "İlahiyat Fakültesi", University = u1027 });
            faculties.Add(new Faculty { Name = "İletişim Fakültesi", University = u1027 });
            faculties.Add(new Faculty { Name = "İnsan ve Toplum Bilimleri Fakültesi", University = u1027 });
        }

        // Bursa Teknik Üniversitesi
        if (uniLookup.TryGetValue("Bursa Teknik Üniversitesi", out var u1028))
        {
            faculties.Add(new Faculty { Name = "Diş Hekimliği Fakültesi", University = u1028 });
            faculties.Add(new Faculty { Name = "Fen Fakültesi", University = u1028 });
            faculties.Add(new Faculty { Name = "Hukuk Fakültesi", University = u1028 });
            faculties.Add(new Faculty { Name = "Mühendislik Fakültesi", University = u1028 });
            faculties.Add(new Faculty { Name = "Orman Fakültesi", University = u1028 });
            faculties.Add(new Faculty { Name = "Sanat, Tasarım ve Mimarlık Fakültesi", University = u1028 });
            faculties.Add(new Faculty { Name = "Sağlık Bilimleri Fakültesi", University = u1028 });
            faculties.Add(new Faculty { Name = "İktisadi ve İdari Bilimler Fakültesi", University = u1028 });
            faculties.Add(new Faculty { Name = "İnsan ve Toplum Bilimleri Fakültesi", University = u1028 });
            faculties.Add(new Faculty { Name = "İslami İlimler Fakültesi", University = u1028 });
        }

        // Bursa Uludağ Üniversitesi
        if (uniLookup.TryGetValue("Bursa Uludağ Üniversitesi", out var u1029))
        {
            faculties.Add(new Faculty { Name = "Ceyhan Mühendislik Fakültesi", University = u1029 });
            faculties.Add(new Faculty { Name = "Ceyhan Veteriner Fakültesi", University = u1029 });
            faculties.Add(new Faculty { Name = "Diş Hekimliği Fakültesi", University = u1029 });
            faculties.Add(new Faculty { Name = "Eczacılık Fakültesi", University = u1029 });
            faculties.Add(new Faculty { Name = "Eğitim Fakültesi", University = u1029 });
            faculties.Add(new Faculty { Name = "Fen-Edebiyat Fakültesi", University = u1029 });
            faculties.Add(new Faculty { Name = "Güzel Sanatlar Fakültesi", University = u1029 });
            faculties.Add(new Faculty { Name = "Hukuk Fakültesi", University = u1029 });
            faculties.Add(new Faculty { Name = "Kozan İşletme Fakültesi", University = u1029 });
            faculties.Add(new Faculty { Name = "Mimarlık Fakültesi", University = u1029 });
            faculties.Add(new Faculty { Name = "Mühendislik Fakültesi", University = u1029 });
            faculties.Add(new Faculty { Name = "Sağlık Bilimleri Fakültesi", University = u1029 });
            faculties.Add(new Faculty { Name = "Spor Bilimleri Fakültesi", University = u1029 });
            faculties.Add(new Faculty { Name = "Su Ürünleri Fakültesi", University = u1029 });
            faculties.Add(new Faculty { Name = "Tıp Fakültesi", University = u1029 });
            faculties.Add(new Faculty { Name = "Ziraat Fakültesi", University = u1029 });
            faculties.Add(new Faculty { Name = "İktisadi ve İdari Bilimler Fakültesi", University = u1029 });
            faculties.Add(new Faculty { Name = "İlahiyat Fakültesi", University = u1029 });
            faculties.Add(new Faculty { Name = "İletişim Fakültesi", University = u1029 });
        }

        // Cumhuriyet Üniversitesi
        if (uniLookup.TryGetValue("Cumhuriyet Üniversitesi", out var u1032))
        {
            faculties.Add(new Faculty { Name = "Eğitim Fakültesi", University = u1032 });
            faculties.Add(new Faculty { Name = "Fen-Edebiyat Fakültesi", University = u1032 });
            faculties.Add(new Faculty { Name = "Güzel Sanatlar Fakültesi", University = u1032 });
            faculties.Add(new Faculty { Name = "Kütahya Uygulamalı Bilimler Fakültesi", University = u1032 });
            faculties.Add(new Faculty { Name = "Mimarlık Fakültesi", University = u1032 });
            faculties.Add(new Faculty { Name = "Mühendislik Fakültesi", University = u1032 });
            faculties.Add(new Faculty { Name = "Simav Teknoloji Fakültesi", University = u1032 });
            faculties.Add(new Faculty { Name = "Spor Bilimleri Fakültesi", University = u1032 });
            faculties.Add(new Faculty { Name = "Tavşanlı Uygulamalı Bilimler Fakültesi", University = u1032 });
            faculties.Add(new Faculty { Name = "İktisadi ve İdari Bilimler Fakültesi", University = u1032 });
            faculties.Add(new Faculty { Name = "İlahiyat Fakültesi", University = u1032 });
        }

        // Çağ Üniversitesi
        if (uniLookup.TryGetValue("Çağ Üniversitesi", out var u2010))
        {
            faculties.Add(new Faculty { Name = "Fen-Edebiyat Fakültesi", University = u2010 });
            faculties.Add(new Faculty { Name = "Hukuk Fakültesi", University = u2010 });
            faculties.Add(new Faculty { Name = "İktisadi ve İdari Bilimler Fakültesi", University = u2010 });
        }

        // Çanakkale Onsekiz Mart Üniversitesi
        if (uniLookup.TryGetValue("Çanakkale Onsekiz Mart Üniversitesi", out var u1030))
        {
            faculties.Add(new Faculty { Name = "Atatürk Sağlık Bilimleri Fakültesi", University = u1030 });
            faculties.Add(new Faculty { Name = "Beden Eğitimi ve Spor Yüksekokulu", University = u1030 });
            faculties.Add(new Faculty { Name = "Diş Hekimliği Fakültesi", University = u1030 });
            faculties.Add(new Faculty { Name = "Eczacılık Fakültesi", University = u1030 });
            faculties.Add(new Faculty { Name = "Fen Fakültesi", University = u1030 });
            faculties.Add(new Faculty { Name = "Hukuk Fakültesi", University = u1030 });
            faculties.Add(new Faculty { Name = "Mimarlık Fakültesi", University = u1030 });
            faculties.Add(new Faculty { Name = "Mühendislik Fakültesi", University = u1030 });
            faculties.Add(new Faculty { Name = "Sezai Karakoç Edebiyat Fakültesi", University = u1030 });
            faculties.Add(new Faculty { Name = "Sivil Havacılık Yüksekokulu", University = u1030 });
            faculties.Add(new Faculty { Name = "Tıp Fakültesi", University = u1030 });
            faculties.Add(new Faculty { Name = "Veteriner Fakültesi", University = u1030 });
            faculties.Add(new Faculty { Name = "Ziraat Fakültesi", University = u1030 });
            faculties.Add(new Faculty { Name = "Ziya Gökalp Eğitim Fakültesi", University = u1030 });
            faculties.Add(new Faculty { Name = "İktisadi ve İdari Bilimler Fakültesi", University = u1030 });
            faculties.Add(new Faculty { Name = "İlahiyat Fakültesi", University = u1030 });
            faculties.Add(new Faculty { Name = "İletişim Fakültesi", University = u1030 });
        }

        // Çankaya Üniversitesi
        if (uniLookup.TryGetValue("Çankaya Üniversitesi", out var u2011))
        {
            faculties.Add(new Faculty { Name = "Fen-Edebiyat Fakültesi", University = u2011 });
            faculties.Add(new Faculty { Name = "Hukuk Fakültesi", University = u2011 });
            faculties.Add(new Faculty { Name = "Mimarlık Fakültesi", University = u2011 });
            faculties.Add(new Faculty { Name = "Mühendislik Fakültesi", University = u2011 });
            faculties.Add(new Faculty { Name = "İktisadi ve İdari Bilimler Fakültesi", University = u2011 });
        }

        // Çankırı Karatekin Üniversitesi
        if (uniLookup.TryGetValue("Çankırı Karatekin Üniversitesi", out var u1031))
        {
            faculties.Add(new Faculty { Name = "Buca Eğitim Fakültesi", University = u1031 });
            faculties.Add(new Faculty { Name = "Denizcilik Fakültesi", University = u1031 });
            faculties.Add(new Faculty { Name = "Diş Hekimliği Fakültesi", University = u1031 });
            faculties.Add(new Faculty { Name = "Edebiyat Fakültesi", University = u1031 });
            faculties.Add(new Faculty { Name = "Fen Fakültesi", University = u1031 });
            faculties.Add(new Faculty { Name = "Fizik Tedavi ve Rehabilitasyon Fakültesi", University = u1031 });
            faculties.Add(new Faculty { Name = "Güzel Sanatlar Fakültesi", University = u1031 });
            faculties.Add(new Faculty { Name = "Hemşirelik Fakültesi", University = u1031 });
            faculties.Add(new Faculty { Name = "Hukuk Fakültesi", University = u1031 });
            faculties.Add(new Faculty { Name = "Mimarlık Fakültesi", University = u1031 });
            faculties.Add(new Faculty { Name = "Mühendislik Fakültesi", University = u1031 });
            faculties.Add(new Faculty { Name = "Necat Hepkon Spor Bilimleri Fakültesi", University = u1031 });
            faculties.Add(new Faculty { Name = "Turizm Fakültesi", University = u1031 });
            faculties.Add(new Faculty { Name = "Tıp Fakültesi", University = u1031 });
            faculties.Add(new Faculty { Name = "İktisadi ve İdari Bilimler Fakültesi", University = u1031 });
            faculties.Add(new Faculty { Name = "İlahiyat Fakültesi", University = u1031 });
            faculties.Add(new Faculty { Name = "İşletme Fakültesi", University = u1031 });
        }

        // Çukurova Üniversitesi
        if (uniLookup.TryGetValue("Çukurova Üniversitesi", out var u1033))
        {
            faculties.Add(new Faculty { Name = "Akçakoca Bey Siyasal Bilgiler Fakültesi", University = u1033 });
            faculties.Add(new Faculty { Name = "Eczacılık Fakültesi", University = u1033 });
            faculties.Add(new Faculty { Name = "Eğitim Fakültesi", University = u1033 });
            faculties.Add(new Faculty { Name = "Fen-Edebiyat Fakültesi", University = u1033 });
            faculties.Add(new Faculty { Name = "Mühendislik Fakültesi", University = u1033 });
            faculties.Add(new Faculty { Name = "Orman Fakültesi", University = u1033 });
            faculties.Add(new Faculty { Name = "Sanat, Tasarım ve Mimarlık Fakültesi", University = u1033 });
            faculties.Add(new Faculty { Name = "Sağlık Bilimleri Fakültesi", University = u1033 });
            faculties.Add(new Faculty { Name = "Spor Bilimleri Fakültesi", University = u1033 });
            faculties.Add(new Faculty { Name = "Turizm Fakültesi", University = u1033 });
            faculties.Add(new Faculty { Name = "Tıp Fakültesi", University = u1033 });
            faculties.Add(new Faculty { Name = "Ziraat Fakültesi", University = u1033 });
            faculties.Add(new Faculty { Name = "İlahiyat Fakültesi", University = u1033 });
            faculties.Add(new Faculty { Name = "İşletme Fakültesi", University = u1033 });
        }

        // Demiroğlu Bilim Üniversitesi
        if (uniLookup.TryGetValue("Demiroğlu Bilim Üniversitesi", out var u2026))
        {
            faculties.Add(new Faculty { Name = "Fen-Edebiyat Fakültesi", University = u2026 });
            faculties.Add(new Faculty { Name = "Florence Nightingale Hastanesi Hemşirelik Yüksekokulu", University = u2026 });
            faculties.Add(new Faculty { Name = "Sağlık Bilimleri Fakültesi", University = u2026 });
            faculties.Add(new Faculty { Name = "Tıp Fakültesi", University = u2026 });
            faculties.Add(new Faculty { Name = "İşletme ve Yönetim Bilimleri Fakültesi", University = u2026 });
        }

        // Dicle Üniversitesi
        if (uniLookup.TryGetValue("Dicle Üniversitesi", out var u1034))
        {
            faculties.Add(new Faculty { Name = "Bilgisayar ve Bilişim Bilimleri Fakültesi", University = u1034 });
            faculties.Add(new Faculty { Name = "Birgivi İlahiyat Fakültesi", University = u1034 });
            faculties.Add(new Faculty { Name = "Diş Hekimliği Fakültesi", University = u1034 });
            faculties.Add(new Faculty { Name = "Eczacılık Fakültesi", University = u1034 });
            faculties.Add(new Faculty { Name = "Edebiyat Fakültesi", University = u1034 });
            faculties.Add(new Faculty { Name = "Eğitim Fakültesi", University = u1034 });
            faculties.Add(new Faculty { Name = "Fen Fakültesi", University = u1034 });
            faculties.Add(new Faculty { Name = "Güzel Sanatlar Tasarım ve Mimarlık Fakültesi", University = u1034 });
            faculties.Add(new Faculty { Name = "Hemşirelik Fakültesi", University = u1034 });
            faculties.Add(new Faculty { Name = "Moda ve Tasarım Yüksekokulu", University = u1034 });
            faculties.Add(new Faculty { Name = "Mühendislik Fakültesi", University = u1034 });
            faculties.Add(new Faculty { Name = "Sağlık Bilimleri Fakültesi", University = u1034 });
            faculties.Add(new Faculty { Name = "Spor Bilimleri Fakültesi", University = u1034 });
            faculties.Add(new Faculty { Name = "Su Ürünleri Fakültesi", University = u1034 });
            faculties.Add(new Faculty { Name = "Tıp Fakültesi", University = u1034 });
            faculties.Add(new Faculty { Name = "Ziraat Fakültesi", University = u1034 });
            faculties.Add(new Faculty { Name = "Çeşme Turizm Fakültesi", University = u1034 });
            faculties.Add(new Faculty { Name = "Ödemiş Sağlık Bilimleri Fakültesi", University = u1034 });
            faculties.Add(new Faculty { Name = "İktisadi ve İdari Bilimler Fakültesi", University = u1034 });
            faculties.Add(new Faculty { Name = "İletişim Fakültesi", University = u1034 });
        }

        // Doğu Akdeniz Üniversitesi
        if (uniLookup.TryGetValue("Doğu Akdeniz Üniversitesi", out var u3001))
        {
            faculties.Add(new Faculty { Name = "Bilgisayar ve Teknoloji Yüksekokulu", University = u3001 });
            faculties.Add(new Faculty { Name = "Diş Hekimliği Fakültesi", University = u3001 });
            faculties.Add(new Faculty { Name = "Eczacılık Fakültesi", University = u3001 });
            faculties.Add(new Faculty { Name = "Eğitim Fakültesi", University = u3001 });
            faculties.Add(new Faculty { Name = "Fen ve Edebiyat Fakültesi", University = u3001 });
            faculties.Add(new Faculty { Name = "Hukuk Fakültesi", University = u3001 });
            faculties.Add(new Faculty { Name = "Mimarlık Fakültesi", University = u3001 });
            faculties.Add(new Faculty { Name = "Mühendislik Fakültesi", University = u3001 });
            faculties.Add(new Faculty { Name = "Sağlık Bilimleri Fakültesi", University = u3001 });
            faculties.Add(new Faculty { Name = "Turizm Fakültesi", University = u3001 });
            faculties.Add(new Faculty { Name = "Tıp Fakültesi", University = u3001 });
            faculties.Add(new Faculty { Name = "İletişim Fakültesi", University = u3001 });
            faculties.Add(new Faculty { Name = "İşletme ve Ekonomi Fakültesi", University = u3001 });
        }

        // Doğu Anadolu Üniversitesi
        if (uniLookup.TryGetValue("Doğu Anadolu Üniversitesi", out var u3009))
        {
            faculties.Add(new Faculty { Name = "Güzel Sanatlar Fakültesi", University = u3009 });
            faculties.Add(new Faculty { Name = "Siyasal Bilgiler Fakültesi", University = u3009 });
            faculties.Add(new Faculty { Name = "İşletme ve Ekonomi Fakültesi", University = u3009 });
        }

        // Doğuş Üniversitesi
        if (uniLookup.TryGetValue("Doğuş Üniversitesi", out var u1036))
        {
            faculties.Add(new Faculty { Name = "Ali Cavit Çelebioğlu Sivil Havacılık Yüksekokulu", University = u1036 });
            faculties.Add(new Faculty { Name = "Diş Hekimliği Fakültesi", University = u1036 });
            faculties.Add(new Faculty { Name = "Eczacılık Fakültesi", University = u1036 });
            faculties.Add(new Faculty { Name = "Eğitim Fakültesi", University = u1036 });
            faculties.Add(new Faculty { Name = "Fen-Edebiyat Fakültesi", University = u1036 });
            faculties.Add(new Faculty { Name = "Hukuk Fakültesi", University = u1036 });
            faculties.Add(new Faculty { Name = "Kemaliye Hacı Ali Akın Uygulamalı Bilimler Yüksekokulu", University = u1036 });
            faculties.Add(new Faculty { Name = "Mühendislik-Mimarlık Fakültesi", University = u1036 });
            faculties.Add(new Faculty { Name = "Sağlık Bilimleri Fakültesi", University = u1036 });
            faculties.Add(new Faculty { Name = "Spor Bilimleri Fakültesi", University = u1036 });
            faculties.Add(new Faculty { Name = "Tıp Fakültesi", University = u1036 });
            faculties.Add(new Faculty { Name = "İktisadi ve İdari Bilimler Fakültesi", University = u1036 });
            faculties.Add(new Faculty { Name = "İlahiyat Fakültesi", University = u1036 });
        }

        // Dokuz Eylül Üniversitesi
        if (uniLookup.TryGetValue("Dokuz Eylül Üniversitesi", out var u1035))
        {
            faculties.Add(new Faculty { Name = "Diş Hekimliği Fakültesi", University = u1035 });
            faculties.Add(new Faculty { Name = "Eczacılık Fakültesi", University = u1035 });
            faculties.Add(new Faculty { Name = "Edebiyat Fakültesi", University = u1035 });
            faculties.Add(new Faculty { Name = "Eğitim Fakültesi", University = u1035 });
            faculties.Add(new Faculty { Name = "Fen Fakültesi", University = u1035 });
            faculties.Add(new Faculty { Name = "Havacılık ve Uzay Bilimleri Fakültesi", University = u1035 });
            faculties.Add(new Faculty { Name = "Hukuk Fakültesi", University = u1035 });
            faculties.Add(new Faculty { Name = "Mimarlık Fakültesi", University = u1035 });
            faculties.Add(new Faculty { Name = "Mühendislik Fakültesi", University = u1035 });
            faculties.Add(new Faculty { Name = "Sağlık Bilimleri Fakültesi", University = u1035 });
            faculties.Add(new Faculty { Name = "Spor Bilimleri Fakültesi", University = u1035 });
            faculties.Add(new Faculty { Name = "Turizm Fakültesi", University = u1035 });
            faculties.Add(new Faculty { Name = "Tıp Fakültesi", University = u1035 });
            faculties.Add(new Faculty { Name = "Veteriner Fakültesi", University = u1035 });
            faculties.Add(new Faculty { Name = "Ziraat Fakültesi", University = u1035 });
            faculties.Add(new Faculty { Name = "İktisadi ve İdari Bilimler Fakültesi", University = u1035 });
            faculties.Add(new Faculty { Name = "İlahiyat Fakültesi", University = u1035 });
            faculties.Add(new Faculty { Name = "İletişim Fakültesi", University = u1035 });
        }

        // Dünya Göz Hastaneler Grubu Üniversitesi
        if (uniLookup.TryGetValue("Dünya Göz Hastaneler Grubu Üniversitesi", out var u2100))
        {
            faculties.Add(new Faculty { Name = "Mühendislik, Mimarlık ve Tasarım Fakültesi", University = u2100 });
            faculties.Add(new Faculty { Name = "Sanat ve Sosyal Bilimler Fakültesi", University = u2100 });
            faculties.Add(new Faculty { Name = "Sağlık Bilimleri Fakültesi", University = u2100 });
        }

        // Düzce Üniversitesi
        if (uniLookup.TryGetValue("Düzce Üniversitesi", out var u1037))
        {
            faculties.Add(new Faculty { Name = "Edebiyat Fakültesi", University = u1037 });
            faculties.Add(new Faculty { Name = "Fen Fakültesi", University = u1037 });
            faculties.Add(new Faculty { Name = "Mühendislik ve Mimarlık Fakültesi", University = u1037 });
            faculties.Add(new Faculty { Name = "Sağlık Bilimleri Fakültesi", University = u1037 });
            faculties.Add(new Faculty { Name = "İktisadi ve İdari Bilimler Fakültesi", University = u1037 });
        }

        // Ege Üniversitesi
        if (uniLookup.TryGetValue("Ege Üniversitesi", out var u1038))
        {
            faculties.Add(new Faculty { Name = "Diş Hekimliği Fakültesi", University = u1038 });
            faculties.Add(new Faculty { Name = "Eğitim Fakültesi", University = u1038 });
            faculties.Add(new Faculty { Name = "Fen Fakültesi", University = u1038 });
            faculties.Add(new Faculty { Name = "Hukuk Fakültesi", University = u1038 });
            faculties.Add(new Faculty { Name = "Mühendislik-Mimarlık Fakültesi", University = u1038 });
            faculties.Add(new Faculty { Name = "Sanat ve Tasarım Fakültesi", University = u1038 });
            faculties.Add(new Faculty { Name = "Sağlık Bilimleri Fakültesi", University = u1038 });
            faculties.Add(new Faculty { Name = "Turizm Fakültesi", University = u1038 });
            faculties.Add(new Faculty { Name = "Tıp Fakültesi", University = u1038 });
            faculties.Add(new Faculty { Name = "Ziraat Fakültesi", University = u1038 });
            faculties.Add(new Faculty { Name = "İktisadi ve İdari Bilimler Fakültesi", University = u1038 });
            faculties.Add(new Faculty { Name = "İlahiyat Fakültesi", University = u1038 });
            faculties.Add(new Faculty { Name = "İnsan ve Toplum Bilimleri Fakültesi", University = u1038 });
        }

        // Ercıyes Üniversitesi
        if (uniLookup.TryGetValue("Ercıyes Üniversitesi", out var u1039))
        {
            faculties.Add(new Faculty { Name = "Diş Hekimliği Fakültesi", University = u1039 });
            faculties.Add(new Faculty { Name = "Eczacılık Fakültesi", University = u1039 });
            faculties.Add(new Faculty { Name = "Eğitim Fakültesi", University = u1039 });
            faculties.Add(new Faculty { Name = "Fen Fakültesi", University = u1039 });
            faculties.Add(new Faculty { Name = "Mimarlık Fakültesi", University = u1039 });
            faculties.Add(new Faculty { Name = "Mühendislik Fakültesi", University = u1039 });
            faculties.Add(new Faculty { Name = "Sağlık Bilimleri Fakültesi", University = u1039 });
            faculties.Add(new Faculty { Name = "Sivil Havacılık Yüksekokulu", University = u1039 });
            faculties.Add(new Faculty { Name = "Spor Bilimleri Fakültesi", University = u1039 });
            faculties.Add(new Faculty { Name = "Su Ürünleri Fakültesi", University = u1039 });
            faculties.Add(new Faculty { Name = "Teknoloji Fakültesi", University = u1039 });
            faculties.Add(new Faculty { Name = "Tıp Fakültesi", University = u1039 });
            faculties.Add(new Faculty { Name = "Veteriner Fakültesi", University = u1039 });
            faculties.Add(new Faculty { Name = "İktisadi ve İdari Bilimler Fakültesi", University = u1039 });
            faculties.Add(new Faculty { Name = "İlahiyat Fakültesi", University = u1039 });
            faculties.Add(new Faculty { Name = "İletişim Fakültesi", University = u1039 });
            faculties.Add(new Faculty { Name = "İnsan ve Toplum Bilimleri Fakültesi", University = u1039 });
        }

        // Erzincan Binali Yıldırım Üniversitesi
        if (uniLookup.TryGetValue("Erzincan Binali Yıldırım Üniversitesi", out var u1040))
        {
            faculties.Add(new Faculty { Name = "Fen-Edebiyat Fakültesi", University = u1040 });
            faculties.Add(new Faculty { Name = "Hukuk Fakültesi", University = u1040 });
            faculties.Add(new Faculty { Name = "Mühendislik ve Teknoloji Fakültesi", University = u1040 });
            faculties.Add(new Faculty { Name = "İktisadi ve İdari Bilimler Fakültesi", University = u1040 });
            faculties.Add(new Faculty { Name = "İletişim Fakültesi", University = u1040 });
        }

        // Erzurum Teknik Üniversitesi
        if (uniLookup.TryGetValue("Erzurum Teknik Üniversitesi", out var u1041))
        {
            faculties.Add(new Faculty { Name = "Diş Hekimliği Fakültesi", University = u1041 });
            faculties.Add(new Faculty { Name = "Eczacılık Fakültesi", University = u1041 });
            faculties.Add(new Faculty { Name = "Fen Fakültesi", University = u1041 });
            faculties.Add(new Faculty { Name = "Gazi Eğitim Fakültesi", University = u1041 });
            faculties.Add(new Faculty { Name = "Hemşirelik Fakültesi", University = u1041 });
            faculties.Add(new Faculty { Name = "Mimarlık Fakültesi", University = u1041 });
            faculties.Add(new Faculty { Name = "Mühendislik Fakültesi", University = u1041 });
            faculties.Add(new Faculty { Name = "Sağlık Bilimleri Fakültesi", University = u1041 });
            faculties.Add(new Faculty { Name = "Spor Bilimleri Fakültesi", University = u1041 });
            faculties.Add(new Faculty { Name = "Teknoloji Fakültesi", University = u1041 });
            faculties.Add(new Faculty { Name = "Tıp Fakültesi", University = u1041 });
            faculties.Add(new Faculty { Name = "Uygulamalı Bilimler Fakültesi", University = u1041 });
        }

        // Eskişehir Osmangazi Üniversitesi
        if (uniLookup.TryGetValue("Eskişehir Osmangazi Üniversitesi", out var u1042))
        {
            faculties.Add(new Faculty { Name = "Diş Hekimliği Fakültesi", University = u1042 });
            faculties.Add(new Faculty { Name = "Fen-Edebiyat Fakültesi", University = u1042 });
            faculties.Add(new Faculty { Name = "Gaziantep Eğitim Fakültesi", University = u1042 });
            faculties.Add(new Faculty { Name = "Güzel Sanatlar Fakültesi", University = u1042 });
            faculties.Add(new Faculty { Name = "Havacılık ve Uzay Bilimleri Fakültesi", University = u1042 });
            faculties.Add(new Faculty { Name = "Hukuk Fakültesi", University = u1042 });
            faculties.Add(new Faculty { Name = "Mimarlık Fakültesi", University = u1042 });
            faculties.Add(new Faculty { Name = "Mühendislik Fakültesi", University = u1042 });
            faculties.Add(new Faculty { Name = "Nizip Eğitim Fakültesi", University = u1042 });
            faculties.Add(new Faculty { Name = "Sağlık Bilimleri Fakültesi", University = u1042 });
            faculties.Add(new Faculty { Name = "Spor Bilimleri Fakültesi", University = u1042 });
            faculties.Add(new Faculty { Name = "Turizm Fakültesi", University = u1042 });
            faculties.Add(new Faculty { Name = "Tıp Fakültesi", University = u1042 });
            faculties.Add(new Faculty { Name = "İktisadi ve İdari Bilimler Fakültesi", University = u1042 });
            faculties.Add(new Faculty { Name = "İlahiyat Fakültesi", University = u1042 });
            faculties.Add(new Faculty { Name = "İletişim Fakültesi", University = u1042 });
            faculties.Add(new Faculty { Name = "İslahiye İktisadi ve İdari Bilimler Fakültesi", University = u1042 });
        }

        // Eskişehir Teknik Üniversitesi
        if (uniLookup.TryGetValue("Eskişehir Teknik Üniversitesi", out var u1043))
        {
            faculties.Add(new Faculty { Name = "Diş Hekimliği Fakültesi", University = u1043 });
            faculties.Add(new Faculty { Name = "Eczacılık Fakültesi", University = u1043 });
            faculties.Add(new Faculty { Name = "Erbaa Sağlık Bilimleri Fakültesi", University = u1043 });
            faculties.Add(new Faculty { Name = "Erbaa Sosyal ve Beşeri Bilimler Fakültesi", University = u1043 });
            faculties.Add(new Faculty { Name = "Eğitim Fakültesi", University = u1043 });
            faculties.Add(new Faculty { Name = "Fen-Edebiyat Fakültesi", University = u1043 });
            faculties.Add(new Faculty { Name = "Güzel Sanatlar Fakültesi", University = u1043 });
            faculties.Add(new Faculty { Name = "Hukuk Fakültesi", University = u1043 });
            faculties.Add(new Faculty { Name = "Mühendislik ve Mimarlık Fakültesi", University = u1043 });
            faculties.Add(new Faculty { Name = "Niksar Uygulamalı Bilimler Fakültesi", University = u1043 });
            faculties.Add(new Faculty { Name = "Sağlık Bilimleri Fakültesi", University = u1043 });
            faculties.Add(new Faculty { Name = "Spor Bilimleri Fakültesi", University = u1043 });
            faculties.Add(new Faculty { Name = "Turhal Uygulamalı Bilimler Fakültesi", University = u1043 });
            faculties.Add(new Faculty { Name = "Tıp Fakültesi", University = u1043 });
            faculties.Add(new Faculty { Name = "Zile Dinçerler Turizm İşletmeciliği ve Otelcilik Yüksekokulu", University = u1043 });
            faculties.Add(new Faculty { Name = "Ziraat Fakültesi", University = u1043 });
            faculties.Add(new Faculty { Name = "İktisadi ve İdari Bilimler Fakültesi", University = u1043 });
            faculties.Add(new Faculty { Name = "İslami İlimler Fakültesi", University = u1043 });
        }

        // Fatih Sultan Mehmet Vakıf Üniversitesi
        if (uniLookup.TryGetValue("Fatih Sultan Mehmet Vakıf Üniversitesi", out var u2014))
        {
            faculties.Add(new Faculty { Name = "Eğitim Fakültesi", University = u2014 });
            faculties.Add(new Faculty { Name = "Hukuk Fakültesi", University = u2014 });
            faculties.Add(new Faculty { Name = "Mühendislik Fakültesi", University = u2014 });
            faculties.Add(new Faculty { Name = "Sanat, Tasarım ve Mimarlık Fakültesi", University = u2014 });
            faculties.Add(new Faculty { Name = "İktisadi ve İdari Bilimler Fakültesi", University = u2014 });
            faculties.Add(new Faculty { Name = "İnsan ve Toplum Bilimleri Fakültesi", University = u2014 });
            faculties.Add(new Faculty { Name = "İslami İlimler Fakültesi", University = u2014 });
        }

        // Fenerbahçe Üniversitesi
        if (uniLookup.TryGetValue("Fenerbahçe Üniversitesi", out var u1044))
        {
            faculties.Add(new Faculty { Name = "Havacılık ve Uzay Bilimleri Fakültesi", University = u1044 });
            faculties.Add(new Faculty { Name = "Mimarlık Fakültesi", University = u1044 });
            faculties.Add(new Faculty { Name = "Mühendislik Fakültesi", University = u1044 });
            faculties.Add(new Faculty { Name = "Temel Bilimler Fakültesi", University = u1044 });
            faculties.Add(new Faculty { Name = "İşletme Fakültesi", University = u1044 });
        }

        // Fırat Üniversitesi
        if (uniLookup.TryGetValue("Fırat Üniversitesi", out var u1045))
        {
            faculties.Add(new Faculty { Name = "Bulancak Kadir Karabaş Uygulamalı Bilimler Yüksekokulu", University = u1045 });
            faculties.Add(new Faculty { Name = "Diş Hekimliği Fakültesi", University = u1045 });
            faculties.Add(new Faculty { Name = "Eğitim Fakültesi", University = u1045 });
            faculties.Add(new Faculty { Name = "Fen-Edebiyat Fakültesi", University = u1045 });
            faculties.Add(new Faculty { Name = "Görele Güzel Sanatlar Fakültesi", University = u1045 });
            faculties.Add(new Faculty { Name = "Görele Uygulamalı Bilimler Yüksekokulu", University = u1045 });
            faculties.Add(new Faculty { Name = "Mühendislik Fakültesi", University = u1045 });
            faculties.Add(new Faculty { Name = "Sivil Havacılık Yüksekokulu", University = u1045 });
            faculties.Add(new Faculty { Name = "Tirebolu İletişim Fakültesi", University = u1045 });
            faculties.Add(new Faculty { Name = "Tıp Fakültesi", University = u1045 });
            faculties.Add(new Faculty { Name = "İktisadi ve İdari Bilimler Fakültesi", University = u1045 });
            faculties.Add(new Faculty { Name = "İlahiyat Fakültesi", University = u1045 });
            faculties.Add(new Faculty { Name = "Şebinkarahisar Uygulamalı Bilimler Yüksekokulu", University = u1045 });
        }

        // Gazi Üniversitesi
        if (uniLookup.TryGetValue("Gazi Üniversitesi", out var u1047))
        {
            faculties.Add(new Faculty { Name = "Edebiyat Fakültesi", University = u1047 });
            faculties.Add(new Faculty { Name = "Gümüşhane İktisadi ve İdari Bilimler Fakültesi", University = u1047 });
            faculties.Add(new Faculty { Name = "Mühendislik ve Doğa Bilimleri Fakültesi", University = u1047 });
            faculties.Add(new Faculty { Name = "Sağlık Bilimleri Fakültesi", University = u1047 });
            faculties.Add(new Faculty { Name = "Spor Bilimleri Fakültesi", University = u1047 });
            faculties.Add(new Faculty { Name = "Turizm Fakültesi", University = u1047 });
            faculties.Add(new Faculty { Name = "Uygulamalı Bilimler Yüksekokulu", University = u1047 });
            faculties.Add(new Faculty { Name = "İlahiyat Fakültesi", University = u1047 });
            faculties.Add(new Faculty { Name = "İletişim Fakültesi", University = u1047 });
        }

        // Gaziantep İslam Bilim ve Teknoloji Üniversitesi
        if (uniLookup.TryGetValue("Gaziantep İslam Bilim ve Teknoloji Üniversitesi", out var u1048))
        {
            faculties.Add(new Faculty { Name = "Diş Hekimliği Fakültesi", University = u1048 });
            faculties.Add(new Faculty { Name = "Eczacılık Fakültesi", University = u1048 });
            faculties.Add(new Faculty { Name = "Edebiyat Fakültesi", University = u1048 });
            faculties.Add(new Faculty { Name = "Eğitim Fakültesi", University = u1048 });
            faculties.Add(new Faculty { Name = "Fen Fakültesi", University = u1048 });
            faculties.Add(new Faculty { Name = "Fizik Tedavi ve Rehabilitasyon Fakültesi", University = u1048 });
            faculties.Add(new Faculty { Name = "Güzel Sanatlar Fakültesi", University = u1048 });
            faculties.Add(new Faculty { Name = "Hemşirelik Fakültesi", University = u1048 });
            faculties.Add(new Faculty { Name = "Hukuk Fakültesi", University = u1048 });
            faculties.Add(new Faculty { Name = "Mimarlık Fakültesi", University = u1048 });
            faculties.Add(new Faculty { Name = "Mühendislik Fakültesi", University = u1048 });
            faculties.Add(new Faculty { Name = "Sağlık Bilimleri Fakültesi", University = u1048 });
            faculties.Add(new Faculty { Name = "Tıp Fakültesi", University = u1048 });
            faculties.Add(new Faculty { Name = "İktisadi ve İdari Bilimler Fakültesi", University = u1048 });
            faculties.Add(new Faculty { Name = "İletişim Fakültesi", University = u1048 });
        }

        // Gaziantep Üniversitesi
        if (uniLookup.TryGetValue("Gaziantep Üniversitesi", out var u1049))
        {
            faculties.Add(new Faculty { Name = "Eğitim Fakültesi", University = u1049 });
            faculties.Add(new Faculty { Name = "Mühendislik Fakültesi", University = u1049 });
            faculties.Add(new Faculty { Name = "Sağlık Bilimleri Fakültesi", University = u1049 });
            faculties.Add(new Faculty { Name = "Spor Bilimleri Fakültesi", University = u1049 });
            faculties.Add(new Faculty { Name = "İktisadi ve İdari Bilimler Fakültesi", University = u1049 });
            faculties.Add(new Faculty { Name = "İlahiyat Fakültesi", University = u1049 });
        }

        // Gebze Teknik Üniversitesi
        if (uniLookup.TryGetValue("Gebze Teknik Üniversitesi", out var u1050))
        {
            faculties.Add(new Faculty { Name = "Diş Hekimliği Fakültesi", University = u1050 });
            faculties.Add(new Faculty { Name = "Eczacılık Fakültesi", University = u1050 });
            faculties.Add(new Faculty { Name = "Eğitim Fakültesi", University = u1050 });
            faculties.Add(new Faculty { Name = "Fen-Edebiyat Fakültesi", University = u1050 });
            faculties.Add(new Faculty { Name = "Güzel Sanatlar Fakültesi", University = u1050 });
            faculties.Add(new Faculty { Name = "Mühendislik Fakültesi", University = u1050 });
            faculties.Add(new Faculty { Name = "Sağlık Bilimleri Fakültesi", University = u1050 });
            faculties.Add(new Faculty { Name = "Siverek Uygulamalı Bilimler Fakültesi", University = u1050 });
            faculties.Add(new Faculty { Name = "Turizm Fakültesi", University = u1050 });
            faculties.Add(new Faculty { Name = "Tıp Fakültesi", University = u1050 });
            faculties.Add(new Faculty { Name = "Veteriner Fakültesi", University = u1050 });
            faculties.Add(new Faculty { Name = "Viranşehir Sağlık Yüksekokulu", University = u1050 });
            faculties.Add(new Faculty { Name = "Ziraat Fakültesi", University = u1050 });
            faculties.Add(new Faculty { Name = "İktisadi ve İdari Bilimler Fakültesi", University = u1050 });
            faculties.Add(new Faculty { Name = "İlahiyat Fakültesi", University = u1050 });
        }

        // Giresun Üniversitesi
        if (uniLookup.TryGetValue("Giresun Üniversitesi", out var u1051))
        {
            faculties.Add(new Faculty { Name = "Mühendislik ve Doğa Bilimleri Fakültesi", University = u1051 });
            faculties.Add(new Faculty { Name = "Sağlık Bilimleri Fakültesi", University = u1051 });
            faculties.Add(new Faculty { Name = "Spor Bilimleri Fakültesi", University = u1051 });
            faculties.Add(new Faculty { Name = "Tıp Fakültesi", University = u1051 });
            faculties.Add(new Faculty { Name = "İktisadi ve İdari Bilimler Fakültesi", University = u1051 });
            faculties.Add(new Faculty { Name = "İlahiyat Fakültesi", University = u1051 });
            faculties.Add(new Faculty { Name = "İnsan ve Toplum Bilimleri Fakültesi", University = u1051 });
        }

        // Girne Amerikan Üniversitesi
        if (uniLookup.TryGetValue("Girne Amerikan Üniversitesi", out var u3007))
        {
            faculties.Add(new Faculty { Name = "Deniz Bilimleri Fakültesi", University = u3007 });
            faculties.Add(new Faculty { Name = "Deniz İşletmeciliği ve Yönetimi Fakültesi", University = u3007 });
            faculties.Add(new Faculty { Name = "Denizcilik Fakültesi", University = u3007 });
            faculties.Add(new Faculty { Name = "Diş Hekimliği Fakültesi", University = u3007 });
            faculties.Add(new Faculty { Name = "Eğitim Fakültesi", University = u3007 });
            faculties.Add(new Faculty { Name = "Fen Edebiyat Fakültesi", University = u3007 });
            faculties.Add(new Faculty { Name = "Havacılık ve Uzay Bilimleri Fakültesi", University = u3007 });
            faculties.Add(new Faculty { Name = "Hukuk Fakültesi", University = u3007 });
            faculties.Add(new Faculty { Name = "Mimarlık Fakültesi", University = u3007 });
            faculties.Add(new Faculty { Name = "Mühendislik Fakültesi", University = u3007 });
            faculties.Add(new Faculty { Name = "Sağlık Bilimleri Fakültesi", University = u3007 });
            faculties.Add(new Faculty { Name = "Turizm Fakültesi", University = u3007 });
            faculties.Add(new Faculty { Name = "Tıp Fakültesi", University = u3007 });
            faculties.Add(new Faculty { Name = "İktisadi ve İdari Bilimler Fakültesi", University = u3007 });
        }

        // Gümüşhane Üniversitesi
        if (uniLookup.TryGetValue("Gümüşhane Üniversitesi", out var u1052))
        {
            faculties.Add(new Faculty { Name = "Diş Hekimliği Fakültesi", University = u1052 });
            faculties.Add(new Faculty { Name = "Fen-Edebiyat Fakültesi", University = u1052 });
            faculties.Add(new Faculty { Name = "Mühendislik Fakültesi", University = u1052 });
            faculties.Add(new Faculty { Name = "Sağlık Bilimleri Fakültesi", University = u1052 });
            faculties.Add(new Faculty { Name = "Spor Bilimleri Fakültesi", University = u1052 });
            faculties.Add(new Faculty { Name = "Turizm Fakültesi", University = u1052 });
            faculties.Add(new Faculty { Name = "Uygulamalı Bilimler Fakültesi", University = u1052 });
            faculties.Add(new Faculty { Name = "Ziraat Fakültesi", University = u1052 });
            faculties.Add(new Faculty { Name = "İktisadi ve İdari Bilimler Fakültesi", University = u1052 });
            faculties.Add(new Faculty { Name = "İlahiyat Fakültesi", University = u1052 });
        }

        // Hacettepe Üniversitesi
        if (uniLookup.TryGetValue("Hacettepe Üniversitesi", out var u1053))
        {
            faculties.Add(new Faculty { Name = "Diş Hekimliği Fakültesi", University = u1053 });
            faculties.Add(new Faculty { Name = "Eczacılık Fakültesi", University = u1053 });
            faculties.Add(new Faculty { Name = "Eğitim Fakültesi", University = u1053 });
            faculties.Add(new Faculty { Name = "Fen-Edebiyat Fakültesi", University = u1053 });
            faculties.Add(new Faculty { Name = "Güzel Sanatlar ve Tasarım Fakültesi", University = u1053 });
            faculties.Add(new Faculty { Name = "Hemşirelik Fakültesi", University = u1053 });
            faculties.Add(new Faculty { Name = "Hukuk Fakültesi", University = u1053 });
            faculties.Add(new Faculty { Name = "Mühendislik Fakültesi", University = u1053 });
            faculties.Add(new Faculty { Name = "Sağlık Bilimleri Fakültesi", University = u1053 });
            faculties.Add(new Faculty { Name = "Spor Bilimleri Fakültesi", University = u1053 });
            faculties.Add(new Faculty { Name = "Tıp Fakültesi", University = u1053 });
            faculties.Add(new Faculty { Name = "İktisadi ve İdari Bilimler Fakültesi", University = u1053 });
            faculties.Add(new Faculty { Name = "İlahiyat Fakültesi", University = u1053 });
            faculties.Add(new Faculty { Name = "İletişim Fakültesi", University = u1053 });
        }

        // Hakkari Üniversitesi
        if (uniLookup.TryGetValue("Hakkari Üniversitesi", out var u1054))
        {
            faculties.Add(new Faculty { Name = "Diş Hekimliği Fakültesi", University = u1054 });
            faculties.Add(new Faculty { Name = "Edebiyat Fakültesi", University = u1054 });
            faculties.Add(new Faculty { Name = "Eğitim Bilimleri Fakültesi", University = u1054 });
            faculties.Add(new Faculty { Name = "Hukuk Fakültesi", University = u1054 });
            faculties.Add(new Faculty { Name = "Mühendislik ve Doğa Bilimleri Fakültesi", University = u1054 });
            faculties.Add(new Faculty { Name = "Sanat, Tasarım ve Mimarlık Fakültesi", University = u1054 });
            faculties.Add(new Faculty { Name = "Sağlık Bilimleri Fakültesi", University = u1054 });
            faculties.Add(new Faculty { Name = "Siyasal Bilgiler Fakültesi", University = u1054 });
            faculties.Add(new Faculty { Name = "Turizm Fakültesi", University = u1054 });
            faculties.Add(new Faculty { Name = "Tıp Fakültesi", University = u1054 });
            faculties.Add(new Faculty { Name = "İlahiyat Fakültesi", University = u1054 });
        }

        // Haliç Üniversitesi
        if (uniLookup.TryGetValue("Haliç Üniversitesi", out var u2019))
        {
            faculties.Add(new Faculty { Name = "Fen-Edebiyat Fakültesi", University = u2019 });
            faculties.Add(new Faculty { Name = "Güzel Sanatlar Fakültesi", University = u2019 });
            faculties.Add(new Faculty { Name = "Mimarlık Fakültesi", University = u2019 });
            faculties.Add(new Faculty { Name = "Mühendislik Fakültesi", University = u2019 });
            faculties.Add(new Faculty { Name = "Sağlık Bilimleri Fakültesi", University = u2019 });
            faculties.Add(new Faculty { Name = "Spor Bilimleri Fakültesi", University = u2019 });
            faculties.Add(new Faculty { Name = "Tıp Fakültesi", University = u2019 });
            faculties.Add(new Faculty { Name = "İşletme Fakültesi", University = u2019 });
        }

        // Harran Üniversitesi
        if (uniLookup.TryGetValue("Harran Üniversitesi", out var u1055))
        {
            faculties.Add(new Faculty { Name = "Bilgisayar ve Bilişim Fakültesi", University = u1055 });
            faculties.Add(new Faculty { Name = "Denizcilik Fakültesi", University = u1055 });
            faculties.Add(new Faculty { Name = "Elektrik-Elektronik Fakültesi", University = u1055 });
            faculties.Add(new Faculty { Name = "Fen-Edebiyat Fakültesi", University = u1055 });
            faculties.Add(new Faculty { Name = "Gemi İnşaatı ve Deniz Bilimleri Fakültesi", University = u1055 });
            faculties.Add(new Faculty { Name = "Kimya-Metalurji Fakültesi", University = u1055 });
            faculties.Add(new Faculty { Name = "Maden Fakültesi", University = u1055 });
            faculties.Add(new Faculty { Name = "Makine Fakültesi", University = u1055 });
            faculties.Add(new Faculty { Name = "Mimarlık Fakültesi", University = u1055 });
            faculties.Add(new Faculty { Name = "Tekstil Teknolojileri ve Tasarımı Fakültesi", University = u1055 });
            faculties.Add(new Faculty { Name = "Uçak ve Uzay Bilimleri Fakültesi", University = u1055 });
            faculties.Add(new Faculty { Name = "İnşaat Fakültesi", University = u1055 });
            faculties.Add(new Faculty { Name = "İşletme Fakültesi", University = u1055 });
        }

        // Hasan Kalyoncu Üniversitesi
        if (uniLookup.TryGetValue("Hasan Kalyoncu Üniversitesi", out var u2016))
        {
            faculties.Add(new Faculty { Name = "Eğitim Fakültesi", University = u2016 });
            faculties.Add(new Faculty { Name = "Güzel Sanatlar ve Mimarlık Fakültesi", University = u2016 });
            faculties.Add(new Faculty { Name = "Havacılık ve Uzay Bilimleri Fakültesi", University = u2016 });
            faculties.Add(new Faculty { Name = "Hukuk Fakültesi", University = u2016 });
            faculties.Add(new Faculty { Name = "Mühendislik Fakültesi", University = u2016 });
            faculties.Add(new Faculty { Name = "Sağlık Bilimleri Fakültesi", University = u2016 });
            faculties.Add(new Faculty { Name = "Turizm Fakültesi", University = u2016 });
            faculties.Add(new Faculty { Name = "İktisadi, İdari ve Sosyal Bilimler Fakültesi", University = u2016 });
            faculties.Add(new Faculty { Name = "İletişim Fakültesi", University = u2016 });
        }

        // Hitit Üniversitesi
        if (uniLookup.TryGetValue("Hitit Üniversitesi", out var u1056))
        {
            faculties.Add(new Faculty { Name = "Açık ve Uzaktan Eğitim Fakültesi", University = u1056 });
            faculties.Add(new Faculty { Name = "Bilgisayar ve Bilişim Teknolojileri Fakültesi", University = u1056 });
            faculties.Add(new Faculty { Name = "Diş Hekimliği Fakültesi", University = u1056 });
            faculties.Add(new Faculty { Name = "Eczacılık Fakültesi", University = u1056 });
            faculties.Add(new Faculty { Name = "Edebiyat Fakültesi", University = u1056 });
            faculties.Add(new Faculty { Name = "Fen Fakültesi", University = u1056 });
            faculties.Add(new Faculty { Name = "Hemşirelik Fakültesi", University = u1056 });
            faculties.Add(new Faculty { Name = "Hukuk Fakültesi", University = u1056 });
            faculties.Add(new Faculty { Name = "Mimarlık Fakültesi", University = u1056 });
            faculties.Add(new Faculty { Name = "Siyasal Bilgiler Fakültesi", University = u1056 });
            faculties.Add(new Faculty { Name = "Su Bilimleri Fakültesi", University = u1056 });
            faculties.Add(new Faculty { Name = "Ulaştırma ve Lojistik Fakültesi", University = u1056 });
            faculties.Add(new Faculty { Name = "İktisat Fakültesi", University = u1056 });
            faculties.Add(new Faculty { Name = "İlahiyat Fakültesi", University = u1056 });
            faculties.Add(new Faculty { Name = "İletişim Fakültesi", University = u1056 });
            faculties.Add(new Faculty { Name = "İstanbul Tıp Fakültesi", University = u1056 });
            faculties.Add(new Faculty { Name = "İşletme Fakültesi", University = u1056 });
        }

        // Iğdır Üniversitesi
        if (uniLookup.TryGetValue("Iğdır Üniversitesi", out var u1057))
        {
            faculties.Add(new Faculty { Name = "Diş Hekimliği Fakültesi", University = u1057 });
            faculties.Add(new Faculty { Name = "Eczacılık Fakültesi", University = u1057 });
            faculties.Add(new Faculty { Name = "Gemi İnşaatı ve Denizcilik Fakültesi", University = u1057 });
            faculties.Add(new Faculty { Name = "Hukuk Fakültesi", University = u1057 });
            faculties.Add(new Faculty { Name = "Mühendislik ve Mimarlık Fakültesi", University = u1057 });
            faculties.Add(new Faculty { Name = "Orman Fakültesi", University = u1057 });
            faculties.Add(new Faculty { Name = "Sanat ve Tasarım Fakültesi", University = u1057 });
            faculties.Add(new Faculty { Name = "Sağlık Bilimleri Fakültesi", University = u1057 });
            faculties.Add(new Faculty { Name = "Sosyal ve Beşeri Bilimler Fakültesi", University = u1057 });
            faculties.Add(new Faculty { Name = "Su Ürünleri Fakültesi", University = u1057 });
            faculties.Add(new Faculty { Name = "Turizm Fakültesi", University = u1057 });
            faculties.Add(new Faculty { Name = "Tıp Fakültesi", University = u1057 });
            faculties.Add(new Faculty { Name = "İktisadi ve İdari Bilimler Fakültesi", University = u1057 });
            faculties.Add(new Faculty { Name = "İlahiyat Fakültesi", University = u1057 });
        }

        // Inonu Üniversitesi
        if (uniLookup.TryGetValue("Inonu Üniversitesi", out var u1058))
        {
            faculties.Add(new Faculty { Name = "Fen Fakültesi", University = u1058 });
            faculties.Add(new Faculty { Name = "Mimarlık Fakültesi", University = u1058 });
            faculties.Add(new Faculty { Name = "Mühendislik Fakültesi", University = u1058 });
        }

        // Işık Üniversitesi
        if (uniLookup.TryGetValue("Işık Üniversitesi", out var u2020))
        {
            faculties.Add(new Faculty { Name = "Mühendislik ve Doğa Bilimleri Fakültesi", University = u2020 });
            faculties.Add(new Faculty { Name = "Sanat, Tasarım ve Mimarlık Fakültesi", University = u2020 });
            faculties.Add(new Faculty { Name = "İktisadi, İdari ve Sosyal Bilimler Fakültesi", University = u2020 });
        }

        // İbn Haldun Üniversitesi
        if (uniLookup.TryGetValue("İbn Haldun Üniversitesi", out var u2086))
        {
            faculties.Add(new Faculty { Name = "Eğitim Bilimleri Fakültesi", University = u2086 });
            faculties.Add(new Faculty { Name = "Hukuk Fakültesi", University = u2086 });
            faculties.Add(new Faculty { Name = "Yönetim Bilimleri Fakültesi", University = u2086 });
            faculties.Add(new Faculty { Name = "İletişim Fakültesi", University = u2086 });
            faculties.Add(new Faculty { Name = "İnsan ve Toplum Bilimleri Fakültesi", University = u2086 });
            faculties.Add(new Faculty { Name = "İslami İlimler Fakültesi", University = u2086 });
        }

        // İhsan Doğramacı Bilkent Üniversitesi
        if (uniLookup.TryGetValue("İhsan Doğramacı Bilkent Üniversitesi", out var u1059))
        {
            faculties.Add(new Faculty { Name = "Dede Korkut Eğitim Fakültesi", University = u1059 });
            faculties.Add(new Faculty { Name = "Diş Hekimliği Fakültesi", University = u1059 });
            faculties.Add(new Faculty { Name = "Fen-Edebiyat Fakültesi", University = u1059 });
            faculties.Add(new Faculty { Name = "Kağızman Uygulamalı Bilimler Yüksekokulu", University = u1059 });
            faculties.Add(new Faculty { Name = "Mühendislik-Mimarlık Fakültesi", University = u1059 });
            faculties.Add(new Faculty { Name = "Sarıkamış Spor Bilimleri Fakültesi", University = u1059 });
            faculties.Add(new Faculty { Name = "Sarıkamış Turizm Fakültesi", University = u1059 });
            faculties.Add(new Faculty { Name = "Sağlık Bilimleri Fakültesi", University = u1059 });
            faculties.Add(new Faculty { Name = "Tıp Fakültesi", University = u1059 });
            faculties.Add(new Faculty { Name = "Veteriner Fakültesi", University = u1059 });
            faculties.Add(new Faculty { Name = "İktisadi ve İdari Bilimler Fakültesi", University = u1059 });
            faculties.Add(new Faculty { Name = "İlahiyat Fakültesi", University = u1059 });
        }

        // İnönü Üniversitesi
        if (uniLookup.TryGetValue("İnönü Üniversitesi", out var u1060))
        {
            faculties.Add(new Faculty { Name = "Afşin Sağlık Yüksekokulu", University = u1060 });
            faculties.Add(new Faculty { Name = "Diş Hekimliği Fakültesi", University = u1060 });
            faculties.Add(new Faculty { Name = "Eğitim Fakültesi", University = u1060 });
            faculties.Add(new Faculty { Name = "Fen Fakültesi", University = u1060 });
            faculties.Add(new Faculty { Name = "Göksun Uygulamalı Bilimler Yüksekokulu", University = u1060 });
            faculties.Add(new Faculty { Name = "Güzel Sanatlar Fakültesi", University = u1060 });
            faculties.Add(new Faculty { Name = "Mühendislik-Mimarlık Fakültesi", University = u1060 });
            faculties.Add(new Faculty { Name = "Orman Fakültesi", University = u1060 });
            faculties.Add(new Faculty { Name = "Sağlık Bilimleri Fakültesi", University = u1060 });
            faculties.Add(new Faculty { Name = "Spor Bilimleri Fakültesi", University = u1060 });
            faculties.Add(new Faculty { Name = "Tıp Fakültesi", University = u1060 });
            faculties.Add(new Faculty { Name = "Ziraat Fakültesi", University = u1060 });
            faculties.Add(new Faculty { Name = "İktisadi ve İdari Bilimler Fakültesi", University = u1060 });
            faculties.Add(new Faculty { Name = "İlahiyat Fakültesi", University = u1060 });
            faculties.Add(new Faculty { Name = "İnsan ve Toplum Bilimleri Fakültesi", University = u1060 });
        }

        // İskele Üniversitesi
        if (uniLookup.TryGetValue("İskele Üniversitesi", out var u3015))
        {
            faculties.Add(new Faculty { Name = "Mühendislik ve Mimarlık Fakültesi", University = u3015 });
            faculties.Add(new Faculty { Name = "İktisadi, İdari ve Sosyal Bilimler Fakültesi", University = u3015 });
        }

        // İskenderun Teknik Üniversitesi
        if (uniLookup.TryGetValue("İskenderun Teknik Üniversitesi", out var u1061))
        {
            faculties.Add(new Faculty { Name = "Bilgisayar ve Bilişim Bilimleri Fakültesi", University = u1061 });
            faculties.Add(new Faculty { Name = "Diş Hekimliği Fakültesi", University = u1061 });
            faculties.Add(new Faculty { Name = "Hasan Doğan Spor Bilimleri Fakültesi", University = u1061 });
            faculties.Add(new Faculty { Name = "Mühendislik ve Doğa Bilimleri Fakültesi", University = u1061 });
            faculties.Add(new Faculty { Name = "Orman Fakültesi", University = u1061 });
            faculties.Add(new Faculty { Name = "Safranbolu Başak Cengiz Mimarlık Fakültesi", University = u1061 });
            faculties.Add(new Faculty { Name = "Safranbolu Fethi Toker Güzel Sanatlar ve Tasarım Fakültesi", University = u1061 });
            faculties.Add(new Faculty { Name = "Safranbolu Turizm Fakültesi", University = u1061 });
            faculties.Add(new Faculty { Name = "Safranbolu Türker İnanoğlu İletişim Fakültesi", University = u1061 });
            faculties.Add(new Faculty { Name = "Sağlık Bilimleri Fakültesi", University = u1061 });
            faculties.Add(new Faculty { Name = "Tıp Fakültesi", University = u1061 });
            faculties.Add(new Faculty { Name = "İktisadi ve İdari Bilimler Fakültesi", University = u1061 });
            faculties.Add(new Faculty { Name = "İlahiyat Fakültesi", University = u1061 });
            faculties.Add(new Faculty { Name = "İnsan ve Toplum Bilimleri Fakültesi", University = u1061 });
            faculties.Add(new Faculty { Name = "İşletme Fakültesi", University = u1061 });
        }

        // İstanbul 29 Mayıs Üniversitesi
        if (uniLookup.TryGetValue("İstanbul 29 Mayıs Üniversitesi", out var u2022))
        {
            faculties.Add(new Faculty { Name = "Edebiyat Fakültesi", University = u2022 });
            faculties.Add(new Faculty { Name = "Eğitim Fakültesi", University = u2022 });
            faculties.Add(new Faculty { Name = "Hukuk Fakültesi", University = u2022 });
            faculties.Add(new Faculty { Name = "Mühendislik ve Doğa Bilimleri Fakültesi", University = u2022 });
            faculties.Add(new Faculty { Name = "İktisadi ve İdari Bilimler Fakültesi", University = u2022 });
            faculties.Add(new Faculty { Name = "İlahiyat Fakültesi", University = u2022 });
        }

        // İstanbul Arel Üniversitesi
        if (uniLookup.TryGetValue("İstanbul Arel Üniversitesi", out var u1062))
        {
            faculties.Add(new Faculty { Name = "Diş Hekimliği Fakültesi", University = u1062 });
            faculties.Add(new Faculty { Name = "Eczacılık Fakültesi", University = u1062 });
            faculties.Add(new Faculty { Name = "Edebiyat Fakültesi", University = u1062 });
            faculties.Add(new Faculty { Name = "Fen Fakültesi", University = u1062 });
            faculties.Add(new Faculty { Name = "Mimarlık Fakültesi", University = u1062 });
            faculties.Add(new Faculty { Name = "Mühendislik Fakültesi", University = u1062 });
            faculties.Add(new Faculty { Name = "Of Teknoloji Fakültesi", University = u1062 });
            faculties.Add(new Faculty { Name = "Orman Fakültesi", University = u1062 });
            faculties.Add(new Faculty { Name = "Sağlık Bilimleri Fakültesi", University = u1062 });
            faculties.Add(new Faculty { Name = "Sürmene Deniz Bilimleri Fakültesi", University = u1062 });
            faculties.Add(new Faculty { Name = "Tıp Fakültesi", University = u1062 });
            faculties.Add(new Faculty { Name = "İktisadi ve İdari Bilimler Fakültesi", University = u1062 });
        }

        // İstanbul Atlas Üniversitesi
        if (uniLookup.TryGetValue("İstanbul Atlas Üniversitesi", out var u1063))
        {
            faculties.Add(new Faculty { Name = "Ahmet Keleşoğlu Diş Hekimliği Fakültesi", University = u1063 });
            faculties.Add(new Faculty { Name = "Edebiyat Fakültesi", University = u1063 });
            faculties.Add(new Faculty { Name = "Eğitim Fakültesi", University = u1063 });
            faculties.Add(new Faculty { Name = "Kamil Özdağ Fen Fakültesi", University = u1063 });
            faculties.Add(new Faculty { Name = "Mühendislik Fakültesi", University = u1063 });
            faculties.Add(new Faculty { Name = "Sağlık Bilimleri Fakültesi", University = u1063 });
            faculties.Add(new Faculty { Name = "Spor Bilimleri Fakültesi", University = u1063 });
            faculties.Add(new Faculty { Name = "Tıp Fakültesi", University = u1063 });
            faculties.Add(new Faculty { Name = "Uygulamalı Bilimler Fakültesi", University = u1063 });
            faculties.Add(new Faculty { Name = "İktisadi ve İdari Bilimler Fakültesi", University = u1063 });
            faculties.Add(new Faculty { Name = "İlahiyat Fakültesi", University = u1063 });
        }

        // İstanbul Aydın Üniversitesi
        if (uniLookup.TryGetValue("İstanbul Aydın Üniversitesi", out var u1064))
        {
            faculties.Add(new Faculty { Name = "Eğitim Fakültesi", University = u1064 });
            faculties.Add(new Faculty { Name = "Fen Fakültesi", University = u1064 });
            faculties.Add(new Faculty { Name = "Mühendislik ve Mimarlık Fakültesi", University = u1064 });
            faculties.Add(new Faculty { Name = "Orman Fakültesi", University = u1064 });
            faculties.Add(new Faculty { Name = "Sağlık Bilimleri Fakültesi", University = u1064 });
            faculties.Add(new Faculty { Name = "Sivil Havacılık Yüksekokulu", University = u1064 });
            faculties.Add(new Faculty { Name = "Spor Bilimleri Fakültesi", University = u1064 });
            faculties.Add(new Faculty { Name = "Turizm Fakültesi", University = u1064 });
            faculties.Add(new Faculty { Name = "Tıp Fakültesi", University = u1064 });
            faculties.Add(new Faculty { Name = "Veteriner Fakültesi", University = u1064 });
            faculties.Add(new Faculty { Name = "İktisadi ve İdari Bilimler Fakültesi", University = u1064 });
            faculties.Add(new Faculty { Name = "İlahiyat Fakültesi", University = u1064 });
            faculties.Add(new Faculty { Name = "İletişim Fakültesi", University = u1064 });
            faculties.Add(new Faculty { Name = "İnsan ve Toplum Bilimleri Fakültesi", University = u1064 });
        }

        // İstanbul Bilgi Üniversitesi
        if (uniLookup.TryGetValue("İstanbul Bilgi Üniversitesi", out var u1066))
        {
            faculties.Add(new Faculty { Name = "Diş Hekimliği Fakültesi", University = u1066 });
            faculties.Add(new Faculty { Name = "Eğitim Fakültesi", University = u1066 });
            faculties.Add(new Faculty { Name = "Güzel Sanatlar Fakültesi", University = u1066 });
            faculties.Add(new Faculty { Name = "Hukuk Fakültesi", University = u1066 });
            faculties.Add(new Faculty { Name = "Mühendislik ve Doğa Bilimleri Fakültesi", University = u1066 });
            faculties.Add(new Faculty { Name = "Sağlık Bilimleri Fakültesi", University = u1066 });
            faculties.Add(new Faculty { Name = "Spor Bilimleri Fakültesi", University = u1066 });
            faculties.Add(new Faculty { Name = "Tıp Fakültesi", University = u1066 });
            faculties.Add(new Faculty { Name = "Veteriner Fakültesi", University = u1066 });
            faculties.Add(new Faculty { Name = "İktisadi ve İdari Bilimler Fakültesi", University = u1066 });
            faculties.Add(new Faculty { Name = "İnsan ve Toplum Bilimleri Fakültesi", University = u1066 });
            faculties.Add(new Faculty { Name = "İslami İlimler Fakültesi", University = u1066 });
        }

        // İstanbul Bilim Üniversitesi
        if (uniLookup.TryGetValue("İstanbul Bilim Üniversitesi", out var u1067))
        {
            faculties.Add(new Faculty { Name = "Fen-Edebiyat Fakültesi", University = u1067 });
            faculties.Add(new Faculty { Name = "Hukuk Fakültesi", University = u1067 });
            faculties.Add(new Faculty { Name = "Mimarlık Fakültesi", University = u1067 });
            faculties.Add(new Faculty { Name = "Mühendislik Fakültesi", University = u1067 });
            faculties.Add(new Faculty { Name = "Sağlık Bilimleri Fakültesi", University = u1067 });
            faculties.Add(new Faculty { Name = "Teknoloji Fakültesi", University = u1067 });
            faculties.Add(new Faculty { Name = "Turizm Fakültesi", University = u1067 });
            faculties.Add(new Faculty { Name = "Tıp Fakültesi", University = u1067 });
            faculties.Add(new Faculty { Name = "İktisadi ve İdari Bilimler Fakültesi", University = u1067 });
            faculties.Add(new Faculty { Name = "İlahiyat Fakültesi", University = u1067 });
        }

        // İstanbul Esenyurt Üniversitesi
        if (uniLookup.TryGetValue("İstanbul Esenyurt Üniversitesi", out var u2094))
        {
            faculties.Add(new Faculty { Name = "Diş Hekimliği Fakültesi", University = u2094 });
            faculties.Add(new Faculty { Name = "Eczacılık Fakültesi", University = u2094 });
            faculties.Add(new Faculty { Name = "Mühendislik ve Doğa Bilimleri Fakültesi", University = u2094 });
            faculties.Add(new Faculty { Name = "Sağlık Bilimleri Fakültesi", University = u2094 });
            faculties.Add(new Faculty { Name = "Tıp Fakültesi", University = u2094 });
            faculties.Add(new Faculty { Name = "İktisadi, İdari ve Sosyal Bilimler Fakültesi", University = u2094 });
        }

        // İstanbul Gelişim Üniversitesi
        if (uniLookup.TryGetValue("İstanbul Gelişim Üniversitesi", out var u1068))
        {
            faculties.Add(new Faculty { Name = "Fen Fakültesi", University = u1068 });
            faculties.Add(new Faculty { Name = "Kilisli Muallim Rıfat Eğitim Fakültesi", University = u1068 });
            faculties.Add(new Faculty { Name = "Mühendislik-Mimarlık Fakültesi", University = u1068 });
            faculties.Add(new Faculty { Name = "Yusuf Şerefoğlu Sağlık Bilimleri Fakültesi", University = u1068 });
            faculties.Add(new Faculty { Name = "Ziraat Fakültesi", University = u1068 });
            faculties.Add(new Faculty { Name = "İktisadi ve İdari Bilimler Fakültesi", University = u1068 });
            faculties.Add(new Faculty { Name = "İlahiyat Fakültesi", University = u1068 });
            faculties.Add(new Faculty { Name = "İnsan ve Toplum Bilimleri Fakültesi", University = u1068 });
        }

        // İstanbul Kent Üniversitesi
        if (uniLookup.TryGetValue("İstanbul Kent Üniversitesi", out var u1069))
        {
            faculties.Add(new Faculty { Name = "Denizcilik Fakültesi", University = u1069 });
            faculties.Add(new Faculty { Name = "Diş Hekimliği Fakültesi", University = u1069 });
            faculties.Add(new Faculty { Name = "Eğitim Fakültesi", University = u1069 });
            faculties.Add(new Faculty { Name = "Fen-Edebiyat Fakültesi", University = u1069 });
            faculties.Add(new Faculty { Name = "Havacılık ve Uzay Bilimleri Fakültesi", University = u1069 });
            faculties.Add(new Faculty { Name = "Hukuk Fakültesi", University = u1069 });
            faculties.Add(new Faculty { Name = "Mimarlık ve Tasarım Fakültesi", University = u1069 });
            faculties.Add(new Faculty { Name = "Mühendislik Fakültesi", University = u1069 });
            faculties.Add(new Faculty { Name = "Sağlık Bilimleri Fakültesi", University = u1069 });
            faculties.Add(new Faculty { Name = "Siyasal Bilgiler Fakültesi", University = u1069 });
            faculties.Add(new Faculty { Name = "Spor Bilimleri Fakültesi", University = u1069 });
            faculties.Add(new Faculty { Name = "Teknoloji Fakültesi", University = u1069 });
            faculties.Add(new Faculty { Name = "Turizm Fakültesi", University = u1069 });
            faculties.Add(new Faculty { Name = "Tıp Fakültesi", University = u1069 });
            faculties.Add(new Faculty { Name = "Ziraat Fakültesi", University = u1069 });
            faculties.Add(new Faculty { Name = "İlahiyat Fakültesi", University = u1069 });
            faculties.Add(new Faculty { Name = "İletişim Fakültesi", University = u1069 });
            faculties.Add(new Faculty { Name = "İşletme Fakültesi", University = u1069 });
        }

        // İstanbul Kültür Üniversitesi
        if (uniLookup.TryGetValue("İstanbul Kültür Üniversitesi", out var u2032))
        {
            faculties.Add(new Faculty { Name = "Eğitim Fakültesi", University = u2032 });
            faculties.Add(new Faculty { Name = "Hukuk Fakültesi", University = u2032 });
            faculties.Add(new Faculty { Name = "Mühendislik ve Doğa Bilimleri Fakültesi", University = u2032 });
            faculties.Add(new Faculty { Name = "Sağlık Bilimleri Fakültesi", University = u2032 });
            faculties.Add(new Faculty { Name = "Spor Bilimleri Fakültesi", University = u2032 });
            faculties.Add(new Faculty { Name = "İnsan ve Toplum Bilimleri Fakültesi", University = u2032 });
            faculties.Add(new Faculty { Name = "İslami İlimler Fakültesi", University = u2032 });
            faculties.Add(new Faculty { Name = "İşletme ve Yönetim Bilimleri Fakültesi", University = u2032 });
        }

        // İstanbul Medeniyet Üniversitesi
        if (uniLookup.TryGetValue("İstanbul Medeniyet Üniversitesi", out var u1070))
        {
            faculties.Add(new Faculty { Name = "Ahmet Keleşoğlu Eğitim Fakültesi", University = u1070 });
            faculties.Add(new Faculty { Name = "Ahmet Keleşoğlu İlahiyat Fakültesi", University = u1070 });
            faculties.Add(new Faculty { Name = "Diş Hekimliği Fakültesi", University = u1070 });
            faculties.Add(new Faculty { Name = "Ereğli Eğitim Fakültesi", University = u1070 });
            faculties.Add(new Faculty { Name = "Ereğli Ziraat Fakültesi", University = u1070 });
            faculties.Add(new Faculty { Name = "Fen Fakültesi", University = u1070 });
            faculties.Add(new Faculty { Name = "Güzel Sanatlar ve Mimarlık Fakültesi", University = u1070 });
            faculties.Add(new Faculty { Name = "Havacılık ve Uzay Bilimleri Fakültesi", University = u1070 });
            faculties.Add(new Faculty { Name = "Hemşirelik Fakültesi", University = u1070 });
            faculties.Add(new Faculty { Name = "Hukuk Fakültesi", University = u1070 });
            faculties.Add(new Faculty { Name = "Mühendislik Fakültesi", University = u1070 });
            faculties.Add(new Faculty { Name = "Nezahat Keleşoğlu Sağlık Bilimleri Fakültesi", University = u1070 });
            faculties.Add(new Faculty { Name = "Seydişehir Ahmet Cengiz Mühendislik Fakültesi", University = u1070 });
            faculties.Add(new Faculty { Name = "Seydişehir Kamil Akkanat Sağlık Bilimleri Fakültesi", University = u1070 });
            faculties.Add(new Faculty { Name = "Siyasal Bilgiler Fakültesi", University = u1070 });
            faculties.Add(new Faculty { Name = "Sosyal ve Beşeri Bilimler Fakültesi", University = u1070 });
            faculties.Add(new Faculty { Name = "Turizm Fakültesi", University = u1070 });
            faculties.Add(new Faculty { Name = "Tıp Fakültesi", University = u1070 });
            faculties.Add(new Faculty { Name = "Uygulamalı Bilimler Fakültesi", University = u1070 });
        }

        // İstanbul Medipol Üniversitesi
        if (uniLookup.TryGetValue("İstanbul Medipol Üniversitesi", out var u1071))
        {
            faculties.Add(new Faculty { Name = "Edebiyat Fakültesi", University = u1071 });
            faculties.Add(new Faculty { Name = "Güzel Sanatlar Fakültesi", University = u1071 });
            faculties.Add(new Faculty { Name = "Kızıltepe Tarım Bilimleri ve Teknolojileri Fakültesi", University = u1071 });
            faculties.Add(new Faculty { Name = "Midyat Sanat ve Tasarım Fakültesi", University = u1071 });
            faculties.Add(new Faculty { Name = "Mühendislik-Mimarlık Fakültesi", University = u1071 });
            faculties.Add(new Faculty { Name = "Sağlık Bilimleri Fakültesi", University = u1071 });
            faculties.Add(new Faculty { Name = "Spor Bilimleri Fakültesi", University = u1071 });
            faculties.Add(new Faculty { Name = "Turizm Fakültesi", University = u1071 });
            faculties.Add(new Faculty { Name = "Tıp Fakültesi", University = u1071 });
            faculties.Add(new Faculty { Name = "İktisadi ve İdari Bilimler Fakültesi", University = u1071 });
            faculties.Add(new Faculty { Name = "İlahiyat Fakültesi", University = u1071 });
        }

        // İstanbul Okan Üniversitesi
        if (uniLookup.TryGetValue("İstanbul Okan Üniversitesi", out var u1072))
        {
            faculties.Add(new Faculty { Name = "Atatürk Eğitim Fakültesi", University = u1072 });
            faculties.Add(new Faculty { Name = "Diş Hekimliği Fakültesi", University = u1072 });
            faculties.Add(new Faculty { Name = "Eczacılık Fakültesi", University = u1072 });
            faculties.Add(new Faculty { Name = "Fen Fakültesi", University = u1072 });
            faculties.Add(new Faculty { Name = "Finansal Bilimler Fakültesi", University = u1072 });
            faculties.Add(new Faculty { Name = "Güzel Sanatlar Fakültesi", University = u1072 });
            faculties.Add(new Faculty { Name = "Hukuk Fakültesi", University = u1072 });
            faculties.Add(new Faculty { Name = "Mimarlık ve Tasarım Fakültesi", University = u1072 });
            faculties.Add(new Faculty { Name = "Mühendislik Fakültesi", University = u1072 });
            faculties.Add(new Faculty { Name = "Sağlık Bilimleri Fakültesi", University = u1072 });
            faculties.Add(new Faculty { Name = "Siyasal Bilgiler Fakültesi", University = u1072 });
            faculties.Add(new Faculty { Name = "Spor Bilimleri Fakültesi", University = u1072 });
            faculties.Add(new Faculty { Name = "Teknoloji Fakültesi", University = u1072 });
            faculties.Add(new Faculty { Name = "Tıp Fakültesi", University = u1072 });
            faculties.Add(new Faculty { Name = "Uygulamalı Bilimler Fakültesi", University = u1072 });
            faculties.Add(new Faculty { Name = "İktisat Fakültesi", University = u1072 });
            faculties.Add(new Faculty { Name = "İlahiyat Fakültesi", University = u1072 });
            faculties.Add(new Faculty { Name = "İletişim Fakültesi", University = u1072 });
            faculties.Add(new Faculty { Name = "İnsan ve Toplum Bilimleri Fakültesi", University = u1072 });
            faculties.Add(new Faculty { Name = "İşletme Fakültesi", University = u1072 });
        }

        // İstanbul Rumelı Üniversitesi
        if (uniLookup.TryGetValue("İstanbul Rumelı Üniversitesi", out var u2037))
        {
            faculties.Add(new Faculty { Name = "Hukuk Fakültesi", University = u2037 });
            faculties.Add(new Faculty { Name = "Mühendislik ve Doğa Bilimleri Fakültesi", University = u2037 });
            faculties.Add(new Faculty { Name = "Sanat ve Tasarım Fakültesi", University = u2037 });
            faculties.Add(new Faculty { Name = "İktisadi, İdari ve Sosyal Bilimler Fakültesi", University = u2037 });
            faculties.Add(new Faculty { Name = "İletişim Fakültesi", University = u2037 });
        }

        // İstanbul Sabahattın Zaim Üniversitesi
        if (uniLookup.TryGetValue("İstanbul Sabahattın Zaim Üniversitesi", out var u1073))
        {
            faculties.Add(new Faculty { Name = "Bucak Bilgisayar ve Bilişim Fakültesi", University = u1073 });
            faculties.Add(new Faculty { Name = "Bucak Sağlık Yüksekokulu", University = u1073 });
            faculties.Add(new Faculty { Name = "Bucak Zeliha Tolunay Uygulamalı Teknoloji ve İşletmecilik Yüksekokulu", University = u1073 });
            faculties.Add(new Faculty { Name = "Bucak İşletme Fakültesi", University = u1073 });
            faculties.Add(new Faculty { Name = "Diş Hekimliği Fakültesi", University = u1073 });
            faculties.Add(new Faculty { Name = "Eğitim Fakültesi", University = u1073 });
            faculties.Add(new Faculty { Name = "Fen-Edebiyat Fakültesi", University = u1073 });
            faculties.Add(new Faculty { Name = "Gölhisar Uygulamalı Bilimler Yüksekokulu", University = u1073 });
            faculties.Add(new Faculty { Name = "Mühendislik-Mimarlık Fakültesi", University = u1073 });
            faculties.Add(new Faculty { Name = "Sanat ve Tasarım Fakültesi", University = u1073 });
            faculties.Add(new Faculty { Name = "Sağlık Bilimleri Fakültesi", University = u1073 });
            faculties.Add(new Faculty { Name = "Spor Bilimleri Fakültesi", University = u1073 });
            faculties.Add(new Faculty { Name = "Turizm Fakültesi", University = u1073 });
            faculties.Add(new Faculty { Name = "Veteriner Fakültesi", University = u1073 });
            faculties.Add(new Faculty { Name = "İktisadi ve İdari Bilimler Fakültesi", University = u1073 });
            faculties.Add(new Faculty { Name = "İlahiyat Fakültesi", University = u1073 });
        }

        // İstanbul Sabahattin Zaim Üniversitesi
        if (uniLookup.TryGetValue("İstanbul Sabahattin Zaim Üniversitesi", out var u2038))
        {
            faculties.Add(new Faculty { Name = "Bilgisayar ve Bilişim Teknolojileri Fakültesi", University = u2038 });
            faculties.Add(new Faculty { Name = "Diş Hekimliği Fakültesi", University = u2038 });
            faculties.Add(new Faculty { Name = "Mimarlık, Tasarım ve Güzel Sanatlar Fakültesi", University = u2038 });
            faculties.Add(new Faculty { Name = "Sağlık Bilimleri Yüksekokulu", University = u2038 });
            faculties.Add(new Faculty { Name = "Uygulamalı Bilimler Yüksekokulu", University = u2038 });
            faculties.Add(new Faculty { Name = "İnsan ve Toplum Bilimleri Fakültesi", University = u2038 });
        }

        // İstanbul Teknik Üniversitesi
        if (uniLookup.TryGetValue("İstanbul Teknik Üniversitesi", out var u1074))
        {
            faculties.Add(new Faculty { Name = "Denizcilik Fakültesi", University = u1074 });
            faculties.Add(new Faculty { Name = "Diş Hekimliği Fakültesi", University = u1074 });
            faculties.Add(new Faculty { Name = "Eczacılık Fakültesi", University = u1074 });
            faculties.Add(new Faculty { Name = "Erdemli Uygulamalı Teknoloji ve İşletmecilik Yüksekokulu", University = u1074 });
            faculties.Add(new Faculty { Name = "Eğitim Fakültesi", University = u1074 });
            faculties.Add(new Faculty { Name = "Fen Fakültesi", University = u1074 });
            faculties.Add(new Faculty { Name = "Hemşirelik Fakültesi", University = u1074 });
            faculties.Add(new Faculty { Name = "Mimarlık Fakültesi", University = u1074 });
            faculties.Add(new Faculty { Name = "Mühendislik Fakültesi", University = u1074 });
            faculties.Add(new Faculty { Name = "Sağlık Bilimleri Fakültesi", University = u1074 });
            faculties.Add(new Faculty { Name = "Silifke Uygulamalı Teknoloji ve İşletmecilik Yüksekokulu", University = u1074 });
            faculties.Add(new Faculty { Name = "Spor Bilimleri Fakültesi", University = u1074 });
            faculties.Add(new Faculty { Name = "Su Ürünleri Fakültesi", University = u1074 });
            faculties.Add(new Faculty { Name = "Turizm Fakültesi", University = u1074 });
            faculties.Add(new Faculty { Name = "Tıp Fakültesi", University = u1074 });
            faculties.Add(new Faculty { Name = "İktisadi ve İdari Bilimler Fakültesi", University = u1074 });
            faculties.Add(new Faculty { Name = "İlahiyat Fakültesi", University = u1074 });
            faculties.Add(new Faculty { Name = "İletişim Fakültesi", University = u1074 });
            faculties.Add(new Faculty { Name = "İnsan ve Toplum Bilimleri Fakültesi", University = u1074 });
        }

        // İstanbul Ticaret Üniversitesi
        if (uniLookup.TryGetValue("İstanbul Ticaret Üniversitesi", out var u1075))
        {
            faculties.Add(new Faculty { Name = "Fen-Edebiyat Fakültesi", University = u1075 });
            faculties.Add(new Faculty { Name = "Güzel Sanatlar Fakültesi", University = u1075 });
            faculties.Add(new Faculty { Name = "Kültür Varlıklarını Koruma ve Onarım Yüksekokulu", University = u1075 });
            faculties.Add(new Faculty { Name = "Mimarlık Fakültesi", University = u1075 });
            faculties.Add(new Faculty { Name = "İletişim Fakültesi", University = u1075 });
        }

        // İstanbul Üniversitesi
        if (uniLookup.TryGetValue("İstanbul Üniversitesi", out var u1076))
        {
            faculties.Add(new Faculty { Name = "Bodrum Güzel Sanatlar Fakültesi", University = u1076 });
            faculties.Add(new Faculty { Name = "Dalaman Sivil Havacılık Yüksekokulu", University = u1076 });
            faculties.Add(new Faculty { Name = "Diş Hekimliği Fakültesi", University = u1076 });
            faculties.Add(new Faculty { Name = "Edebiyat Fakültesi", University = u1076 });
            faculties.Add(new Faculty { Name = "Eğitim Fakültesi", University = u1076 });
            faculties.Add(new Faculty { Name = "Fen Fakültesi", University = u1076 });
            faculties.Add(new Faculty { Name = "Fethiye Sağlık Bilimleri Fakültesi", University = u1076 });
            faculties.Add(new Faculty { Name = "Fethiye Ziraat Fakültesi", University = u1076 });
            faculties.Add(new Faculty { Name = "Fethiye İşletme Fakültesi", University = u1076 });
            faculties.Add(new Faculty { Name = "Milas Veteriner Fakültesi", University = u1076 });
            faculties.Add(new Faculty { Name = "Mimarlık Fakültesi", University = u1076 });
            faculties.Add(new Faculty { Name = "Mühendislik Fakültesi", University = u1076 });
            faculties.Add(new Faculty { Name = "Sağlık Bilimleri Fakültesi", University = u1076 });
            faculties.Add(new Faculty { Name = "Seydikemer Uygulamalı Bilimler Yüksekokulu", University = u1076 });
            faculties.Add(new Faculty { Name = "Spor Bilimleri Fakültesi", University = u1076 });
            faculties.Add(new Faculty { Name = "Su Ürünleri Fakültesi", University = u1076 });
            faculties.Add(new Faculty { Name = "Teknoloji Fakültesi", University = u1076 });
            faculties.Add(new Faculty { Name = "Turizm Fakültesi", University = u1076 });
            faculties.Add(new Faculty { Name = "Tıp Fakültesi", University = u1076 });
            faculties.Add(new Faculty { Name = "İktisadi ve İdari Bilimler Fakültesi", University = u1076 });
            faculties.Add(new Faculty { Name = "İlahiyat Fakültesi", University = u1076 });
        }

        // İstanbul Üniversitesi-cerrahpaşa
        if (uniLookup.TryGetValue("İstanbul Üniversitesi-cerrahpaşa", out var u1077))
        {
            faculties.Add(new Faculty { Name = "Diş Hekimliği Fakültesi", University = u1077 });
            faculties.Add(new Faculty { Name = "Eğitim Fakültesi", University = u1077 });
            faculties.Add(new Faculty { Name = "Fen-Edebiyat Fakültesi", University = u1077 });
            faculties.Add(new Faculty { Name = "Mimarlık Fakültesi", University = u1077 });
            faculties.Add(new Faculty { Name = "Sağlık Bilimleri Fakültesi", University = u1077 });
            faculties.Add(new Faculty { Name = "Spor Bilimleri Fakültesi", University = u1077 });
            faculties.Add(new Faculty { Name = "Tayfur Ata Sökmen Tıp Fakültesi", University = u1077 });
            faculties.Add(new Faculty { Name = "Turizm İşletmeciliği ve Otelcilik Yüksekokulu", University = u1077 });
            faculties.Add(new Faculty { Name = "Veteriner Fakültesi", University = u1077 });
            faculties.Add(new Faculty { Name = "Ziraat Fakültesi", University = u1077 });
            faculties.Add(new Faculty { Name = "İktisadi ve İdari Bilimler Fakültesi", University = u1077 });
            faculties.Add(new Faculty { Name = "İlahiyat Fakültesi", University = u1077 });
            faculties.Add(new Faculty { Name = "İletişim Fakültesi", University = u1077 });
        }

        // İstanbul Yeni Yüzyıl Üniversitesi
        if (uniLookup.TryGetValue("İstanbul Yeni Yüzyıl Üniversitesi", out var u1078))
        {
            faculties.Add(new Faculty { Name = "Eğitim Fakültesi", University = u1078 });
            faculties.Add(new Faculty { Name = "Fen-Edebiyat Fakültesi", University = u1078 });
            faculties.Add(new Faculty { Name = "Mühendislik-Mimarlık Fakültesi", University = u1078 });
            faculties.Add(new Faculty { Name = "Sağlık Bilimleri Fakültesi", University = u1078 });
            faculties.Add(new Faculty { Name = "Spor Bilimleri Fakültesi", University = u1078 });
            faculties.Add(new Faculty { Name = "Uygulamalı Bilimler Fakültesi", University = u1078 });
            faculties.Add(new Faculty { Name = "İktisadi ve İdari Bilimler Fakültesi", University = u1078 });
            faculties.Add(new Faculty { Name = "İlahiyat Fakültesi", University = u1078 });
            faculties.Add(new Faculty { Name = "İletişim Fakültesi", University = u1078 });
        }

        // İzmir Bakırçay Üniversitesi
        if (uniLookup.TryGetValue("İzmir Bakırçay Üniversitesi", out var u1079))
        {
            faculties.Add(new Faculty { Name = "Fen-Edebiyat Fakültesi", University = u1079 });
            faculties.Add(new Faculty { Name = "Güzel Sanatlar, Tasarım ve Mimarlık Fakültesi", University = u1079 });
            faculties.Add(new Faculty { Name = "Hukuk Fakültesi", University = u1079 });
            faculties.Add(new Faculty { Name = "Sağlık Bilimleri Fakültesi", University = u1079 });
            faculties.Add(new Faculty { Name = "Spor Bilimleri Fakültesi", University = u1079 });
            faculties.Add(new Faculty { Name = "Tıp Fakültesi", University = u1079 });
            faculties.Add(new Faculty { Name = "Veteriner Fakültesi", University = u1079 });
            faculties.Add(new Faculty { Name = "Ziraat Fakültesi", University = u1079 });
            faculties.Add(new Faculty { Name = "Çorlu Mühendislik Fakültesi", University = u1079 });
            faculties.Add(new Faculty { Name = "İktisadi ve İdari Bilimler Fakültesi", University = u1079 });
            faculties.Add(new Faculty { Name = "İlahiyat Fakültesi", University = u1079 });
        }

        // İzmir Demokrasi Üniversitesi
        if (uniLookup.TryGetValue("İzmir Demokrasi Üniversitesi", out var u1080))
        {
            faculties.Add(new Faculty { Name = "Diş Hekimliği Fakültesi", University = u1080 });
            faculties.Add(new Faculty { Name = "Eğitim Fakültesi", University = u1080 });
            faculties.Add(new Faculty { Name = "Fen-Edebiyat Fakültesi", University = u1080 });
            faculties.Add(new Faculty { Name = "Mühendislik-Mimarlık Fakültesi", University = u1080 });
            faculties.Add(new Faculty { Name = "Semra ve Vefa Küçük Sağlık Bilimleri Fakültesi", University = u1080 });
            faculties.Add(new Faculty { Name = "Turizm Fakültesi", University = u1080 });
            faculties.Add(new Faculty { Name = "İktisadi ve İdari Bilimler Fakültesi", University = u1080 });
            faculties.Add(new Faculty { Name = "İlahiyat Fakültesi", University = u1080 });
        }

        // İzmir Ekonomi Üniversitesi
        if (uniLookup.TryGetValue("İzmir Ekonomi Üniversitesi", out var u1081))
        {
            faculties.Add(new Faculty { Name = "Bilgisayar ve Bilişim Bilimleri Fakültesi", University = u1081 });
            faculties.Add(new Faculty { Name = "Bor Sağlık Bilimleri Fakültesi", University = u1081 });
            faculties.Add(new Faculty { Name = "Diş Hekimliği Fakültesi", University = u1081 });
            faculties.Add(new Faculty { Name = "Eğitim Fakültesi", University = u1081 });
            faculties.Add(new Faculty { Name = "Fen Fakültesi", University = u1081 });
            faculties.Add(new Faculty { Name = "Güzel Sanatlar Fakültesi", University = u1081 });
            faculties.Add(new Faculty { Name = "Mimarlık Fakültesi", University = u1081 });
            faculties.Add(new Faculty { Name = "Mühendislik Fakültesi", University = u1081 });
            faculties.Add(new Faculty { Name = "Spor Bilimleri Fakültesi", University = u1081 });
            faculties.Add(new Faculty { Name = "Tarım Bilimleri ve Teknolojileri Fakültesi", University = u1081 });
            faculties.Add(new Faculty { Name = "Tıp Fakültesi", University = u1081 });
            faculties.Add(new Faculty { Name = "Zübeyde Hanım Sağlık Bilimleri Fakültesi", University = u1081 });
            faculties.Add(new Faculty { Name = "İktisadi ve İdari Bilimler Fakültesi", University = u1081 });
            faculties.Add(new Faculty { Name = "İletişim Fakültesi", University = u1081 });
            faculties.Add(new Faculty { Name = "İnsan ve Toplum Bilimleri Fakültesi", University = u1081 });
            faculties.Add(new Faculty { Name = "İslami İlimler Fakültesi", University = u1081 });
        }

        // İzmir Katip Çelebi Üniversitesi
        if (uniLookup.TryGetValue("İzmir Katip Çelebi Üniversitesi", out var u1082))
        {
            faculties.Add(new Faculty { Name = "Diş Hekimliği Fakültesi", University = u1082 });
            faculties.Add(new Faculty { Name = "Eczacılık Fakültesi", University = u1082 });
            faculties.Add(new Faculty { Name = "Eğitim Fakültesi", University = u1082 });
            faculties.Add(new Faculty { Name = "Fen Fakültesi", University = u1082 });
            faculties.Add(new Faculty { Name = "Güzel Sanatlar Fakültesi", University = u1082 });
            faculties.Add(new Faculty { Name = "Mimarlık Fakültesi", University = u1082 });
            faculties.Add(new Faculty { Name = "Mühendislik Fakültesi", University = u1082 });
            faculties.Add(new Faculty { Name = "Sağlık Bilimleri Fakültesi", University = u1082 });
            faculties.Add(new Faculty { Name = "Tıp Fakültesi", University = u1082 });
            faculties.Add(new Faculty { Name = "Veteriner Fakültesi", University = u1082 });
            faculties.Add(new Faculty { Name = "Yaşar Doğu Spor Bilimleri Fakültesi", University = u1082 });
            faculties.Add(new Faculty { Name = "Ziraat Fakültesi", University = u1082 });
            faculties.Add(new Faculty { Name = "Çarşamba İnsan ve Toplum Bilimleri Fakültesi", University = u1082 });
            faculties.Add(new Faculty { Name = "İktisadi ve İdari Bilimler Fakültesi", University = u1082 });
            faculties.Add(new Faculty { Name = "İlahiyat Fakültesi", University = u1082 });
            faculties.Add(new Faculty { Name = "İnsan ve Toplum Bilimleri Fakültesi", University = u1082 });
        }

        // İzmir Tin Üniversitesi
        if (uniLookup.TryGetValue("İzmir Tin Üniversitesi", out var u2081))
        {
            faculties.Add(new Faculty { Name = "Mühendislik ve Doğa Bilimleri Fakültesi", University = u2081 });
            faculties.Add(new Faculty { Name = "Sanat, Tasarım ve Mimarlık Fakültesi", University = u2081 });
            faculties.Add(new Faculty { Name = "Sağlık Bilimleri Fakültesi", University = u2081 });
            faculties.Add(new Faculty { Name = "Spor Bilimleri Fakültesi", University = u2081 });
            faculties.Add(new Faculty { Name = "İktisadi, İdari ve Sosyal Bilimler Fakültesi", University = u2081 });
        }

        // İzmir Yüksek Teknoloji Enstitüsü
        if (uniLookup.TryGetValue("İzmir Yüksek Teknoloji Enstitüsü", out var u1083))
        {
            faculties.Add(new Faculty { Name = "Diş Hekimliği Fakültesi", University = u1083 });
            faculties.Add(new Faculty { Name = "Eğitim Fakültesi", University = u1083 });
            faculties.Add(new Faculty { Name = "Fatsa Deniz Bilimleri Fakültesi", University = u1083 });
            faculties.Add(new Faculty { Name = "Fen-Edebiyat Fakültesi", University = u1083 });
            faculties.Add(new Faculty { Name = "Güzel Sanatlar Fakültesi", University = u1083 });
            faculties.Add(new Faculty { Name = "Sağlık Bilimleri Fakültesi", University = u1083 });
            faculties.Add(new Faculty { Name = "Spor Bilimleri Fakültesi", University = u1083 });
            faculties.Add(new Faculty { Name = "Turizm Fakültesi", University = u1083 });
            faculties.Add(new Faculty { Name = "Tıp Fakültesi", University = u1083 });
            faculties.Add(new Faculty { Name = "Ziraat Fakültesi", University = u1083 });
            faculties.Add(new Faculty { Name = "Ünye İktisadi ve İdari Bilimler Fakültesi", University = u1083 });
            faculties.Add(new Faculty { Name = "İlahiyat Fakültesi", University = u1083 });
        }

        // Kadir Has Üniversitesi
        if (uniLookup.TryGetValue("Kadir Has Üniversitesi", out var u2046))
        {
            faculties.Add(new Faculty { Name = "Diş Hekimliği Fakültesi", University = u2046 });
            faculties.Add(new Faculty { Name = "Fen-Edebiyat Fakültesi", University = u2046 });
            faculties.Add(new Faculty { Name = "Güzel Sanatlar ve Tasarım Fakültesi", University = u2046 });
            faculties.Add(new Faculty { Name = "Hukuk Fakültesi", University = u2046 });
            faculties.Add(new Faculty { Name = "Sağlık Bilimleri Fakültesi", University = u2046 });
            faculties.Add(new Faculty { Name = "İktisadi ve İdari Bilimler Fakültesi", University = u2046 });
            faculties.Add(new Faculty { Name = "İktisadi, İdari ve Sosyal Bilimler Fakültesi", University = u2046 });
        }

        // Kafkas Üniversitesi
        if (uniLookup.TryGetValue("Kafkas Üniversitesi", out var u1084))
        {
            faculties.Add(new Faculty { Name = "Eğitim Fakültesi", University = u1084 });
            faculties.Add(new Faculty { Name = "Fen-Edebiyat Fakültesi", University = u1084 });
            faculties.Add(new Faculty { Name = "Mimarlık Fakültesi", University = u1084 });
            faculties.Add(new Faculty { Name = "Mühendislik Fakültesi", University = u1084 });
            faculties.Add(new Faculty { Name = "İktisadi ve İdari Bilimler Fakültesi", University = u1084 });
        }

        // Kahramanmaraş İstiklal Üniversitesi
        if (uniLookup.TryGetValue("Kahramanmaraş İstiklal Üniversitesi", out var u1085))
        {
            faculties.Add(new Faculty { Name = "Beden Eğitimi ve Spor Yüksekokulu", University = u1085 });
            faculties.Add(new Faculty { Name = "Kadirli Sosyal ve Beşeri Bilimler Fakültesi", University = u1085 });
            faculties.Add(new Faculty { Name = "Kadirli Uygulamalı Bilimler Fakültesi", University = u1085 });
            faculties.Add(new Faculty { Name = "Mimarlık, Tasarım ve Güzel Sanatlar Fakültesi", University = u1085 });
            faculties.Add(new Faculty { Name = "Mühendislik ve Doğa Bilimleri Fakültesi", University = u1085 });
            faculties.Add(new Faculty { Name = "Sağlık Bilimleri Fakültesi", University = u1085 });
            faculties.Add(new Faculty { Name = "İktisadi ve İdari Bilimler Fakültesi", University = u1085 });
            faculties.Add(new Faculty { Name = "İlahiyat Fakültesi", University = u1085 });
            faculties.Add(new Faculty { Name = "İnsan ve Toplum Bilimleri Fakültesi", University = u1085 });
        }

        // Kahramanmaraş Sütçü İmam Üniversitesi
        if (uniLookup.TryGetValue("Kahramanmaraş Sütçü İmam Üniversitesi", out var u1086))
        {
            faculties.Add(new Faculty { Name = "Diş Hekimliği Fakültesi", University = u1086 });
            faculties.Add(new Faculty { Name = "Eğitim Fakültesi", University = u1086 });
            faculties.Add(new Faculty { Name = "Fen Fakültesi", University = u1086 });
            faculties.Add(new Faculty { Name = "Fizyoterapi ve Rehabilitasyon Fakültesi", University = u1086 });
            faculties.Add(new Faculty { Name = "Hukuk Fakültesi", University = u1086 });
            faculties.Add(new Faculty { Name = "Mimarlık ve Tasarım Fakültesi", University = u1086 });
            faculties.Add(new Faculty { Name = "Mühendislik Fakültesi", University = u1086 });
            faculties.Add(new Faculty { Name = "Sağlık Bilimleri Fakültesi", University = u1086 });
            faculties.Add(new Faculty { Name = "Spor Bilimleri Fakültesi", University = u1086 });
            faculties.Add(new Faculty { Name = "Teknoloji Fakültesi", University = u1086 });
            faculties.Add(new Faculty { Name = "Turizm Fakültesi", University = u1086 });
            faculties.Add(new Faculty { Name = "Tıp Fakültesi", University = u1086 });
            faculties.Add(new Faculty { Name = "İktisadi ve İdari Bilimler Fakültesi", University = u1086 });
            faculties.Add(new Faculty { Name = "İlahiyat Fakültesi", University = u1086 });
            faculties.Add(new Faculty { Name = "İletişim Fakültesi", University = u1086 });
            faculties.Add(new Faculty { Name = "İnsan ve Toplum Bilimleri Fakültesi", University = u1086 });
        }

        // Kapadokya Üniversitesi
        if (uniLookup.TryGetValue("Kapadokya Üniversitesi", out var u2047))
        {
            faculties.Add(new Faculty { Name = "Diş Hekimliği Fakültesi", University = u2047 });
            faculties.Add(new Faculty { Name = "Eczacılık Fakültesi", University = u2047 });
            faculties.Add(new Faculty { Name = "Eğitim Fakültesi", University = u2047 });
            faculties.Add(new Faculty { Name = "Hukuk Fakültesi", University = u2047 });
            faculties.Add(new Faculty { Name = "Mühendislik ve Doğa Bilimleri Fakültesi", University = u2047 });
            faculties.Add(new Faculty { Name = "Sanat, Tasarım ve Mimarlık Fakültesi", University = u2047 });
            faculties.Add(new Faculty { Name = "Sağlık Bilimleri Fakültesi", University = u2047 });
            faculties.Add(new Faculty { Name = "Tıp Fakültesi", University = u2047 });
            faculties.Add(new Faculty { Name = "Uygulamalı Bilimler Fakültesi", University = u2047 });
            faculties.Add(new Faculty { Name = "İnsan ve Toplum Bilimleri Fakültesi", University = u2047 });
            faculties.Add(new Faculty { Name = "İşletme ve Yönetim Bilimleri Fakültesi", University = u2047 });
        }

        // Karabük Üniversitesi
        if (uniLookup.TryGetValue("Karabük Üniversitesi", out var u1087))
        {
            faculties.Add(new Faculty { Name = "Ardeşen Turizm Fakültesi", University = u1087 });
            faculties.Add(new Faculty { Name = "Diş Hekimliği Fakültesi", University = u1087 });
            faculties.Add(new Faculty { Name = "Fen-Edebiyat Fakültesi", University = u1087 });
            faculties.Add(new Faculty { Name = "Fındıklı Uygulamalı Bilimler Yüksekokulu", University = u1087 });
            faculties.Add(new Faculty { Name = "Güneysu Fizik Tedavi ve Rehabilitasyon Yüksekokulu", University = u1087 });
            faculties.Add(new Faculty { Name = "Hukuk Fakültesi", University = u1087 });
            faculties.Add(new Faculty { Name = "Mühendislik ve Mimarlık Fakültesi", University = u1087 });
            faculties.Add(new Faculty { Name = "Sağlık Bilimleri Fakültesi", University = u1087 });
            faculties.Add(new Faculty { Name = "Spor Bilimleri Fakültesi", University = u1087 });
            faculties.Add(new Faculty { Name = "Su Ürünleri Fakültesi", University = u1087 });
            faculties.Add(new Faculty { Name = "Turgut Kıran Denizcilik Fakültesi", University = u1087 });
            faculties.Add(new Faculty { Name = "Tıp Fakültesi", University = u1087 });
            faculties.Add(new Faculty { Name = "İktisadi ve İdari Bilimler Fakültesi", University = u1087 });
            faculties.Add(new Faculty { Name = "İlahiyat Fakültesi", University = u1087 });
        }

        // Karadeniz Teknik Üniversitesi
        if (uniLookup.TryGetValue("Karadeniz Teknik Üniversitesi", out var u1088))
        {
            faculties.Add(new Faculty { Name = "Bilgisayar ve Bilişim Bilimleri Fakültesi", University = u1088 });
            faculties.Add(new Faculty { Name = "Diş Hekimliği Fakültesi", University = u1088 });
            faculties.Add(new Faculty { Name = "Fen Fakültesi", University = u1088 });
            faculties.Add(new Faculty { Name = "Hukuk Fakültesi", University = u1088 });
            faculties.Add(new Faculty { Name = "Mühendislik Fakültesi", University = u1088 });
            faculties.Add(new Faculty { Name = "Sanat, Tasarım ve Mimarlık Fakültesi", University = u1088 });
            faculties.Add(new Faculty { Name = "Sağlık Bilimleri Fakültesi", University = u1088 });
            faculties.Add(new Faculty { Name = "Siyasal Bilgiler Fakültesi", University = u1088 });
            faculties.Add(new Faculty { Name = "Tıp Fakültesi", University = u1088 });
            faculties.Add(new Faculty { Name = "İlahiyat Fakültesi", University = u1088 });
            faculties.Add(new Faculty { Name = "İletişim Fakültesi", University = u1088 });
            faculties.Add(new Faculty { Name = "İnsan ve Toplum Bilimleri Fakültesi", University = u1088 });
            faculties.Add(new Faculty { Name = "İşletme Fakültesi", University = u1088 });
        }

        // Karamanoğlu Mehmetbey Üniversitesi
        if (uniLookup.TryGetValue("Karamanoğlu Mehmetbey Üniversitesi", out var u1089))
        {
            faculties.Add(new Faculty { Name = "Akşehir Kadir Yallagöz Sağlık Yüksekokulu", University = u1089 });
            faculties.Add(new Faculty { Name = "Akşehir Mühendislik ve Mimarlık Fakültesi", University = u1089 });
            faculties.Add(new Faculty { Name = "Akşehir İktisadi ve İdari Bilimler Fakültesi", University = u1089 });
            faculties.Add(new Faculty { Name = "Beyşehir Ali Akkanat Turizm Fakültesi", University = u1089 });
            faculties.Add(new Faculty { Name = "Beyşehir Ali Akkanat Uygulamalı Bilimler Yüksekokulu", University = u1089 });
            faculties.Add(new Faculty { Name = "Beyşehir Ali Akkanat İşletme Fakültesi", University = u1089 });
            faculties.Add(new Faculty { Name = "Diş Hekimliği Fakültesi", University = u1089 });
            faculties.Add(new Faculty { Name = "Eczacılık Fakültesi", University = u1089 });
            faculties.Add(new Faculty { Name = "Edebiyat Fakültesi", University = u1089 });
            faculties.Add(new Faculty { Name = "Eğitim Fakültesi", University = u1089 });
            faculties.Add(new Faculty { Name = "Fen Fakültesi", University = u1089 });
            faculties.Add(new Faculty { Name = "Güzel Sanatlar Fakültesi", University = u1089 });
            faculties.Add(new Faculty { Name = "Hemşirelik Fakültesi", University = u1089 });
            faculties.Add(new Faculty { Name = "Hukuk Fakültesi", University = u1089 });
            faculties.Add(new Faculty { Name = "Kulu Sağlık Bilimleri Fakültesi", University = u1089 });
            faculties.Add(new Faculty { Name = "Mimarlık ve Tasarım Fakültesi", University = u1089 });
            faculties.Add(new Faculty { Name = "Sağlık Bilimleri Fakültesi", University = u1089 });
            faculties.Add(new Faculty { Name = "Sivil Havacılık Yüksekokulu", University = u1089 });
            faculties.Add(new Faculty { Name = "Spor Bilimleri Fakültesi", University = u1089 });
            faculties.Add(new Faculty { Name = "Teknoloji Fakültesi", University = u1089 });
            faculties.Add(new Faculty { Name = "Turizm Fakültesi", University = u1089 });
            faculties.Add(new Faculty { Name = "Tıp Fakültesi", University = u1089 });
            faculties.Add(new Faculty { Name = "Veteriner Fakültesi", University = u1089 });
            faculties.Add(new Faculty { Name = "Ziraat Fakültesi", University = u1089 });
            faculties.Add(new Faculty { Name = "Çumra Uygulamalı Bilimler Yüksekokulu", University = u1089 });
            faculties.Add(new Faculty { Name = "İktisadi ve İdari Bilimler Fakültesi", University = u1089 });
            faculties.Add(new Faculty { Name = "İlahiyat Fakültesi", University = u1089 });
            faculties.Add(new Faculty { Name = "İletişim Fakültesi", University = u1089 });
        }

        // Kastamonu Üniversitesi
        if (uniLookup.TryGetValue("Kastamonu Üniversitesi", out var u1090))
        {
            faculties.Add(new Faculty { Name = "Beden Eğitimi ve Spor Yüksekokulu", University = u1090 });
            faculties.Add(new Faculty { Name = "Eğitim Fakültesi", University = u1090 });
            faculties.Add(new Faculty { Name = "Fen-Edebiyat Fakültesi", University = u1090 });
            faculties.Add(new Faculty { Name = "Güzel Sanatlar ve Tasarım Fakültesi", University = u1090 });
            faculties.Add(new Faculty { Name = "Mühendislik Fakültesi", University = u1090 });
            faculties.Add(new Faculty { Name = "Sağlık Bilimleri Fakültesi", University = u1090 });
            faculties.Add(new Faculty { Name = "Turizm İşletmeciliği ve Otelcilik Yüksekokulu", University = u1090 });
            faculties.Add(new Faculty { Name = "Tıp Fakültesi", University = u1090 });
            faculties.Add(new Faculty { Name = "Veteriner Fakültesi", University = u1090 });
            faculties.Add(new Faculty { Name = "Ziraat Fakültesi", University = u1090 });
            faculties.Add(new Faculty { Name = "İktisadi ve İdari Bilimler Fakültesi", University = u1090 });
            faculties.Add(new Faculty { Name = "İlahiyat Fakültesi", University = u1090 });
        }

        // Kıbrıs İlim Üniversitesi
        if (uniLookup.TryGetValue("Kıbrıs İlim Üniversitesi", out var u3016))
        {
            faculties.Add(new Faculty { Name = "Sanat Fakültesi", University = u3016 });
            faculties.Add(new Faculty { Name = "Tasarım Fakültesi", University = u3016 });
            faculties.Add(new Faculty { Name = "İletişim Fakültesi", University = u3016 });
        }

        // Kıbrıs Sağlık ve Toplum Bilimleri Üniversitesi
        if (uniLookup.TryGetValue("Kıbrıs Sağlık ve Toplum Bilimleri Üniversitesi", out var u3012))
        {
            faculties.Add(new Faculty { Name = "Beden Eğitimi ve Spor Yüksekokulu", University = u3012 });
            faculties.Add(new Faculty { Name = "Diş Hekimliği Fakültesi", University = u3012 });
            faculties.Add(new Faculty { Name = "Sağlık Bilimleri Fakültesi", University = u3012 });
            faculties.Add(new Faculty { Name = "Sosyal ve Beşeri Bilimler Fakültesi", University = u3012 });
        }

        // Kıbrıs Türk Üniversitesi
        if (uniLookup.TryGetValue("Kıbrıs Türk Üniversitesi", out var u3013))
        {
            faculties.Add(new Faculty { Name = "Diş Hekimliği Fakültesi", University = u3013 });
            faculties.Add(new Faculty { Name = "Eğitim Bilimleri Fakültesi", University = u3013 });
            faculties.Add(new Faculty { Name = "Fen-Edebiyat Fakültesi", University = u3013 });
            faculties.Add(new Faculty { Name = "Hukuk Fakültesi", University = u3013 });
            faculties.Add(new Faculty { Name = "Mimarlık ve Güzel Sanatlar Fakültesi", University = u3013 });
            faculties.Add(new Faculty { Name = "Mühendislik Fakültesi", University = u3013 });
            faculties.Add(new Faculty { Name = "Sağlık Bilimleri Fakültesi", University = u3013 });
            faculties.Add(new Faculty { Name = "Ünal Çağıner Turizm ve Mutfak Sanatları Yüksekokulu", University = u3013 });
            faculties.Add(new Faculty { Name = "İktisadi ve İdari Bilimler Fakültesi", University = u3013 });
        }

        // Kırşehir Ahi Evran Üniversitesi
        if (uniLookup.TryGetValue("Kırşehir Ahi Evran Üniversitesi", out var u1006))
        {
            faculties.Add(new Faculty { Name = "Eğitim Fakültesi", University = u1006 });
            faculties.Add(new Faculty { Name = "Fen-Edebiyat Fakültesi", University = u1006 });
            faculties.Add(new Faculty { Name = "Fizik Tedavi ve Rehabilitasyon Yüksekokulu", University = u1006 });
            faculties.Add(new Faculty { Name = "Kaman Uygulamalı Bilimler Yüksekokulu", University = u1006 });
            faculties.Add(new Faculty { Name = "Mühendislik-Mimarlık Fakültesi", University = u1006 });
            faculties.Add(new Faculty { Name = "Sağlık Bilimleri Fakültesi", University = u1006 });
            faculties.Add(new Faculty { Name = "Spor Bilimleri Fakültesi", University = u1006 });
            faculties.Add(new Faculty { Name = "Tıp Fakültesi", University = u1006 });
            faculties.Add(new Faculty { Name = "Ziraat Fakültesi", University = u1006 });
            faculties.Add(new Faculty { Name = "İktisadi ve İdari Bilimler Fakültesi", University = u1006 });
            faculties.Add(new Faculty { Name = "İlahiyat Fakültesi", University = u1006 });
        }

        // Kilis 7 Aralık Üniversitesi
        if (uniLookup.TryGetValue("Kilis 7 Aralık Üniversitesi", out var u1091))
        {
            faculties.Add(new Faculty { Name = "Boyabat İktisadi ve İdari Bilimler Fakültesi", University = u1091 });
            faculties.Add(new Faculty { Name = "Eğitim Fakültesi", University = u1091 });
            faculties.Add(new Faculty { Name = "Fen-Edebiyat Fakültesi", University = u1091 });
            faculties.Add(new Faculty { Name = "Mühendislik ve Mimarlık Fakültesi", University = u1091 });
            faculties.Add(new Faculty { Name = "Sağlık Bilimleri Fakültesi", University = u1091 });
            faculties.Add(new Faculty { Name = "Spor Bilimleri Fakültesi", University = u1091 });
            faculties.Add(new Faculty { Name = "Su Ürünleri Fakültesi", University = u1091 });
            faculties.Add(new Faculty { Name = "Turizm Fakültesi", University = u1091 });
            faculties.Add(new Faculty { Name = "İlahiyat Fakültesi", University = u1091 });
        }

        // Kocaeli Üniversitesi
        if (uniLookup.TryGetValue("Kocaeli Üniversitesi", out var u1092))
        {
            faculties.Add(new Faculty { Name = "Diş Hekimliği Fakültesi", University = u1092 });
            faculties.Add(new Faculty { Name = "Eczacılık Fakültesi", University = u1092 });
            faculties.Add(new Faculty { Name = "Eğitim Fakültesi", University = u1092 });
            faculties.Add(new Faculty { Name = "Hukuk Fakültesi", University = u1092 });
            faculties.Add(new Faculty { Name = "Mimarlık Fakültesi", University = u1092 });
            faculties.Add(new Faculty { Name = "Mühendislik ve Doğa Bilimleri Fakültesi", University = u1092 });
            faculties.Add(new Faculty { Name = "Sağlık Bilimleri Fakültesi", University = u1092 });
            faculties.Add(new Faculty { Name = "Spor Bilimleri Fakültesi", University = u1092 });
            faculties.Add(new Faculty { Name = "Tıp Fakültesi", University = u1092 });
            faculties.Add(new Faculty { Name = "İktisadi ve İdari Bilimler Fakültesi", University = u1092 });
            faculties.Add(new Faculty { Name = "İlahiyat Fakültesi", University = u1092 });
            faculties.Add(new Faculty { Name = "İletişim Fakültesi", University = u1092 });
            faculties.Add(new Faculty { Name = "İnsan ve Toplum Bilimleri Fakültesi", University = u1092 });
        }

        // Koç Üniversitesi
        if (uniLookup.TryGetValue("Koç Üniversitesi", out var u1093))
        {
            faculties.Add(new Faculty { Name = "Mühendislik Fakültesi", University = u1093 });
            faculties.Add(new Faculty { Name = "Sağlık Bilimleri Fakültesi", University = u1093 });
            faculties.Add(new Faculty { Name = "Turizm ve Otel İşletmeciliği Yüksekokulu", University = u1093 });
            faculties.Add(new Faculty { Name = "Ziraat Fakültesi", University = u1093 });
            faculties.Add(new Faculty { Name = "İktisadi ve İdari Bilimler Fakültesi", University = u1093 });
            faculties.Add(new Faculty { Name = "İlahiyat Fakültesi", University = u1093 });
        }

        // Konya Gıda ve Tarım Üniversitesi
        if (uniLookup.TryGetValue("Konya Gıda ve Tarım Üniversitesi", out var u2049))
        {
            faculties.Add(new Faculty { Name = "Denizcilik Fakültesi", University = u2049 });
            faculties.Add(new Faculty { Name = "Hukuk Fakültesi", University = u2049 });
            faculties.Add(new Faculty { Name = "Mühendislik Fakültesi", University = u2049 });
            faculties.Add(new Faculty { Name = "İktisadi ve İdari Bilimler Fakültesi", University = u2049 });
        }

        // Konya Teknik Üniversitesi
        if (uniLookup.TryGetValue("Konya Teknik Üniversitesi", out var u1094))
        {
            faculties.Add(new Faculty { Name = "Diş Hekimliği Fakültesi", University = u1094 });
            faculties.Add(new Faculty { Name = "Eczacılık Fakültesi", University = u1094 });
            faculties.Add(new Faculty { Name = "Edebiyat Fakültesi", University = u1094 });
            faculties.Add(new Faculty { Name = "Eğitim Fakültesi", University = u1094 });
            faculties.Add(new Faculty { Name = "Fen Fakültesi", University = u1094 });
            faculties.Add(new Faculty { Name = "Güzel Sanatlar Fakültesi", University = u1094 });
            faculties.Add(new Faculty { Name = "Keşan Hakkı Yörük Sağlık Yüksekokulu", University = u1094 });
            faculties.Add(new Faculty { Name = "Keşan Yusuf Çapraz Uygulamalı Bilimler Yüksekokulu", University = u1094 });
            faculties.Add(new Faculty { Name = "Kırkpınar Spor Bilimleri Fakültesi", University = u1094 });
            faculties.Add(new Faculty { Name = "Mimarlık Fakültesi", University = u1094 });
            faculties.Add(new Faculty { Name = "Mühendislik Fakültesi", University = u1094 });
            faculties.Add(new Faculty { Name = "Sağlık Bilimleri Fakültesi", University = u1094 });
            faculties.Add(new Faculty { Name = "Tıp Fakültesi", University = u1094 });
            faculties.Add(new Faculty { Name = "Uygulamalı Bilimler Fakültesi", University = u1094 });
            faculties.Add(new Faculty { Name = "Uzunköprü Uygulamalı Bilimler Yüksekokulu", University = u1094 });
            faculties.Add(new Faculty { Name = "İktisadi ve İdari Bilimler Fakültesi", University = u1094 });
            faculties.Add(new Faculty { Name = "İlahiyat Fakültesi", University = u1094 });
        }

        // Kto Karatay Üniversitesi
        if (uniLookup.TryGetValue("Kto Karatay Üniversitesi", out var u2087))
        {
            faculties.Add(new Faculty { Name = "Diş Hekimliği Fakültesi", University = u2087 });
            faculties.Add(new Faculty { Name = "Eczacılık Fakültesi", University = u2087 });
            faculties.Add(new Faculty { Name = "Mühendislik ve Doğa Bilimleri Fakültesi", University = u2087 });
            faculties.Add(new Faculty { Name = "Sanat ve Tasarım Fakültesi", University = u2087 });
            faculties.Add(new Faculty { Name = "Sağlık Bilimleri Fakültesi", University = u2087 });
            faculties.Add(new Faculty { Name = "İktisadi, İdari ve Sosyal Bilimler Fakültesi", University = u2087 });
        }

        // Kütahya Dumlupınar Üniversitesi
        if (uniLookup.TryGetValue("Kütahya Dumlupınar Üniversitesi", out var u1095))
        {
            faculties.Add(new Faculty { Name = "Edebiyat Fakültesi", University = u1095 });
            faculties.Add(new Faculty { Name = "Güzel Sanatlar, Tasarım ve Mimarlık Fakültesi", University = u1095 });
            faculties.Add(new Faculty { Name = "Mühendislik Fakültesi", University = u1095 });
            faculties.Add(new Faculty { Name = "Sağlık Bilimleri Fakültesi", University = u1095 });
            faculties.Add(new Faculty { Name = "Spor Bilimleri Fakültesi", University = u1095 });
            faculties.Add(new Faculty { Name = "Su Ürünleri Fakültesi", University = u1095 });
            faculties.Add(new Faculty { Name = "İktisadi ve İdari Bilimler Fakültesi", University = u1095 });
            faculties.Add(new Faculty { Name = "İletişim Fakültesi", University = u1095 });
        }

        // Kütahya Sağlık Bilimleri Üniversitesi
        if (uniLookup.TryGetValue("Kütahya Sağlık Bilimleri Üniversitesi", out var u1096))
        {
            faculties.Add(new Faculty { Name = "Fen Fakültesi", University = u1096 });
            faculties.Add(new Faculty { Name = "Hukuk Fakültesi", University = u1096 });
            faculties.Add(new Faculty { Name = "Kültür ve Sosyal Bilimler Fakültesi", University = u1096 });
            faculties.Add(new Faculty { Name = "Mühendislik Fakültesi", University = u1096 });
            faculties.Add(new Faculty { Name = "İktisadi ve İdari Bilimler Fakültesi", University = u1096 });
        }

        // Lefke Avrupa Üniversitesi
        if (uniLookup.TryGetValue("Lefke Avrupa Üniversitesi", out var u3002))
        {
            faculties.Add(new Faculty { Name = "Beşeri Bilimler Fakültesi", University = u3002 });
            faculties.Add(new Faculty { Name = "Denizcilik ve Ulaştırma Yüksekokulu", University = u3002 });
            faculties.Add(new Faculty { Name = "Eczacılık Fakültesi", University = u3002 });
            faculties.Add(new Faculty { Name = "Eğitim Fakültesi", University = u3002 });
            faculties.Add(new Faculty { Name = "Havacılık Yüksekokulu", University = u3002 });
            faculties.Add(new Faculty { Name = "Hemşirelik Yüksekokulu", University = u3002 });
            faculties.Add(new Faculty { Name = "Hukuk Fakültesi", University = u3002 });
            faculties.Add(new Faculty { Name = "Mimarlık, Tasarım ve Güzel Sanatlar Fakültesi", University = u3002 });
            faculties.Add(new Faculty { Name = "Mühendislik Fakültesi", University = u3002 });
            faculties.Add(new Faculty { Name = "Sağlık Bilimleri Fakültesi", University = u3002 });
            faculties.Add(new Faculty { Name = "Siyasal Bilimler Fakültesi", University = u3002 });
            faculties.Add(new Faculty { Name = "Uygulamalı Sosyal Bilimler Yüksekokulu", University = u3002 });
            faculties.Add(new Faculty { Name = "İletişim Fakültesi", University = u3002 });
            faculties.Add(new Faculty { Name = "İşletme Fakültesi", University = u3002 });
        }

        // Lefkoşa Üniversitesi
        if (uniLookup.TryGetValue("Lefkoşa Üniversitesi", out var u3014))
        {
            faculties.Add(new Faculty { Name = "Diş Hekimliği Fakültesi", University = u3014 });
            faculties.Add(new Faculty { Name = "Eğitim Fakültesi", University = u3014 });
            faculties.Add(new Faculty { Name = "Hukuk Fakültesi", University = u3014 });
            faculties.Add(new Faculty { Name = "Mühendislik ve Mimarlık Fakültesi", University = u3014 });
            faculties.Add(new Faculty { Name = "İktisadi, İdari ve Sosyal Bilimler Fakültesi", University = u3014 });
        }

        // Lokman Hekim Üniversitesi
        if (uniLookup.TryGetValue("Lokman Hekim Üniversitesi", out var u2050))
        {
            faculties.Add(new Faculty { Name = "Güzel Sanatlar, Tasarım ve Mimarlık Fakültesi", University = u2050 });
            faculties.Add(new Faculty { Name = "Mühendislik ve Doğa Bilimleri Fakültesi", University = u2050 });
            faculties.Add(new Faculty { Name = "Sağlık Bilimleri Fakültesi", University = u2050 });
            faculties.Add(new Faculty { Name = "Spor Bilimleri Fakültesi", University = u2050 });
            faculties.Add(new Faculty { Name = "İktisadi, İdari ve Sosyal Bilimler Fakültesi", University = u2050 });
            faculties.Add(new Faculty { Name = "İletişim Fakültesi", University = u2050 });
            faculties.Add(new Faculty { Name = "İnsan ve Toplum Bilimleri Fakültesi", University = u2050 });
        }

        // Magosa Üniversitesi
        if (uniLookup.TryGetValue("Magosa Üniversitesi", out var u3018))
        {
            faculties.Add(new Faculty { Name = "Mimarlık ve Mühendislik Fakültesi", University = u3018 });
            faculties.Add(new Faculty { Name = "İşletme ve Ekonomi Fakültesi", University = u3018 });
        }

        // Malatya Turgut Özal Üniversitesi
        if (uniLookup.TryGetValue("Malatya Turgut Özal Üniversitesi", out var u1097))
        {
            faculties.Add(new Faculty { Name = "Diş Hekimliği Fakültesi", University = u1097 });
            faculties.Add(new Faculty { Name = "Eğitim Fakültesi", University = u1097 });
            faculties.Add(new Faculty { Name = "Fen-Edebiyat Fakültesi", University = u1097 });
            faculties.Add(new Faculty { Name = "Mimarlık Fakültesi", University = u1097 });
            faculties.Add(new Faculty { Name = "Mühendislik Fakültesi", University = u1097 });
            faculties.Add(new Faculty { Name = "Sağlık Bilimleri Fakültesi", University = u1097 });
            faculties.Add(new Faculty { Name = "Tıp Fakültesi", University = u1097 });
            faculties.Add(new Faculty { Name = "Veteriner Fakültesi", University = u1097 });
            faculties.Add(new Faculty { Name = "Ziraat Fakültesi", University = u1097 });
            faculties.Add(new Faculty { Name = "İktisadi ve İdari Bilimler Fakültesi", University = u1097 });
            faculties.Add(new Faculty { Name = "İlahiyat Fakültesi", University = u1097 });
            faculties.Add(new Faculty { Name = "İnegöl İşletme Fakültesi", University = u1097 });
        }

        // Manisa Celal Bayar Üniversitesi
        if (uniLookup.TryGetValue("Manisa Celal Bayar Üniversitesi", out var u1098))
        {
            faculties.Add(new Faculty { Name = "Diş Hekimliği Fakültesi", University = u1098 });
            faculties.Add(new Faculty { Name = "Eğitim Fakültesi", University = u1098 });
            faculties.Add(new Faculty { Name = "Güzel Sanatlar Fakültesi", University = u1098 });
            faculties.Add(new Faculty { Name = "Mimarlık ve Tasarım Fakültesi", University = u1098 });
            faculties.Add(new Faculty { Name = "Mühendislik ve Doğa Bilimleri Fakültesi", University = u1098 });
            faculties.Add(new Faculty { Name = "Sağlık Bilimleri Fakültesi", University = u1098 });
            faculties.Add(new Faculty { Name = "Spor Bilimleri Fakültesi", University = u1098 });
            faculties.Add(new Faculty { Name = "Tıp Fakültesi", University = u1098 });
            faculties.Add(new Faculty { Name = "Uygulamalı Bilimler Fakültesi", University = u1098 });
            faculties.Add(new Faculty { Name = "Ziraat Fakültesi", University = u1098 });
            faculties.Add(new Faculty { Name = "İktisadi ve İdari Bilimler Fakültesi", University = u1098 });
            faculties.Add(new Faculty { Name = "İlahiyat Fakültesi", University = u1098 });
            faculties.Add(new Faculty { Name = "İletişim Fakültesi", University = u1098 });
            faculties.Add(new Faculty { Name = "İnsan ve Toplum Bilimleri Fakültesi", University = u1098 });
        }

        // Mardin Artuklu Üniversitesi
        if (uniLookup.TryGetValue("Mardin Artuklu Üniversitesi", out var u1099))
        {
            faculties.Add(new Faculty { Name = "Hukuk Fakültesi", University = u1099 });
            faculties.Add(new Faculty { Name = "Mühendislik Fakültesi", University = u1099 });
            faculties.Add(new Faculty { Name = "Sanat ve Tasarım Fakültesi", University = u1099 });
            faculties.Add(new Faculty { Name = "Sağlık Bilimleri Fakültesi", University = u1099 });
            faculties.Add(new Faculty { Name = "Spor Bilimleri Fakültesi", University = u1099 });
            faculties.Add(new Faculty { Name = "Tıp Fakültesi", University = u1099 });
            faculties.Add(new Faculty { Name = "Yalova İktisadi ve İdari Bilimler Fakültesi", University = u1099 });
            faculties.Add(new Faculty { Name = "İnsan ve Toplum Bilimleri Fakültesi", University = u1099 });
            faculties.Add(new Faculty { Name = "İslami İlimler Fakültesi", University = u1099 });
        }

        // Marmara Üniversitesi
        if (uniLookup.TryGetValue("Marmara Üniversitesi", out var u1101))
        {
            faculties.Add(new Faculty { Name = "Elektrik-Elektronik Fakültesi", University = u1101 });
            faculties.Add(new Faculty { Name = "Eğitim Fakültesi", University = u1101 });
            faculties.Add(new Faculty { Name = "Fen-Edebiyat Fakültesi", University = u1101 });
            faculties.Add(new Faculty { Name = "Gemi İnşaatı ve Denizcilik Fakültesi", University = u1101 });
            faculties.Add(new Faculty { Name = "Kimya-Metalurji Fakültesi", University = u1101 });
            faculties.Add(new Faculty { Name = "Makine Fakültesi", University = u1101 });
            faculties.Add(new Faculty { Name = "Mimarlık Fakültesi", University = u1101 });
            faculties.Add(new Faculty { Name = "Sanat ve Tasarım Fakültesi", University = u1101 });
            faculties.Add(new Faculty { Name = "İktisadi ve İdari Bilimler Fakültesi", University = u1101 });
            faculties.Add(new Faculty { Name = "İnşaat Fakültesi", University = u1101 });
        }

        // Mef Üniversitesi
        if (uniLookup.TryGetValue("Mef Üniversitesi", out var u2051))
        {
            faculties.Add(new Faculty { Name = "Mühendislik ve Doğa Bilimleri Fakültesi", University = u2051 });
            faculties.Add(new Faculty { Name = "Sanat ve Sosyal Bilimler Fakültesi", University = u2051 });
            faculties.Add(new Faculty { Name = "Yönetim Bilimleri Fakültesi", University = u2051 });
        }

        // Mersin Uluslararası Üniversitesi
        if (uniLookup.TryGetValue("Mersin Uluslararası Üniversitesi", out var u3017))
        {
            faculties.Add(new Faculty { Name = "Mimarlık ve Mühendislik Fakültesi", University = u3017 });
            faculties.Add(new Faculty { Name = "Turizm Fakültesi", University = u3017 });
            faculties.Add(new Faculty { Name = "İktisadi, İdari ve Sosyal Bilimler Fakültesi", University = u3017 });
        }

        // Mersin Üniversitesi
        if (uniLookup.TryGetValue("Mersin Üniversitesi", out var u1102))
        {
            faculties.Add(new Faculty { Name = "Diş Hekimliği Fakültesi", University = u1102 });
            faculties.Add(new Faculty { Name = "Eczacılık Fakültesi", University = u1102 });
            faculties.Add(new Faculty { Name = "Edebiyat Fakültesi", University = u1102 });
            faculties.Add(new Faculty { Name = "Erciş İşletme Fakültesi", University = u1102 });
            faculties.Add(new Faculty { Name = "Eğitim Fakültesi", University = u1102 });
            faculties.Add(new Faculty { Name = "Fen Fakültesi", University = u1102 });
            faculties.Add(new Faculty { Name = "Güzel Sanatlar Fakültesi", University = u1102 });
            faculties.Add(new Faculty { Name = "Mimarlık ve Tasarım Fakültesi", University = u1102 });
            faculties.Add(new Faculty { Name = "Mühendislik Fakültesi", University = u1102 });
            faculties.Add(new Faculty { Name = "Sağlık Bilimleri Fakültesi", University = u1102 });
            faculties.Add(new Faculty { Name = "Spor Bilimleri Fakültesi", University = u1102 });
            faculties.Add(new Faculty { Name = "Su Ürünleri Fakültesi", University = u1102 });
            faculties.Add(new Faculty { Name = "Turizm Fakültesi", University = u1102 });
            faculties.Add(new Faculty { Name = "Tıp Fakültesi", University = u1102 });
            faculties.Add(new Faculty { Name = "Veteriner Fakültesi", University = u1102 });
            faculties.Add(new Faculty { Name = "Ziraat Fakültesi", University = u1102 });
            faculties.Add(new Faculty { Name = "İktisadi ve İdari Bilimler Fakültesi", University = u1102 });
            faculties.Add(new Faculty { Name = "İlahiyat Fakültesi", University = u1102 });
        }

        // Mimar Sinan Güzel Sanatlar Üniversitesi
        if (uniLookup.TryGetValue("Mimar Sinan Güzel Sanatlar Üniversitesi", out var u1103))
        {
            faculties.Add(new Faculty { Name = "Denizcilik Fakültesi", University = u1103 });
            faculties.Add(new Faculty { Name = "Diş Hekimliği Fakültesi", University = u1103 });
            faculties.Add(new Faculty { Name = "Eczacılık Fakültesi", University = u1103 });
            faculties.Add(new Faculty { Name = "Ereğli Eğitim Fakültesi", University = u1103 });
            faculties.Add(new Faculty { Name = "Fen Fakültesi", University = u1103 });
            faculties.Add(new Faculty { Name = "Karadeniz Ereğli Turizm Fakültesi", University = u1103 });
            faculties.Add(new Faculty { Name = "Mühendislik Fakültesi", University = u1103 });
            faculties.Add(new Faculty { Name = "Sağlık Bilimleri Fakültesi", University = u1103 });
            faculties.Add(new Faculty { Name = "Teoman Duralı İnsan ve Toplum Bilimleri Fakültesi", University = u1103 });
            faculties.Add(new Faculty { Name = "Tıp Fakültesi", University = u1103 });
            faculties.Add(new Faculty { Name = "İktisadi ve İdari Bilimler Fakültesi", University = u1103 });
            faculties.Add(new Faculty { Name = "İlahiyat Fakültesi", University = u1103 });
            faculties.Add(new Faculty { Name = "İletişim Fakültesi", University = u1103 });
        }

        // Muğla Sıtkı Koçman Üniversitesi
        if (uniLookup.TryGetValue("Muğla Sıtkı Koçman Üniversitesi", out var u1106))
        {
            faculties.Add(new Faculty { Name = "Denizcilik Fakültesi", University = u1106 });
            faculties.Add(new Faculty { Name = "Mühendislik ve Doğa Bilimleri Fakültesi", University = u1106 });
            faculties.Add(new Faculty { Name = "Sağlık Bilimleri Fakültesi", University = u1106 });
            faculties.Add(new Faculty { Name = "Spor Bilimleri Fakültesi", University = u1106 });
            faculties.Add(new Faculty { Name = "Tıp Fakültesi", University = u1106 });
            faculties.Add(new Faculty { Name = "Ömer Seyfettin Uygulamalı Bilimler Fakültesi", University = u1106 });
            faculties.Add(new Faculty { Name = "İktisadi ve İdari Bilimler Fakültesi", University = u1106 });
            faculties.Add(new Faculty { Name = "İlahiyat Fakültesi", University = u1106 });
            faculties.Add(new Faculty { Name = "İletişim Fakültesi", University = u1106 });
            faculties.Add(new Faculty { Name = "İnsan ve Toplum Bilimleri Fakültesi", University = u1106 });
        }

        // Muş Alparslan Üniversitesi
        if (uniLookup.TryGetValue("Muş Alparslan Üniversitesi", out var u1107))
        {
            faculties.Add(new Faculty { Name = "Barbaros Hayrettin Gemi İnşaatı ve Denizcilik Fakültesi", University = u1107 });
            faculties.Add(new Faculty { Name = "Deniz Bilimleri ve Teknolojisi Fakültesi", University = u1107 });
            faculties.Add(new Faculty { Name = "Havacılık ve Uzay Bilimleri Fakültesi", University = u1107 });
            faculties.Add(new Faculty { Name = "Mimarlık Fakültesi", University = u1107 });
            faculties.Add(new Faculty { Name = "Mühendislik ve Doğa Bilimleri Fakültesi", University = u1107 });
            faculties.Add(new Faculty { Name = "Turizm Fakültesi", University = u1107 });
            faculties.Add(new Faculty { Name = "İşletme ve Yönetim Bilimleri Fakültesi", University = u1107 });
        }

        // Necmettin Erbakan Üniversitesi
        if (uniLookup.TryGetValue("Necmettin Erbakan Üniversitesi", out var u1110))
        {
            faculties.Add(new Faculty { Name = "Adana Tıp Fakültesi", University = u1110 });
            faculties.Add(new Faculty { Name = "Bursa Tıp Fakültesi", University = u1110 });
            faculties.Add(new Faculty { Name = "Erzurum Tıp Fakültesi", University = u1110 });
            faculties.Add(new Faculty { Name = "Gülhane Fizyoterapi ve Rehabilitasyon Fakültesi", University = u1110 });
            faculties.Add(new Faculty { Name = "Gülhane Hemşirelik Fakültesi", University = u1110 });
            faculties.Add(new Faculty { Name = "Gülhane Sağlık Bilimleri Fakültesi", University = u1110 });
            faculties.Add(new Faculty { Name = "Gülhane Tıp Fakültesi", University = u1110 });
            faculties.Add(new Faculty { Name = "Hamidiye Diş Hekimliği Fakültesi", University = u1110 });
            faculties.Add(new Faculty { Name = "Hamidiye Eczacılık Fakültesi", University = u1110 });
            faculties.Add(new Faculty { Name = "Hamidiye Hemşirelik Fakültesi", University = u1110 });
            faculties.Add(new Faculty { Name = "Hamidiye Sağlık Bilimleri Fakültesi", University = u1110 });
            faculties.Add(new Faculty { Name = "Hamidiye Tıp Fakültesi", University = u1110 });
            faculties.Add(new Faculty { Name = "Hamidiye Uluslararası Tıp Fakültesi", University = u1110 });
            faculties.Add(new Faculty { Name = "Hamidiye Yaşam Bilimleri Fakültesi", University = u1110 });
            faculties.Add(new Faculty { Name = "Kayseri Tıp Fakültesi", University = u1110 });
            faculties.Add(new Faculty { Name = "Saraybosna Uluslararası Diş Hekimliği Fakültesi", University = u1110 });
            faculties.Add(new Faculty { Name = "Saraybosna Uluslararası Tıp Fakültesi", University = u1110 });
            faculties.Add(new Faculty { Name = "İzmir Tıp Fakültesi", University = u1110 });
        }

        // Nevşehir Hacı Bektaş Veli Üniversitesi
        if (uniLookup.TryGetValue("Nevşehir Hacı Bektaş Veli Üniversitesi", out var u1111))
        {
            faculties.Add(new Faculty { Name = "Diş Hekimliği Fakültesi", University = u1111 });
            faculties.Add(new Faculty { Name = "Eğitim Fakültesi", University = u1111 });
            faculties.Add(new Faculty { Name = "Fen-Edebiyat Fakültesi", University = u1111 });
            faculties.Add(new Faculty { Name = "Hukuk Fakültesi", University = u1111 });
            faculties.Add(new Faculty { Name = "Mimarlık Fakültesi", University = u1111 });
            faculties.Add(new Faculty { Name = "Mühendislik Fakültesi", University = u1111 });
            faculties.Add(new Faculty { Name = "Sağlık Bilimleri Fakültesi", University = u1111 });
            faculties.Add(new Faculty { Name = "Tıp Fakültesi", University = u1111 });
            faculties.Add(new Faculty { Name = "İktisadi ve İdari Bilimler Fakültesi", University = u1111 });
        }

        // Niğde Ömer Halisdemir Üniversitesi
        if (uniLookup.TryGetValue("Niğde Ömer Halisdemir Üniversitesi", out var u1112))
        {
            faculties.Add(new Faculty { Name = "Güzel Sanatlar Tasarım ve Mimarlık Fakültesi", University = u1112 });
            faculties.Add(new Faculty { Name = "Mühendislik ve Doğa Bilimleri Fakültesi", University = u1112 });
            faculties.Add(new Faculty { Name = "Sağlık Bilimleri Fakültesi", University = u1112 });
            faculties.Add(new Faculty { Name = "Tıp Fakültesi", University = u1112 });
            faculties.Add(new Faculty { Name = "İktisadi, İdari ve Sosyal Bilimler Fakültesi", University = u1112 });
            faculties.Add(new Faculty { Name = "İlahiyat Fakültesi", University = u1112 });
        }

        // Nişantaşı Üniversitesi
        if (uniLookup.TryGetValue("Nişantaşı Üniversitesi", out var u2054))
        {
            faculties.Add(new Faculty { Name = "Fen-Edebiyat Fakültesi", University = u2054 });
            faculties.Add(new Faculty { Name = "Hukuk Fakültesi", University = u2054 });
            faculties.Add(new Faculty { Name = "Mimarlık ve Tasarım Fakültesi", University = u2054 });
            faculties.Add(new Faculty { Name = "Mühendislik Fakültesi", University = u2054 });
            faculties.Add(new Faculty { Name = "Tıp Fakültesi", University = u2054 });
            faculties.Add(new Faculty { Name = "İktisadi ve İdari Bilimler Fakültesi", University = u2054 });
        }

        // Nuh Naci Yazgan Üniversitesi
        if (uniLookup.TryGetValue("Nuh Naci Yazgan Üniversitesi", out var u1113))
        {
            faculties.Add(new Faculty { Name = "Bilgisayar ve Bilişim Bilimleri Fakültesi", University = u1113 });
            faculties.Add(new Faculty { Name = "Mimarlık ve Tasarım Fakültesi", University = u1113 });
            faculties.Add(new Faculty { Name = "Mühendislik ve Doğa Bilimleri Fakültesi", University = u1113 });
        }

        // Ondokuz Mayıs Üniversitesi
        if (uniLookup.TryGetValue("Ondokuz Mayıs Üniversitesi", out var u1114))
        {
            faculties.Add(new Faculty { Name = "Diş Hekimliği Fakültesi", University = u1114 });
            faculties.Add(new Faculty { Name = "Mühendislik ve Doğa Bilimleri Fakültesi", University = u1114 });
            faculties.Add(new Faculty { Name = "Sağlık Bilimleri Fakültesi", University = u1114 });
            faculties.Add(new Faculty { Name = "Tıp Fakültesi", University = u1114 });
        }

        // Ordu Üniversitesi
        if (uniLookup.TryGetValue("Ordu Üniversitesi", out var u1115))
        {
            faculties.Add(new Faculty { Name = "Mühendislik ve Doğa Bilimleri Fakültesi", University = u1115 });
            faculties.Add(new Faculty { Name = "Sanat, Tasarım ve Mimarlık Fakültesi", University = u1115 });
            faculties.Add(new Faculty { Name = "Sağlık Bilimleri Fakültesi", University = u1115 });
            faculties.Add(new Faculty { Name = "Sivil Havacılık Yüksekokulu", University = u1115 });
            faculties.Add(new Faculty { Name = "Sosyal ve Beşeri Bilimler Fakültesi", University = u1115 });
            faculties.Add(new Faculty { Name = "Tıp Fakültesi", University = u1115 });
            faculties.Add(new Faculty { Name = "Ziraat Fakültesi", University = u1115 });
        }

        // Orta Doğu Teknik Üniversitesi
        if (uniLookup.TryGetValue("Orta Doğu Teknik Üniversitesi", out var u1116))
        {
            faculties.Add(new Faculty { Name = "Cerrahpaşa Tıp Fakültesi", University = u1116 });
            faculties.Add(new Faculty { Name = "Diş Hekimliği Fakültesi", University = u1116 });
            faculties.Add(new Faculty { Name = "Eczacılık Fakültesi", University = u1116 });
            faculties.Add(new Faculty { Name = "Florence Nightingale Hemşirelik Fakültesi", University = u1116 });
            faculties.Add(new Faculty { Name = "Hasan Ali Yücel Eğitim Fakültesi", University = u1116 });
            faculties.Add(new Faculty { Name = "Mühendislik Fakültesi", University = u1116 });
            faculties.Add(new Faculty { Name = "Orman Fakültesi", University = u1116 });
            faculties.Add(new Faculty { Name = "Sağlık Bilimleri Fakültesi", University = u1116 });
            faculties.Add(new Faculty { Name = "Spor Bilimleri Fakültesi", University = u1116 });
            faculties.Add(new Faculty { Name = "Veteriner Fakültesi", University = u1116 });
        }

        // Osmaniye Korkut Ata Üniversitesi
        if (uniLookup.TryGetValue("Osmaniye Korkut Ata Üniversitesi", out var u1118))
        {
            faculties.Add(new Faculty { Name = "Spor Bilimleri Fakültesi", University = u1118 });
            faculties.Add(new Faculty { Name = "Teknoloji Fakültesi", University = u1118 });
            faculties.Add(new Faculty { Name = "Uygulamalı Bilimler Fakültesi", University = u1118 });
            faculties.Add(new Faculty { Name = "Ziraat Fakültesi", University = u1118 });
        }

        // Özyeğın Üniversitesi
        if (uniLookup.TryGetValue("Özyeğın Üniversitesi", out var u1119))
        {
            faculties.Add(new Faculty { Name = "Havacılık ve Uzay Bilimleri Fakültesi", University = u1119 });
            faculties.Add(new Faculty { Name = "Mühendislik ve Doğa Bilimleri Fakültesi", University = u1119 });
            faculties.Add(new Faculty { Name = "Tarım Bilimleri ve Teknoloji Fakültesi", University = u1119 });
        }

        // Piri Reis Üniversitesi
        if (uniLookup.TryGetValue("Piri Reis Üniversitesi", out var u1120))
        {
            faculties.Add(new Faculty { Name = "Havacılık ve Uzay Bilimleri Fakültesi", University = u1120 });
            faculties.Add(new Faculty { Name = "Mühendislik Fakültesi", University = u1120 });
            faculties.Add(new Faculty { Name = "Sağlık Bilimleri Fakültesi", University = u1120 });
            faculties.Add(new Faculty { Name = "Uygulamalı Bilimler Fakültesi", University = u1120 });
            faculties.Add(new Faculty { Name = "İktisadi ve İdari Bilimler Fakültesi", University = u1120 });
        }

        // Rauf Denktaş Üniversitesi
        if (uniLookup.TryGetValue("Rauf Denktaş Üniversitesi", out var u3020))
        {
            faculties.Add(new Faculty { Name = "Güzel Sanatlar ve Tasarım Fakültesi", University = u3020 });
            faculties.Add(new Faculty { Name = "Hukuk Fakültesi", University = u3020 });
            faculties.Add(new Faculty { Name = "Mühendislik Fakültesi", University = u3020 });
            faculties.Add(new Faculty { Name = "Sağlık Bilimleri Fakültesi", University = u3020 });
            faculties.Add(new Faculty { Name = "Turizm Fakültesi", University = u3020 });
            faculties.Add(new Faculty { Name = "İktisadi, İdari ve Sosyal Bilimler Fakültesi", University = u3020 });
        }

        // Recep Tayyip Erdoğan Üniversitesi
        if (uniLookup.TryGetValue("Recep Tayyip Erdoğan Üniversitesi", out var u1121))
        {
            faculties.Add(new Faculty { Name = "Bilgisayar ve Bilişim Bilimleri Fakültesi", University = u1121 });
            faculties.Add(new Faculty { Name = "Fatih Eğitim Fakültesi", University = u1121 });
            faculties.Add(new Faculty { Name = "Hukuk Fakültesi", University = u1121 });
            faculties.Add(new Faculty { Name = "Siyasal Bilgiler Fakültesi", University = u1121 });
            faculties.Add(new Faculty { Name = "Spor Bilimleri Fakültesi", University = u1121 });
            faculties.Add(new Faculty { Name = "Tıp Fakültesi", University = u1121 });
            faculties.Add(new Faculty { Name = "Uygulamalı Bilimler Yüksekokulu", University = u1121 });
            faculties.Add(new Faculty { Name = "İlahiyat Fakültesi", University = u1121 });
            faculties.Add(new Faculty { Name = "İletişim Fakültesi", University = u1121 });
            faculties.Add(new Faculty { Name = "İnsan ve Toplum Bilimleri Fakültesi", University = u1121 });
        }

        // Sabancı Üniversitesi
        if (uniLookup.TryGetValue("Sabancı Üniversitesi", out var u1122))
        {
            faculties.Add(new Faculty { Name = "Mühendislik, Mimarlık ve Tasarım Fakültesi", University = u1122 });
            faculties.Add(new Faculty { Name = "Sağlık Bilimleri Fakültesi", University = u1122 });
            faculties.Add(new Faculty { Name = "Turizm Fakültesi", University = u1122 });
            faculties.Add(new Faculty { Name = "İletişim Fakültesi", University = u1122 });
            faculties.Add(new Faculty { Name = "İnsan ve Toplum Bilimleri Fakültesi", University = u1122 });
        }

        // Sakarya Uygulamalı Bilimler Üniversitesi
        if (uniLookup.TryGetValue("Sakarya Uygulamalı Bilimler Üniversitesi", out var u1124))
        {
            faculties.Add(new Faculty { Name = "Bilgisayar ve Bilişim Bilimleri Fakültesi", University = u1124 });
            faculties.Add(new Faculty { Name = "Fen Fakültesi", University = u1124 });
            faculties.Add(new Faculty { Name = "Havacılık ve Uzay Bilimleri Fakültesi", University = u1124 });
            faculties.Add(new Faculty { Name = "Mimarlık ve Tasarım Fakültesi", University = u1124 });
            faculties.Add(new Faculty { Name = "Mühendislik Fakültesi", University = u1124 });
            faculties.Add(new Faculty { Name = "Spor Bilimleri Fakültesi", University = u1124 });
        }

        // Sakarya Üniversitesi
        if (uniLookup.TryGetValue("Sakarya Üniversitesi", out var u1123))
        {
            faculties.Add(new Faculty { Name = "Bilgisayar ve Bilişim Bilimleri Fakültesi", University = u1123 });
            faculties.Add(new Faculty { Name = "Develi Sosyal ve Beşeri Bilimler Fakültesi", University = u1123 });
            faculties.Add(new Faculty { Name = "Develi İslami İlimler Fakültesi", University = u1123 });
            faculties.Add(new Faculty { Name = "Mühendislik, Mimarlık ve Tasarım Fakültesi", University = u1123 });
            faculties.Add(new Faculty { Name = "Sağlık Bilimleri Fakültesi", University = u1123 });
            faculties.Add(new Faculty { Name = "Uygulamalı Bilimler Fakültesi", University = u1123 });
        }

        // Samsun Üniversitesi
        if (uniLookup.TryGetValue("Samsun Üniversitesi", out var u1125))
        {
            faculties.Add(new Faculty { Name = "Eğirdir Su Ürünleri Fakültesi", University = u1125 });
            faculties.Add(new Faculty { Name = "Orman Fakültesi", University = u1125 });
            faculties.Add(new Faculty { Name = "Teknoloji Fakültesi", University = u1125 });
            faculties.Add(new Faculty { Name = "Ziraat Fakültesi", University = u1125 });
        }

        // Sanko Üniversitesi
        if (uniLookup.TryGetValue("Sanko Üniversitesi", out var u2061))
        {
            faculties.Add(new Faculty { Name = "Bilgisayar ve Bilişim Bilimleri Fakültesi", University = u2061 });
            faculties.Add(new Faculty { Name = "Diş Hekimliği Fakültesi", University = u2061 });
            faculties.Add(new Faculty { Name = "Eczacılık Fakültesi", University = u2061 });
            faculties.Add(new Faculty { Name = "Eğitim Fakültesi", University = u2061 });
            faculties.Add(new Faculty { Name = "Fen-Edebiyat Fakültesi", University = u2061 });
            faculties.Add(new Faculty { Name = "Güzel Sanatlar Fakültesi", University = u2061 });
            faculties.Add(new Faculty { Name = "Hukuk Fakültesi", University = u2061 });
            faculties.Add(new Faculty { Name = "Mimarlık Fakültesi", University = u2061 });
            faculties.Add(new Faculty { Name = "Mühendislik Fakültesi", University = u2061 });
            faculties.Add(new Faculty { Name = "Sağlık Bilimleri Fakültesi", University = u2061 });
            faculties.Add(new Faculty { Name = "Tıp Fakültesi", University = u2061 });
            faculties.Add(new Faculty { Name = "İktisadi ve İdari Bilimler Fakültesi", University = u2061 });
            faculties.Add(new Faculty { Name = "İletişim Fakültesi", University = u2061 });
        }

        // Selçuk Üniversitesi
        if (uniLookup.TryGetValue("Selçuk Üniversitesi", out var u1127))
        {
            faculties.Add(new Faculty { Name = "Mimarlık ve Tasarım Fakültesi", University = u1127 });
            faculties.Add(new Faculty { Name = "Mühendislik ve Doğa Bilimleri Fakültesi", University = u1127 });
            faculties.Add(new Faculty { Name = "Sivil Havacılık Yüksekokulu", University = u1127 });
            faculties.Add(new Faculty { Name = "Siyasal Bilgiler Fakültesi", University = u1127 });
            faculties.Add(new Faculty { Name = "Tıp Fakültesi", University = u1127 });
            faculties.Add(new Faculty { Name = "Özdemir Bayraktar Havacılık ve Uzay Bilimleri Fakültesi", University = u1127 });
            faculties.Add(new Faculty { Name = "İlahiyat Fakültesi", University = u1127 });
            faculties.Add(new Faculty { Name = "İnsan ve Toplum Bilimleri Fakültesi", University = u1127 });
        }

        // Siirt Üniversitesi
        if (uniLookup.TryGetValue("Siirt Üniversitesi", out var u1129))
        {
            faculties.Add(new Faculty { Name = "Hukuk Fakültesi", University = u1129 });
            faculties.Add(new Faculty { Name = "Mühendislik ve Mimarlık Fakültesi", University = u1129 });
            faculties.Add(new Faculty { Name = "Sağlık Bilimleri Fakültesi", University = u1129 });
            faculties.Add(new Faculty { Name = "Tıp Fakültesi", University = u1129 });
            faculties.Add(new Faculty { Name = "İktisadi ve İdari Bilimler Fakültesi", University = u1129 });
            faculties.Add(new Faculty { Name = "İnsan ve Toplum Bilimleri Fakültesi", University = u1129 });
        }

        // Ted Üniversitesi
        if (uniLookup.TryGetValue("Ted Üniversitesi", out var u2062))
        {
            faculties.Add(new Faculty { Name = "Diş Hekimliği Fakültesi", University = u2062 });
            faculties.Add(new Faculty { Name = "Eczacılık Fakültesi", University = u2062 });
            faculties.Add(new Faculty { Name = "Fen-Edebiyat Fakültesi", University = u2062 });
            faculties.Add(new Faculty { Name = "Hukuk Fakültesi", University = u2062 });
            faculties.Add(new Faculty { Name = "Mühendislik-Mimarlık Fakültesi", University = u2062 });
            faculties.Add(new Faculty { Name = "Sağlık Bilimleri Fakültesi", University = u2062 });
            faculties.Add(new Faculty { Name = "Spor Bilimleri Fakültesi", University = u2062 });
            faculties.Add(new Faculty { Name = "Tıp Fakültesi", University = u2062 });
            faculties.Add(new Faculty { Name = "İktisadi ve İdari Bilimler Fakültesi", University = u2062 });
            faculties.Add(new Faculty { Name = "İletişim Fakültesi", University = u2062 });
        }

        // Toros Üniversitesi
        if (uniLookup.TryGetValue("Toros Üniversitesi", out var u2065))
        {
            faculties.Add(new Faculty { Name = "Diş Hekimliği Fakültesi", University = u2065 });
            faculties.Add(new Faculty { Name = "Mühendislik ve Doğa Bilimleri Fakültesi", University = u2065 });
            faculties.Add(new Faculty { Name = "Sağlık Bilimleri Fakültesi", University = u2065 });
            faculties.Add(new Faculty { Name = "Tıp Fakültesi", University = u2065 });
            faculties.Add(new Faculty { Name = "İletişim Fakültesi", University = u2065 });
            faculties.Add(new Faculty { Name = "İnsan ve Toplum Bilimleri Fakültesi", University = u2065 });
        }

        // Trakya Üniversitesi
        if (uniLookup.TryGetValue("Trakya Üniversitesi", out var u4096))
        {
            faculties.Add(new Faculty { Name = "Diş Hekimliği Fakültesi", University = u4096 });
            faculties.Add(new Faculty { Name = "Eğitim Fakültesi", University = u4096 });
            faculties.Add(new Faculty { Name = "Hukuk Fakültesi", University = u4096 });
            faculties.Add(new Faculty { Name = "Mühendislik Fakültesi", University = u4096 });
            faculties.Add(new Faculty { Name = "Sanat ve Tasarım Fakültesi", University = u4096 });
            faculties.Add(new Faculty { Name = "Sosyal ve Beşeri Bilimler Fakültesi", University = u4096 });
            faculties.Add(new Faculty { Name = "İktisadi ve İdari Bilimler Fakültesi", University = u4096 });
        }

        // Ufuk Üniversitesi
        if (uniLookup.TryGetValue("Ufuk Üniversitesi", out var u2067))
        {
            faculties.Add(new Faculty { Name = "Eğitim Fakültesi", University = u2067 });
            faculties.Add(new Faculty { Name = "Fen-Edebiyat Fakültesi", University = u2067 });
            faculties.Add(new Faculty { Name = "Mimarlık ve Tasarım Fakültesi", University = u2067 });
            faculties.Add(new Faculty { Name = "Mühendislik Fakültesi", University = u2067 });
            faculties.Add(new Faculty { Name = "İktisadi ve İdari Bilimler Fakültesi", University = u2067 });
        }

        // Uluslararası Kıbrıs Üniversitesi
        if (uniLookup.TryGetValue("Uluslararası Kıbrıs Üniversitesi", out var u3003))
        {
            faculties.Add(new Faculty { Name = "Beden Eğitimi ve Spor Yüksekokulu", University = u3003 });
            faculties.Add(new Faculty { Name = "Diş Hekimliği Fakültesi", University = u3003 });
            faculties.Add(new Faculty { Name = "Dr. Fazıl Küçük Eğitim Fakültesi", University = u3003 });
            faculties.Add(new Faculty { Name = "Fen ve Edebiyat Fakültesi", University = u3003 });
            faculties.Add(new Faculty { Name = "Hemşirelik Yüksekokulu", University = u3003 });
            faculties.Add(new Faculty { Name = "Hukuk Fakültesi", University = u3003 });
            faculties.Add(new Faculty { Name = "Mimarlık Fakültesi", University = u3003 });
            faculties.Add(new Faculty { Name = "Mühendislik Fakültesi", University = u3003 });
            faculties.Add(new Faculty { Name = "Sağlık Bilimleri Fakültesi", University = u3003 });
            faculties.Add(new Faculty { Name = "Tarım Bilimleri ve Teknolojileri Fakültesi", University = u3003 });
            faculties.Add(new Faculty { Name = "Turizm ve Otel İşletmeciliği Yüksekokulu", University = u3003 });
            faculties.Add(new Faculty { Name = "Uygulamalı Bilimler Yüksekokulu", University = u3003 });
            faculties.Add(new Faculty { Name = "İktisadi ve İdari Bilimler Fakültesi", University = u3003 });
            faculties.Add(new Faculty { Name = "İletişim Bilimleri Fakültesi", University = u3003 });
        }

        // Uluslararası Orta Doğu Üniversitesi
        if (uniLookup.TryGetValue("Uluslararası Orta Doğu Üniversitesi", out var u3006))
        {
            faculties.Add(new Faculty { Name = "İşletme Fakültesi", University = u3006 });
        }

        // Üsküdar Üniversitesi
        if (uniLookup.TryGetValue("Üsküdar Üniversitesi", out var u2072))
        {
            faculties.Add(new Faculty { Name = "Hukuk Fakültesi", University = u2072 });
            faculties.Add(new Faculty { Name = "Mühendislik Fakültesi", University = u2072 });
            faculties.Add(new Faculty { Name = "Sanat, Tasarım ve Mimarlık Fakültesi", University = u2072 });
            faculties.Add(new Faculty { Name = "İktisadi, İdari ve Sosyal Bilimler Fakültesi", University = u2072 });
        }

        // Yakın Doğu Üniversitesi
        if (uniLookup.TryGetValue("Yakın Doğu Üniversitesi", out var u3004))
        {
            faculties.Add(new Faculty { Name = "Diş Hekimliği Fakültesi", University = u3004 });
            faculties.Add(new Faculty { Name = "Eczacılık Fakültesi", University = u3004 });
            faculties.Add(new Faculty { Name = "Eğitim Fakültesi", University = u3004 });
            faculties.Add(new Faculty { Name = "Fen-Edebiyat Fakültesi", University = u3004 });
            faculties.Add(new Faculty { Name = "Güzel Sanatlar Tasarım ve Mimarlık Fakültesi", University = u3004 });
            faculties.Add(new Faculty { Name = "Hukuk Fakültesi", University = u3004 });
            faculties.Add(new Faculty { Name = "Mühendislik Fakültesi", University = u3004 });
            faculties.Add(new Faculty { Name = "Sağlık Bilimleri Fakültesi", University = u3004 });
            faculties.Add(new Faculty { Name = "Sağlık Bilimleri Yüksekokulu", University = u3004 });
            faculties.Add(new Faculty { Name = "Tarım Bilimleri ve Teknolojileri Fakültesi", University = u3004 });
            faculties.Add(new Faculty { Name = "Turizm ve Otel İşletmeciliği Yüksekokulu", University = u3004 });
            faculties.Add(new Faculty { Name = "Uygulamalı Bilimler Yüksekokulu", University = u3004 });
            faculties.Add(new Faculty { Name = "İktisadi ve İdari Bilimler Fakültesi", University = u3004 });
            faculties.Add(new Faculty { Name = "İletişim Fakültesi", University = u3004 });
        }

        // Yaşar Üniversitesi
        if (uniLookup.TryGetValue("Yaşar Üniversitesi", out var u2075))
        {
            faculties.Add(new Faculty { Name = "Mühendislik ve Mimarlık Fakültesi", University = u2075 });
            faculties.Add(new Faculty { Name = "Sanat ve Sosyal Bilimler Fakültesi", University = u2075 });
            faculties.Add(new Faculty { Name = "Sağlık Bilimleri Fakültesi", University = u2075 });
            faculties.Add(new Faculty { Name = "Spor Bilimleri Fakültesi", University = u2075 });
            faculties.Add(new Faculty { Name = "Uygulamalı Bilimler Yüksekokulu", University = u2075 });
            faculties.Add(new Faculty { Name = "İşletme ve Yönetim Bilimleri Fakültesi", University = u2075 });
        }

        // Yeditepe Üniversitesi
        if (uniLookup.TryGetValue("Yeditepe Üniversitesi", out var u2077))
        {
            faculties.Add(new Faculty { Name = "Sağlık Bilimleri Fakültesi", University = u2077 });
            faculties.Add(new Faculty { Name = "Tıp Fakültesi", University = u2077 });
        }

        // Yıldırım Beyazıt Üniversitesi
        if (uniLookup.TryGetValue("Yıldırım Beyazıt Üniversitesi", out var u4099))
        {
            faculties.Add(new Faculty { Name = "Hukuk ve Sosyal Bilimler Fakültesi", University = u4099 });
            faculties.Add(new Faculty { Name = "Mühendislik ve Mimarlık Fakültesi", University = u4099 });
        }

        // Yüksek İhtisas Üniversitesi
        if (uniLookup.TryGetValue("Yüksek İhtisas Üniversitesi", out var u2079))
        {
            faculties.Add(new Faculty { Name = "Sağlık Bilimleri Fakültesi", University = u2079 });
            faculties.Add(new Faculty { Name = "Tıp Fakültesi", University = u2079 });
        }

        await context.Faculties.AddRangeAsync(faculties);
        await context.SaveChangesAsync();
    }

    private static async Task SeedAdminUserAsync(AppDbContext context)
    {
        if (context.Users.Any(u => u.Role == UserRole.Admin)) return;

        await context.Users.AddAsync(new User
        {
            Username        = "admin",
            Email           = "admin@hocapuan.com",
            PasswordHash    = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
            Role            = UserRole.Admin,
            IsEmailVerified = true
        });
        await context.SaveChangesAsync();
    }
}