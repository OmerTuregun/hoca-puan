#!/usr/bin/env python3
"""
HocaPuan production PostgreSQL yedeği → Google Drive.

Ortam değişkenleri .env.production dosyasından veya shell ortamından okunur.
"""

from __future__ import annotations

import gzip
import logging
import os
import shutil
import smtplib
import subprocess
import sys
import tempfile
from datetime import datetime, timedelta, timezone
from email.mime.text import MIMEText
from pathlib import Path

from google.oauth2 import service_account
from googleapiclient.discovery import build
from googleapiclient.http import MediaFileUpload

SCRIPT_DIR = Path(__file__).resolve().parent
PROJECT_ROOT = SCRIPT_DIR.parents[1]
ENV_FILE = PROJECT_ROOT / ".env.production"
LOG_FILE = SCRIPT_DIR / "backup.log"
RETENTION_DAYS = 30
DRIVE_SCOPES = ["https://www.googleapis.com/auth/drive"]


def setup_logging() -> logging.Logger:
    logger = logging.getLogger("hocapuan_backup")
    logger.setLevel(logging.INFO)
    if not logger.handlers:
        formatter = logging.Formatter("%(asctime)s %(levelname)s %(message)s")
        file_handler = logging.FileHandler(LOG_FILE, encoding="utf-8")
        file_handler.setFormatter(formatter)
        stream_handler = logging.StreamHandler(sys.stdout)
        stream_handler.setFormatter(formatter)
        logger.addHandler(file_handler)
        logger.addHandler(stream_handler)
    return logger


def load_env_file(path: Path) -> dict[str, str]:
    values: dict[str, str] = {}
    if not path.is_file():
        return values
    for line in path.read_text(encoding="utf-8").splitlines():
        line = line.strip()
        if not line or line.startswith("#") or "=" not in line:
            continue
        key, _, raw = line.partition("=")
        values[key.strip()] = raw.strip()
    return values


def env(key: str, file_values: dict[str, str], default: str | None = None) -> str | None:
    return os.environ.get(key) or file_values.get(key) or default


def send_failure_email(cfg: dict[str, str], subject: str, body: str, logger: logging.Logger) -> None:
    host = cfg.get("EMAIL_HOST")
    port = int(cfg.get("EMAIL_PORT", "587"))
    username = cfg.get("EMAIL_USERNAME")
    password = cfg.get("EMAIL_PASSWORD")
    from_addr = cfg.get("EMAIL_FROM")
    to_addr = cfg.get("BACKUP_ALERT_EMAIL") or os.environ.get("BACKUP_ALERT_EMAIL")

    if not all([host, username, password, from_addr, to_addr]):
        logger.error(
            "Uyarı e-postası gönderilemedi: SMTP veya BACKUP_ALERT_EMAIL eksik."
        )
        return

    msg = MIMEText(body, "plain", "utf-8")
    msg["Subject"] = subject
    msg["From"] = from_addr
    msg["To"] = to_addr

    try:
        with smtplib.SMTP(host, port, timeout=60) as smtp:
            smtp.starttls()
            smtp.login(username, password)
            smtp.sendmail(from_addr, [to_addr], msg.as_string())
        logger.info("Uyarı e-postası gönderildi: %s", to_addr)
    except Exception as exc:
        logger.error("Uyarı e-postası gönderilemedi: %s", exc)


def run_pg_dump(cfg: dict[str, str], dump_path: Path, logger: logging.Logger) -> None:
    container = cfg.get("DB_CONTAINER", "hocapuan_prod_db")
    db_user = cfg["DB_USER"]
    db_name = cfg["DB_NAME"]
    db_password = cfg["DB_PASSWORD"]

    logger.info(
        "pg_dump başlıyor (container=%s, db=%s, user=%s)",
        container,
        db_name,
        db_user,
    )

    cmd = [
        "docker",
        "exec",
        "-e",
        f"PGPASSWORD={db_password}",
        container,
        "pg_dump",
        "-U",
        db_user,
        "-d",
        db_name,
        "-F",
        "c",
    ]

    with dump_path.open("wb") as outfile:
        result = subprocess.run(cmd, stdout=outfile, stderr=subprocess.PIPE, check=False)

    if result.returncode != 0:
        stderr = result.stderr.decode("utf-8", errors="replace").strip()
        raise RuntimeError(f"pg_dump başarısız (kod {result.returncode}): {stderr}")

    if dump_path.stat().st_size == 0:
        raise RuntimeError("pg_dump çıktı dosyası boş.")

    logger.info("pg_dump başarılı (%s bayt)", dump_path.stat().st_size)


def compress_dump(dump_path: Path, gz_path: Path, logger: logging.Logger) -> None:
    logger.info("Sıkıştırma başlıyor: %s", gz_path.name)
    with dump_path.open("rb") as src, gzip.open(gz_path, "wb") as dst:
        shutil.copyfileobj(src, dst)

    if not gz_path.is_file() or gz_path.stat().st_size == 0:
        raise RuntimeError("gzip sıkıştırma başarısız veya çıktı boş.")

    logger.info("Sıkıştırma başarılı (%s bayt)", gz_path.stat().st_size)


def build_drive_service(key_path: Path):
    credentials = service_account.Credentials.from_service_account_file(
        str(key_path),
        scopes=DRIVE_SCOPES,
    )
    return build("drive", "v3", credentials=credentials, cache_discovery=False)


def upload_to_drive(
    service,
    folder_id: str,
    local_path: Path,
    logger: logging.Logger,
) -> str:
    logger.info("Google Drive yüklemesi başlıyor: %s", local_path.name)
    metadata = {"name": local_path.name, "parents": [folder_id]}
    media = MediaFileUpload(str(local_path), mimetype="application/gzip", resumable=True)
    created = (
        service.files()
        .create(body=metadata, media_body=media, fields="id,name,createdTime")
        .execute()
    )
    file_id = created["id"]
    logger.info(
        "Google Drive yüklemesi başarılı (id=%s, name=%s)",
        file_id,
        created.get("name"),
    )
    return file_id


def prune_old_backups(service, folder_id: str, logger: logging.Logger) -> None:
    cutoff = datetime.now(timezone.utc) - timedelta(days=RETENTION_DAYS)
    logger.info(
        "Eski yedekler temizleniyor (>%s gün, createdTime < %s)",
        RETENTION_DAYS,
        cutoff.isoformat(),
    )

    page_token = None
    deleted = 0
    while True:
        response = (
            service.files()
            .list(
                q=f"'{folder_id}' in parents and trashed=false",
                fields="nextPageToken, files(id, name, createdTime)",
                pageSize=200,
                pageToken=page_token,
            )
            .execute()
        )
        for item in response.get("files", []):
            created_raw = item.get("createdTime")
            if not created_raw:
                continue
            created_at = datetime.fromisoformat(created_raw.replace("Z", "+00:00"))
            if created_at < cutoff:
                service.files().delete(fileId=item["id"]).execute()
                deleted += 1
                logger.info("Silindi: %s (createdTime=%s)", item.get("name"), created_raw)

        page_token = response.get("nextPageToken")
        if not page_token:
            break

    logger.info("Eski yedek temizliği tamamlandı (%s dosya silindi)", deleted)


def main() -> int:
    logger = setup_logging()
    file_values = load_env_file(ENV_FILE)
    cfg = {**file_values, **{k: v for k, v in os.environ.items() if v}}

    required = [
        "DB_NAME",
        "DB_USER",
        "DB_PASSWORD",
        "GOOGLE_SERVICE_ACCOUNT_KEY_PATH",
        "GOOGLE_DRIVE_BACKUP_FOLDER_ID",
    ]
    missing = [k for k in required if not cfg.get(k)]
    if missing:
        msg = f"Eksik ortam değişkenleri: {', '.join(missing)}"
        logger.error(msg)
        send_failure_email(cfg, "HocaPuan Backup başarısız", msg, logger)
        return 1

    key_path = Path(cfg["GOOGLE_SERVICE_ACCOUNT_KEY_PATH"])
    folder_id = cfg["GOOGLE_DRIVE_BACKUP_FOLDER_ID"]
    timestamp = datetime.now().strftime("%Y-%m-%d_%H-%M")
    archive_name = f"hocapuan_backup_{timestamp}.dump.gz"

    tmpdir = Path(tempfile.mkdtemp(prefix="hocapuan_backup_"))
    dump_path = tmpdir / f"hocapuan_backup_{timestamp}.dump"
    gz_path = tmpdir / archive_name

    try:
        run_pg_dump(cfg, dump_path, logger)
        compress_dump(dump_path, gz_path, logger)

        if not key_path.is_file():
            raise FileNotFoundError(f"Servis hesabı anahtarı bulunamadı: {key_path}")

        service = build_drive_service(key_path)
        upload_to_drive(service, folder_id, gz_path, logger)
        prune_old_backups(service, folder_id, logger)

        logger.info("Yedekleme işlemi tamamlandı.")
        return 0
    except Exception as exc:
        msg = f"Backup başarısız oldu: {exc}"
        logger.exception(msg)
        send_failure_email(cfg, "HocaPuan Backup başarısız", msg, logger)
        return 1
    finally:
        shutil.rmtree(tmpdir, ignore_errors=True)


if __name__ == "__main__":
    sys.exit(main())
