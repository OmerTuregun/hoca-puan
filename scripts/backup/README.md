# HocaPuan — Veritabanı Yedekleme (Google Drive via rclone)

Production PostgreSQL veritabanının günlük yedeğini alır, sıkıştırır ve **rclone** ile Google Drive'a yükler. 30 günden eski yedekler Drive klasöründen silinir.

## Gereksinimler

- Docker (`hocapuan_prod_db` çalışıyor olmalı)
- Python 3.10+ (ek pip paketi gerekmez)
- **rclone** kurulu ve yapılandırılmış olmalı:
  - Remote adı: `gdrive`
  - OAuth ile **kendi Google hesabınız** olarak yetkilendirilmiş
  - Hedef klasör (varsayılan): `Ömer Bireysel/Hocanı Yorumla/HocaPuan-Backups`

### rclone ilk kurulum (bir kez)

```bash
rclone config
# n) New remote → name: gdrive → Storage: drive → OAuth ile giriş
```

Drive'da klasör yapısını oluşturun (veya Drive arayüzünden taşıyın):

```bash
rclone mkdir "gdrive:Ömer Bireysel/Hocanı Yorumla/HocaPuan-Backups"
```

## Ortam değişkenleri

`.env.production` içinde:

```bash
# rclone hedef yolu (Drive kökünden, / ile ayrılmış)
RCLONE_BACKUP_PATH=Ömer Bireysel/Hocanı Yorumla/HocaPuan-Backups

# Başarısız yedek uyarısı (önerilir)
BACKUP_ALERT_EMAIL=you@example.com
```

Klasörü Drive'da taşırsanız yalnızca `RCLONE_BACKUP_PATH` değerini güncellemeniz yeterli.

Mevcut `DB_*` ve `EMAIL_*` değişkenleri `.env.production`'dan otomatik okunur.

İsteğe bağlı:

```bash
DB_CONTAINER=hocapuan_prod_db   # varsayılan
```

## Kurulum (sunucuda bir kez)

```bash
cd /root/hoca-puan/scripts/backup
chmod +x backup_to_drive.py
```

## Manuel test

```bash
cd /root/hoca-puan/scripts/backup
python3 backup_to_drive.py
```

Doğrulama:

```bash
rclone ls "gdrive:Ömer Bireysel/Hocanı Yorumla/HocaPuan-Backups/"
```

Drive web arayüzü (aynı klasör):

https://drive.google.com/drive/folders/1pIPsv8CakIU5QBRhJQxxMGrTLNUcCHAk

## Cron (her gece 03:00)

```cron
0 3 * * * cd /root/hoca-puan/scripts/backup && BACKUP_ALERT_EMAIL=sizin@email.com python3 /root/hoca-puan/scripts/backup/backup_to_drive.py >> /root/hoca-puan/scripts/backup/backup_cron.log 2>&1
```

## Geri yükleme (restore)

### 1. Yedeği Drive'dan indirin

```bash
rclone copy "gdrive:Ömer Bireysel/Hocanı Yorumla/HocaPuan-Backups/hocapuan_backup_YYYY-MM-DD_HH-MM.dump.gz" /tmp/
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
docker compose --env-file .env.production -f docker-compose-prod.yml up -d api frontend
```

## Güvenlik notları

- `pg_dump` salt okunur bir işlemdir; production verisini değiştirmez.
- rclone OAuth token'ı `~/.config/rclone/rclone.conf` içinde saklanır.
