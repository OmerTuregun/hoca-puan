# Hocanı Yorumla

Türkiye'deki üniversite hocaları hakkında anonim değerlendirme ve yorum platformu. Öğrenciler ders deneyimlerini paylaşır; içerik moderasyonu, şikayet sistemi ve admin paneli ile topluluk güvenliği korunur.

> Eski proje adı: **HocaPuan** (repo ve kod ad alanı hâlâ `HocaPuan` kullanır).

## Teknoloji yığını

| Katman | Teknoloji |
|--------|-----------|
| Backend | ASP.NET Core (.NET 10), EF Core, PostgreSQL |
| Frontend | React 18, TypeScript, Vite, Tailwind CSS, TanStack Query |
| Altyapı | Docker Compose, nginx (production frontend image), GitHub Actions CI |
| Veri toplama | YÖK Atlas Playwright scraper, veri kalitesi raporlama |

## Özellikler

- **Üniversite / fakülte / bölüm / hoca** hiyerarşisi, arama ve autocomplete
- **Yorumlar:** puanlama, etiketler, güncellik oylaması, kullanıcı katkı geçmişi
- **İçerik moderasyonu:** Türkçe küfür/hakaret listesi, fuzzy matching (Levenshtein), kök eşleştirme, otomatik red / manuel inceleme
- **Kimlik doğrulama:** JWT + httpOnly cookie, CSRF koruması, hesap kilitleme (başarısız giriş)
- **Rate limiting:** auth, yorum yazma, şikayet bildirme ve global IP limitleri
- **Şikayet sistemi:** kullanıcı başına bildirim, 3 farklı kullanıcıda otomatik inceleme eşiği
- **Admin / moderasyon paneli:** bekleyen yorumlar, onay/red
- **SEO:** üniversite, fakülte, bölüm ve hoca sayfaları (slug URL'ler)
- **Veri kalitesi:** `DataQualityReportService` ve `scripts/diag-data-quality.sh` ile bozuk fakülte/bölüm tespiti
- **YÖK import:** `YokPlaywrightScraperService`, fakülte/bölüm adı doğrulama ve temizlik servisleri

## Hızlı başlangıç (geliştirme)

### Gereksinimler

- Docker ve Docker Compose
- (İsteğe bağlı) .NET 10 SDK ve Node.js 20+ — Docker dışı yerel geliştirme için

### Kurulum

```bash
git clone https://github.com/OmerTuregun/hoca-puan.git
cd hoca-puan

cp .env.example .env.development
# JWT_SECRET, DB_PASSWORD, e-posta ayarlarını düzenleyin

docker compose --env-file .env.development -f docker-compose-dev.yml up --build -d
```

| Servis | URL |
|--------|-----|
| Frontend (Vite) | http://localhost:5173 |
| API (+ Swagger) | http://localhost:5001 |
| pgAdmin | http://localhost:5051 |
| PostgreSQL | localhost:5433 (`.env.development` → `DB_PORT`) |

İlk API başlatılışında EF Core migration'ları ve seed verisi otomatik uygulanır.

Docker dışı backend için proje kökünde `.env.development` gerekir; `ASPNETCORE_ENVIRONMENT=Development` iken `EnvLoader` bu dosyayı okur.

```bash
cd hocapuan-backend
dotnet run --project src/HocaPuan.API
```

Frontend:

```bash
cd hocapuan-frontend
npm install
npm run dev
```

## Production deployment

Production ortamı `docker-compose-prod.yml` ve `.env.production` ile yönetilir. nginx tabanlı frontend image'ı, HTTPS (Certbot), Cloudflare ve ortam değişkenleri ayrıntıları için **[DEPLOYMENT.md](DEPLOYMENT.md)** dosyasına bakın — bu README'de tekrarlanmaz.

## CI/CD

Backend testleri GitHub Actions ile çalışır:

- Workflow: [`.github/workflows/ci.yml`](.github/workflows/ci.yml)
- `main` push ve PR'larda tetiklenir
- `dotnet restore` → `build` → izole PostgreSQL üzerinde migration → `dotnet test`
- Otomatik deploy **yok**; yalnızca build/test doğrulaması
- Frontend test altyapısı henüz yok; CI yalnızca backend'i kapsar

## Proje yapısı

```
hoca-puan/
├── hocapuan-backend/          # ASP.NET Core API
│   ├── src/
│   │   ├── HocaPuan.Core/     # Entity, DTO, arayüzler, validasyon
│   │   ├── HocaPuan.Data/     # DbContext, migration, seed
│   │   ├── HocaPuan.Services/ # İş mantığı, scraper, moderasyon
│   │   └── HocaPuan.API/      # Controllers, middleware, Program
│   └── tests/                 # xUnit birim ve entegrasyon testleri
├── hocapuan-frontend/         # React + Vite SPA
├── scripts/                   # Veri kalitesi, yedekleme, diag araçları
├── docker-compose-dev.yml
├── docker-compose-prod.yml
├── DEPLOYMENT.md
└── .github/workflows/ci.yml
```

## Katkıda bulunma

1. `main` dalından feature branch açın
2. Değişiklikleriniz için backend testlerinin geçtiğinden emin olun (`dotnet test hocapuan-backend/HocaPuan.sln`)
3. Pull request açın — CI sonucu PR üzerinde görünür

## Lisans

Bu proje şu an özel / kapalı geliştirme aşamasındadır. Lisans bilgisi eklendiğinde bu bölüm güncellenecektir.
