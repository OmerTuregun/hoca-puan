# HocaPuan — Veritabanı Yedekleme (Google Drive via rclone)

Production PostgreSQL veritabanının günlük yedeğini alır, sıkıştırır ve **rclone** ile Google Drive'a yükler. 30 günden eski yedekler Drive klasöründen silinir.

## Gereksinimler

- Docker (`hocapuan_prod_db` çalışıyor olmalı)
- Python 3.10+ (ek pip paketi gerekmez)
- **rclone** kurulu ve yapılandırılmış olmalı:
  - Remote adı: `gdrive`
  - OAuth ile **kendi Google hesabınız** olarak yetkilendirilmiş
  - Hedef klasör: `HocaPuan-Backups` (Drive'da otomatik oluşur veya önceden oluşturulabilir)

### rclone ilk kurulum (bir kez)

```bash
rclone config
# n) New remote
# name: gdrive
# Storage: drive (Google Drive)
# client_id / client_secret: boş bırakılabilir (varsayılan)
# scope: drive (tam erişim) veya drive.file
# OAuth ile tarayıcıda giriş yapın
```

Yapılandırmayı doğrulayın:

```bash
rclone lsd gdrive:
rclone mkdir gdrive:HocaPuan-Backups   # isteğe bağlı, yoksa copy sırasında oluşur
```

## Ortam değişkenleri

`.env.production` içinde (veya cron ortamında):

```bash
# Başarısız yedek uyarısı (önerilir)
BACKUP_ALERT_EMAIL=you@example.com
```

Mevcut `DB_*` ve `EMAIL_*` değişkenleri `.env.production`'dan otomatik okunur.

İsteğe bağlı:

```bash
DB_CONTAINER=hocapuan_prod_db   # varsayılan
```

## Kurulum (sunucuda bir kez)

```bash
cd /root/hoca-puan/scripts/backup
python3 -m venv venv    # isteğe bağlı; script stdlib ile çalışır
chmod +x backup_to_drive.py
```

## Manuel test

```bash
cd /root/hoca-puan/scripts/backup
export BACKUP_ALERT_EMAIL="sizin@email.com"   # isteğe bağlı, hata bildirimi için
./venv/bin/python backup_to_drive.py
# veya: python3 backup_to_drive.py
```

Başarılıysa:

- `backup.log` içinde `Yedekleme işlemi tamamlandı.` görünür
- Drive'da `hocapuan_backup_YYYY-MM-DD_HH-MM.dump.gz` dosyası oluşur

Doğrulama:

```bash
rclone lsjson gdrive:HocaPuan-Backups/
```

## Cron (her gece 03:00)

```bash
crontab -e
```

Ekleyin:

```cron
0 3 * * * cd /root/hoca-puan/scripts/backup && BACKUP_ALERT_EMAIL=sizin@email.com /root/hoca-puan/scripts/backup/venv/bin/python /root/hoca-puan/scripts/backup/backup_to_drive.py >> /root/hoca-puan/scripts/backup/backup_cron.log 2>&1
```

`BACKUP_ALERT_EMAIL` değerini kendi adresinizle değiştirin.

## Log dosyaları

| Dosya | Açıklama |
|-------|----------|
| `scripts/backup/backup.log` | Script'in kendi logu |
| `scripts/backup/backup_cron.log` | Cron stdout/stderr |

## Geri yükleme (restore)

**DİKKAT:** Geri yükleme mevcut veritabanının üzerine yazar. Önce bakım penceresi planlayın ve mümkünse anlık bir ek yedek alın.

### 1. Yedeği Drive'dan indirin

```bash
rclone copy gdrive:HocaPuan-Backups/hocapuan_backup_YYYY-MM-DD_HH-MM.dump.gz /tmp/
```

### 2. API'yi durdurun (önerilir)

```bash
cd /root/hoca-puan
docker compose --env-file .env.production -f docker-compose-prod.yml stop api frontend
```

### 3. Veritabanını geri yükleyin

```bash
source /root/hoca-puan/.env.production
gunzip -c /tmp/hocapuan_backup_YYYY-MM-DD_HH-MM.dump.gz | docker exec -i \
  -e PGPASSWORD="$DB_PASSWORD" \
  hocapuan_prod_db \
  pg_restore -U "$DB_USER" -d "$DB_NAME" --clean --if-exists --no-owner --role="$DB_USER"
```

### 4. Servisleri tekrar başlatın

```bash
cd /root/hoca-puan
docker compose --env-file .env.production -f docker-compose-prod.yml up -d api frontend
```

### 5. Doğrulama

```bash
docker exec hocapuan_prod_db psql -U hocapuan_user -d hocapuan -c "SELECT count(*) FROM \"Users\";"
```

## Güvenlik notları

- `pg_dump` salt okunur bir işlemdir; production verisini değiştirmez.
- rclone OAuth token'ı `~/.config/rclone/rclone.conf` içinde saklanır; dosya izinlerini koruyun.
