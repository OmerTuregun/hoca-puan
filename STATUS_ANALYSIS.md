## Genel Durum (2026-03-30)

Bu dosya, şu anki **Docker / DB doluluğu** ve **frontend↔backend bağlantılarını** hızlıca özetler.

---

## Docker: “Komple ne kadar yer kaplıyor?”

### Çalışan HocaPuan konteynerleri
- **`hocapuan_api`** (ASP.NET Core API) — host port: **5001 → 8080**
- **`hocapuan_frontend`** (Vite dev) — host port: **5173 → 5173**
- **`hocapuan_db`** (Postgres 16) — host port: **5433 → 5432**
- **`hocapuan_pgadmin`** (pgAdmin) — host port: **5051 → 80**

### Image boyutları (diskte kapladığı alan)
> `docker images` çıktısına göre:
- **`hocapuan-api:latest`**: **2.1 GB**
- **`hocapuan-frontend:latest`**: **558 MB**
- **`postgres:16-alpine`**: **395 MB**
- **`dpage/pgadmin4:latest`**: **823 MB**

**Notlar**
- Bu boyutlar “image” katmanlarıdır; HocaPuan stack’in toplam disk etkisinin büyük kısmı burada + DB volume’dadır.
- Container “writable layer” (konteynerin kendi küçük dosya değişiklikleri) pratikte çok küçük (KB seviyeleri).

### DB volume boyutu (kalıcı veri)
Compose volume adı: **`hocapuan_postgres_data`**
- Docker “Local Volumes” raporunda görünen boyut: **~106.6 MB**

### Docker build cache (genel sistem etkisi)
`docker system df -v` çıktısında **build cache ~49.72 GB** görünüyor.
- Bu **sadece HocaPuan’a özel değil**, makinedeki tüm Docker build’lerinden birikiyor.
- Disk temizliği gerekirse (dikkatli): `docker builder prune` / `docker system prune` değerlendirilebilir.

---

## DB Doluluğu (Postgres içi)

### Toplam DB boyutu
- `pg_database_size(current_database())`: **47 MB**

### Tablo satır sayıları (pg_stat_user_tables tahmini)
> Bu değerler **VACUUM/ANALYZE** görmediyse “yaklaşık” olabilir.

- **`Professors`**: **180,548**
- **`Faculties`**: **1,934**
- Diğer tablolar: bu istatistikte **0** görünüyor (muhtemelen istatistik güncel değil / çok az veri).

### Tablo boyutları (en çok yer kaplayanlar)
- **`Professors`**: **38 MB**
- **`Departments`**: **~1.1 MB**
- **`Faculties`**: **~312 KB**
- **`Universities`**: **~128 KB**
- Diğerleri: çok küçük

**Yorum**
- Import sonrası asıl şişen tablo doğal olarak **`Professors`**.
- DB boyutu (47MB) ile volume (106MB) arasında fark normal: volume içinde WAL/metadata vb. de olur.

---

## Frontend: Şu an neler var, backend’le neler bağlı?

### Routing / sayfalar
`hocapuan-frontend/src/App.tsx`:
- **`/`**: `HomePage`
- **`/search`**: `SearchPage`
- **`/professors/:id`**: `ProfessorPage`
- **`/universities/:id`**: `UniversityPage`
- **`/professors/:id/review`**: `AddReviewPage`
- **`/login`**: `LoginPage`
- **`/register`**: `RegisterPage`
- **`*`**: `NotFoundPage`

### API katmanı (Axios)
`hocapuan-frontend/src/services/api.ts`:
- Axios **baseURL = `/api`**
- Request interceptor: `localStorage.token` varsa `Authorization: Bearer <token>`
- Response interceptor: **401** gelirse token silip `/login`’e yönlendiriyor

### Vite proxy (frontend → backend)
`hocapuan-frontend/vite.config.ts`:
- `/api` istekleri proxy ile **`process.env.VITE_API_URL`**’a gidiyor.
- Docker Compose’ta frontend env: **`VITE_API_URL=http://api:8080`** (container network üzerinden API’ye gider)
- Local çalışmada fallback: **`http://localhost:5001`**

### Frontend’in kullandığı backend endpoint’leri
`api.ts` üzerinden kullanılanlar:

#### Auth
- `POST /api/auth/register`
- `POST /api/auth/login`

#### Universities
- `GET /api/universities` (opsiyonel `search`)
- `GET /api/universities/{id}`
- `GET /api/universities/{id}/faculties`
- `GET /api/universities/faculties/{facultyId}/departments`

#### Professors
- `GET /api/professors` (search/list; paging/sorting)
- `GET /api/professors/{id}`

#### Reviews
- `GET /api/reviews/professor/{professorId}?page=&pageSize=`
- `POST /api/reviews` (Authorize)
- `POST /api/reviews/{id}/vote?isUpvote=...` (Authorize)
- `DELETE /api/reviews/{id}` (Authorize)

### UI akışı (mevcut durum)
- **HomePage**: Üniversite listesini çekiyor (`GET /universities`) + arama yönlendiriyor
- **SearchPage**: Hoca arama/list (`GET /professors`) + üniversite filtresi için üniversite listesi (`GET /universities`)
- **ProfessorPage**: Hoca detay (`GET /professors/{id}`) + yorumlar (`GET /reviews/professor/{id}`)
- **AddReviewPage**: Yorum oluşturma (`POST /reviews`) — giriş gerektiriyor
- **UniversityPage**: Üniversite detay (`GET /universities/{id}`) + o üniversiteden örnek hocalar (`GET /professors?universityId=...`)

---

## Backend: Frontend’e ek olarak neler var?

Controller’lardan görünen ek API’ler:
- **Import**: `/api/import/...` altında YÖK import/preview/browser-import + job status
  - Frontend tarafında **şu an bir import/admin ekranı bağlı değil** (sadece API var).

---

## Kısa “Ne durumdayız?” özeti

- **DB dolu ve import çalışmış**: `Professors` ~**180k** satır, DB **~47MB**.
- **Docker footprint**: HocaPuan’a ait image’lar toplamda kabaca **~3.9GB** (image boyutlarının toplamı) + DB volume **~106MB**.
- **Frontend**: Arama / hoca detay / üniversite detay / yorum yazma / auth akışı var ve backend endpoint’lerine bağlı.
- **Eksik/sonraki adım adayları**:
  - Admin/import ekranı (ImportController endpoint’lerini UI’dan tetikleme + job progress görüntüleme)
  - Review moderasyon ekranı (pending/moderate endpoint’leri)
  - Prod dağıtım senaryosu (Vite dev proxy yerine reverse proxy / aynı origin)

