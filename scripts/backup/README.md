# HocaPuan — Veritabanı Yedekleme (Google Drive)

Production PostgreSQL veritabanının günlük yedeğini alır, sıkıştırır ve Google Drive'a yükler. 30 günden eski yedekler Drive klasöründen silinir.

## Gereksinimler

- Docker (`hocapuan_prod_db` çalışıyor olmalı)
- Python 3.10+
- Google Cloud servis hesabı JSON anahtarı (`chmod 600`, örn. `/root/secrets/hocapuan-backup-key.json`)
- Drive klasörünün servis hesabı e-postasıyla paylaşılmış olması (Düzenleyici veya En azından içerik yöneticisi)

## Ortam değişkenleri

`.env.production` içinde (veya cron ortamında):

```bash
GOOGLE_SERVICE_ACCOUNT_KEY_PATH=/root/secrets/hocapuan-backup-key.json
GOOGLE_DRIVE_BACKUP_FOLDER_ID=your-folder-id

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
python3 -m venv venv
./venv/bin/pip install --upgrade pip
./venv/bin/pip install -r requirements.txt
chmod 600 /root/secrets/hocapuan-backup-key.json
chmod +x backup_to_drive.py
```

## Manuel test

```bash
cd /root/hoca-puan/scripts/backup
export BACKUP_ALERT_EMAIL="sizin@email.com"   # isteğe bağlı, hata bildirimi için
./venv/bin/python backup_to_drive.py
```

Başarılıysa:

- `backup.log` içinde `Yedekleme işlemi tamamlandı.` görünür
- Google Drive klasöründe `hocapuan_backup_YYYY-MM-DD_HH-MM.dump.gz` dosyası oluşur

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

`hocapuan_backup_YYYY-MM-DD_HH-MM.dump.gz` dosyasını sunucuya kopyalayın, örn. `/tmp/restore.dump.gz`

### 2. Sıkıştırmayı açın

```bash
gunzip -k /tmp/restore.dump.gz
# /tmp/restore.dump oluşur
```

### 3. API'yi durdurun (önerilir)

```bash
cd /root/hoca-puan
docker compose --env-file .env.production -f docker-compose-prod.yml stop api frontend
```

### 4. Veritabanını geri yükleyin

**Tüm veritabanını değiştir** (`--clean` mevcut nesneleri siler, dikkatli kullanın):

```bash
source /root/hoca-puan/.env.production
gunzip -c /tmp/restore.dump.gz | docker exec -i \
  -e PGPASSWORD="$DB_PASSWORD" \
  hocapuan_prod_db \
  pg_restore -U "$DB_USER" -d "$DB_NAME" --clean --if-exists --no-owner --role="$DB_USER"
```

Alternatif: önce boş bir veritabanına restore (daha güvenli test):

```bash
docker exec -e PGPASSWORD="$DB_PASSWORD" hocapuan_prod_db \
  psql -U "$DB_USER" -d postgres -c "DROP DATABASE IF EXISTS hocapuan_restore_test;"
docker exec -e PGPASSWORD="$DB_PASSWORD" hocapuan_prod_db \
  psql -U "$DB_USER" -d postgres -c "CREATE DATABASE hocapuan_restore_test OWNER hocapuan_user;"
gunzip -c /tmp/restore.dump.gz | docker exec -i \
  -e PGPASSWORD="$DB_PASSWORD" \
  hocapuan_prod_db \
  pg_restore -U "$DB_USER" -d hocapuan_restore_test --no-owner --role="$DB_USER"
```

### 5. Servisleri tekrar başlatın

```bash
cd /root/hoca-puan
docker compose --env-file .env.production -f docker-compose-prod.yml up -d api frontend
```

### 6. Doğrulama

```bash
docker exec hocapuan_prod_db psql -U hocapuan_user -d hocapuan -c "SELECT count(*) FROM \"Users\";"
```

## Güvenlik notları

- Servis hesabı JSON içeriğini loglara veya repoya koymayın.
- Anahtar dosyası `chmod 600` olmalı.
- `pg_dump` salt okunur bir işlemdir; production verisini değiştirmez.
