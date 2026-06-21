# HocaPuan — Deployment

Docker Compose yapılandırması geliştirme ve production için ayrılmıştır.

> **Not:** Kök dizindeki `docker-compose.yml` kaldırıldı. Geliştirme artık yalnızca
> `docker-compose.dev.yml` ile çalıştırılır:
> `docker compose -f docker-compose.dev.yml up --build -d`

| Dosya | Ortam | Env dosyası |
|-------|-------|-------------|
| `docker-compose.dev.yml` | Local geliştirme | `.env.development` |
| `docker-compose.prod.yml` | Sunucu (IP + HTTP) | `.env.production` |

## İlk kurulum

```bash
# Geliştirme
cp .env.example .env.development
# Değerleri düzenleyin (JWT, DB şifresi, Mailtrap vb.)

# Production
cp .env.example .env.production
# Aşağıdaki zorunlu alanları sunucuya göre doldurun
```

## Geliştirme

```bash
docker compose -f docker-compose.dev.yml up --build -d
```

- Frontend: http://localhost:5173 (Vite dev server, yalnızca localhost)
- API: http://localhost:5001
- pgAdmin: http://localhost:5051
- PostgreSQL: localhost:5433

`dotnet run` (Docker dışı) için proje kökünde `.env.development` veya `.env` bulunmalıdır. `ASPNETCORE_ENVIRONMENT=Development` iken `EnvLoader` önce `.env.development` dosyasını arar.

## Production

```bash
docker compose -f docker-compose.prod.yml up --build -d
```

- Frontend (nginx + static): `http://SUNUCU_IP:${FRONTEND_PORT}` (varsayılan 8080)
- API: `http://SUNUCU_IP:${API_PORT}` (varsayılan 5001; tarayıcı trafiği nginx üzerinden `/api` ile gider)
- pgAdmin **yok** (yalnızca dev compose'da)
- Servisler `restart: always` ile çalışır

### `.env.production` içinde mutlaka güncellenmesi gerekenler

| Değişken | Açıklama |
|----------|----------|
| `DB_PASSWORD` | Güçlü PostgreSQL şifresi (`CHANGE_ME_STRONG_PASSWORD` yerine) |
| `JWT_SECRET` | Uzun rastgele secret (`CHANGE_ME_GENERATE_STRONG_SECRET` yerine) |
| `API_PORT` | Sunucuda müsait API host portu (varsayılan: 5001) |
| `FRONTEND_PORT` | Sunucuda müsait frontend host portu (varsayılan: 8080) |
| `DB_PORT` | Sunucuda müsait DB host portu (varsayılan: 5433) |
| `ALLOWED_ORIGIN` | `http://GERÇEK_IP:FRONTEND_PORT` (CORS) |
| `APP_FRONTEND_URL` | E-posta linkleri için aynı adres |
| `EMAIL_*` | Production SMTP bilgileri |

### HTTPS / domain eklendiğinde

Kod veya compose dosyası değiştirmeden `.env.production` içinde:

```env
USE_HTTPS=true
COOKIE_SECURE=true
AUTH_COOKIE_SAMESITE=Strict
ALLOWED_ORIGIN=https://alanadiniz.com
APP_FRONTEND_URL=https://alanadiniz.com
```

Bu değişkenler API cookie `Secure` bayrağını, CSRF cookie politikasını ve CORS origin listesini günceller.

## Ortam değişkenleri özeti

Tüm değişkenler için referans: `.env.example`

Cookie / HTTPS:

- `USE_HTTPS` — `true` olunca cookie `Secure=true` ve (ayrıca set edilmediyse) `SameSite=Strict`
- `COOKIE_SECURE` — `USE_HTTPS` üzerinde önceliklidir; doğrudan cookie Secure bayrağını kontrol eder
- `AUTH_COOKIE_SAMESITE` — `Strict`, `Lax` veya `None`

CORS:

- Development: `appsettings.Development.json` içindeki localhost origin listesi
- Production: `ALLOWED_ORIGIN` (veya `Cors__AllowedOrigins__0`)
