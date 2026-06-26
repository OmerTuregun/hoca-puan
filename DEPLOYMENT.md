# Hocanı Yorumla — Deployment

Docker Compose yapılandırması geliştirme ve production için ayrılmıştır.

> **Not:** Kök dizindeki `docker-compose.yml` kaldırıldı. Geliştirme artık yalnızca
> `docker-compose.dev.yml` ile çalıştırılır:
> `docker compose -f docker-compose.dev.yml up --build -d`

| Dosya | Ortam | Env dosyası | Portlar (host) |
|-------|-------|-------------|----------------|
| `docker-compose.dev.yml` | Local geliştirme | `.env.development` | API 5001, Frontend 5173, DB 5433 |
| `docker-compose.prod.yml` | Sunucu (IP + HTTP) | `.env.production` | API 5011, Frontend 8089, DB 5435 |

Dev ve prod **aynı sunucuda aynı anda** çalışabilir: portlar ve container adları çakışmaz.

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
docker compose --env-file .env.development -f docker-compose.dev.yml up --build -d
```

- Frontend: http://localhost:5173 (Vite dev server, yalnızca localhost)
- API: http://localhost:5001
- pgAdmin: http://localhost:5051
- PostgreSQL: localhost:5433

`dotnet run` (Docker dışı) için proje kökünde `.env.development` veya `.env` bulunmalıdır. `ASPNETCORE_ENVIRONMENT=Development` iken `EnvLoader` önce `.env.development` dosyasını arar.

## Production

```bash
docker compose --env-file .env.production -f docker-compose.prod.yml up --build -d
```

- Frontend: `http://SUNUCU_IP:8089` (şimdilik Vite dev build — nginx/static TODO)
- API: `http://SUNUCU_IP:5011`
- PostgreSQL: `SUNUCU_IP:5435`
- pgAdmin **yok** (yalnızca dev compose'da)
- Servisler `restart: unless-stopped` ile çalışır

### `.env.production` içinde mutlaka güncellenmesi gerekenler

| Değişken | Açıklama |
|----------|----------|
| `DB_PASSWORD` | Güçlü PostgreSQL şifresi (`CHANGE_ME_STRONG_PASSWORD` yerine) |
| `JWT_SECRET` | Uzun rastgele secret (`CHANGE_ME_GENERATE_STRONG_SECRET` yerine; örn. `openssl rand -base64 64`) |
| `ALLOWED_ORIGIN` | `http://GERÇEK_IP:8089` — `SUNUCU_IP` yerine gerçek IP |
| `APP_FRONTEND_URL` | E-posta linkleri için aynı adres (`http://GERÇEK_IP:8089`) |
| `EMAIL_*` | Production SMTP bilgileri |

Portlar varsayılan olarak prod için **5011 / 8089 / 5435** ayarlıdır; `.env.production` içinde değiştirmenize gerek yoktur.

### HTTPS / domain eklendiğinde

Kod veya compose dosyası değiştirmeden `.env.production` içinde:

```env
USE_HTTPS=true
ALLOWED_ORIGIN=https://alanadiniz.com
APP_FRONTEND_URL=https://alanadiniz.com
```

`USE_HTTPS=true` tek başına cookie `Secure=true` ve `SameSite=Strict` uygular. İsteğe bağlı ince ayar için `COOKIE_SECURE` ve `AUTH_COOKIE_SAMESITE` kullanılabilir.

## Ortam değişkenleri özeti

Tüm değişkenler için referans: `.env.example`

Cookie / HTTPS:

- `USE_HTTPS=false` (HTTP) → `Secure=false`, `SameSite=Lax`
- `USE_HTTPS=true` (HTTPS) → `Secure=true`, `SameSite=Strict`
- `COOKIE_SECURE` — `USE_HTTPS` üzerinde önceliklidir
- `AUTH_COOKIE_SAMESITE` — `USE_HTTPS` türetmesini override eder

CORS:

- Development: `appsettings.Development.json` içindeki localhost origin listesi (değişmez)
- Production: `ALLOWED_ORIGIN` (veya `Cors__AllowedOrigins__0`)

