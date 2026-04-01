# 🎓 HocaPuan Backend

Türkiye üniversitelerindeki hocaları değerlendirme platformu — ASP.NET Core 8 + PostgreSQL + Docker.

---

## 🏗️ Proje Mimarisi

```
HocaPuan.sln
├── src/
│   ├── HocaPuan.Core        → Entity'ler, DTO'lar, Interface'ler
│   ├── HocaPuan.Data        → DbContext, EF Configurations, Seed
│   ├── HocaPuan.Services    → İş mantığı (AuthService, ProfessorService...)
│   └── HocaPuan.API         → Controllers, Middleware, Program.cs
├── docker-compose.yml
├── .env.example
└── .env                     ← Bunu kendin oluştur
```

---

## 🚀 Hızlı Başlangıç

### 1. Ortam değişkenlerini ayarla
```bash
cp .env.example .env
# .env dosyasını düzenle (şifreleri değiştir)
```

### 2. Docker ile ayağa kaldır
```bash
docker-compose up -d --build
```

### 3. Servisleri kontrol et
| Servis    | URL                          |
|-----------|------------------------------|
| API + Swagger | http://localhost:5001    |
| pgAdmin   | http://localhost:5051        |

İlk çalıştırmada otomatik olarak:
- PostgreSQL migration'ları çalışır
- 20 Türkiye üniversitesi seed edilir
- Admin kullanıcısı oluşturulur (`admin@hocapuan.com` / `Admin123!`)

---

## 📡 API Endpointleri

### Auth
| Method | Endpoint                       | Açıklama               |
|--------|--------------------------------|------------------------|
| POST   | `/api/auth/register`           | Kayıt ol               |
| POST   | `/api/auth/login`              | Giriş yap (JWT al)     |
| GET    | `/api/auth/verify-email/{token}` | E-posta doğrula      |
| POST   | `/api/auth/forgot-password`    | Şifremi unuttum        |
| POST   | `/api/auth/reset-password`     | Şifre sıfırla          |

### Üniversiteler
| Method | Endpoint                                    | Açıklama              |
|--------|---------------------------------------------|-----------------------|
| GET    | `/api/universities`                         | Listele (arama var)   |
| GET    | `/api/universities/{id}`                    | Detay                 |
| GET    | `/api/universities/{id}/faculties`          | Fakülteler            |
| GET    | `/api/universities/faculties/{id}/departments` | Bölümler           |
| POST   | `/api/universities`                         | Ekle (Admin)          |
| PUT    | `/api/universities/{id}`                    | Güncelle (Admin)      |
| DELETE | `/api/universities/{id}`                    | Sil (Admin)           |

### Hocalar
| Method | Endpoint                | Açıklama                             |
|--------|-------------------------|--------------------------------------|
| GET    | `/api/professors`       | Ara/listele (`?query=&universityId=`) |
| GET    | `/api/professors/{id}`  | Detay                                |
| POST   | `/api/professors`       | Ekle (Admin/Moderator)               |
| PUT    | `/api/professors/{id}`  | Güncelle (Admin/Moderator)           |
| DELETE | `/api/professors/{id}`  | Sil (Admin)                          |

### Yorumlar
| Method | Endpoint                           | Açıklama               |
|--------|------------------------------------|------------------------|
| GET    | `/api/reviews/professor/{id}`      | Hoca yorumları         |
| GET    | `/api/reviews/{id}`                | Yorum detayı           |
| POST   | `/api/reviews`                     | Yorum ekle (Auth)      |
| DELETE | `/api/reviews/{id}`                | Yorum sil (Auth)       |
| POST   | `/api/reviews/{id}/vote?isUpvote=` | 👍/👎 oy (Auth)        |
| POST   | `/api/reviews/{id}/moderate`       | Onayla/Reddet (Admin)  |
| GET    | `/api/reviews/pending`             | Bekleyen yorumlar (Admin) |

---

## 🛠️ EF Core Migration

```bash
# Container içinde veya dotnet CLI ile:
cd src/HocaPuan.API

dotnet ef migrations add InitialCreate \
  --project ../HocaPuan.Data \
  --startup-project .

dotnet ef database update \
  --project ../HocaPuan.Data \
  --startup-project .
```

---

## 📋 Türkçe Yorum Etiketleri (Önerilen)

```
Çok Ödev Verir | Az Ödev Verir | Proje Odaklı | Sınava Dayalı
Devama Dikkat Eder | Devam Şart Değil | İlham Verici | Sıkıcı
Notlar Kolay Anlaşılır | Ders Kitabına Bağlı | Pratik Ağırlıklı
Yardımsever | Ulaşılması Zor | Online Kaynaklar Sağlıyor
```

---

## 🔮 Roadmap

- [ ] E-posta doğrulaması (SMTP entegrasyonu)
- [ ] `.edu.tr` doğrulama aktif etme
- [ ] Rate limiting (spam önleme)
- [ ] YÖK'ten tam üniversite listesi import
- [ ] Moderasyon kuyruğu aktif etme (`ReviewStatus.Pending`)
- [ ] Redis cache (arama performansı)
- [ ] Hoca önerme formu (kullanıcıdan gelecek)
- [ ] Frontend (React + Vite)
