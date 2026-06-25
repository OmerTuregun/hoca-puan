#!/usr/bin/env python3
"""
HocaPuan production PostgreSQL yedeği → Google Drive (rclone).

Ortam değişkenleri .env.production dosyasından veya shell ortamından okunur.
Yükleme için önceden yapılandırılmış rclone remote kullanılır (varsayılan: gdrive).
"""

from __future__ import annotations

import gzip
import json
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

SCRIPT_DIR = Path(__file__).resolve().parent
PROJECT_ROOT = SCRIPT_DIR.parents[1]
ENV_FILE = PROJECT_ROOT / ".env.production"
LOG_FILE = SCRIPT_DIR / "backup.log"
RETENTION_DAYS = 30

RCLONE_REMOTE = "gdrive"
DEFAULT_BACKUP_FOLDER = "Ömer Bireysel/Hocanı Yorumla/HocaPuan-Backups"


def get_backup_folder(cfg: dict[str, str]) -> str:
    path = (
        os.environ.get("RCLONE_BACKUP_PATH")
        or cfg.get("RCLONE_BACKUP_PATH")
        or DEFAULT_BACKUP_FOLDER
    ).strip().strip("/")
    if not path:
        raise ValueError("RCLONE_BACKUP_PATH boş olamaz.")
    return path


def remote_dir(backup_folder: str) -> str:
    return f"{RCLONE_REMOTE}:{backup_folder}/"


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


def check_rclone(logger: logging.Logger) -> None:
    result = subprocess.run(
        ["rclone", "version"],
        capture_output=True,
        text=True,
        check=False,
    )
    if result.returncode != 0:
        detail = (result.stderr or result.stdout or "").strip()
        raise RuntimeError(
            "rclone kurulu değil veya çalıştırılamıyor. "
            f"'rclone version' başarısız oldu: {detail or 'bilinmeyen hata'}"
        )
    first_line = result.stdout.splitlines()[0] if result.stdout else "rclone"
    logger.info("rclone hazır: %s", first_line)


def run_rclone(args: list[str], logger: logging.Logger, action: str) -> subprocess.CompletedProcess[str]:
    cmd = ["rclone", *args]
    logger.info("rclone %s", " ".join(args))
    result = subprocess.run(cmd, capture_output=True, text=True, check=False)
    if result.returncode != 0:
        detail = (result.stderr or result.stdout or "").strip()
        raise RuntimeError(
            f"rclone {action} başarısız (kod {result.returncode}): {detail or 'çıktı yok'}"
        )
    if result.stdout.strip():
        for line in result.stdout.strip().splitlines():
            logger.info("rclone: %s", line)
    return result


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


def upload_via_rclone(local_path: Path, backup_folder: str, logger: logging.Logger) -> None:
    target = remote_dir(backup_folder)
    logger.info("rclone ile yükleme başlıyor: %s → %s", local_path.name, target)
    run_rclone(["copy", str(local_path), target, "-v"], logger, "copy")
    logger.info("rclone yükleme başarılı: %s", local_path.name)


def parse_rclone_modtime(mod_time: str) -> datetime:
    if mod_time.endswith("Z"):
        mod_time = mod_time[:-1] + "+00:00"
    return datetime.fromisoformat(mod_time).astimezone(timezone.utc)


def prune_old_backups_rclone(backup_folder: str, logger: logging.Logger) -> None:
    cutoff = datetime.now(timezone.utc) - timedelta(days=RETENTION_DAYS)
    remote_list_path = remote_dir(backup_folder)
    logger.info(
        "Eski yedekler temizleniyor (>%s gün, ModTime < %s)",
        RETENTION_DAYS,
        cutoff.isoformat(),
    )

    result = run_rclone(["lsjson", remote_list_path], logger, "lsjson")
    try:
        entries = json.loads(result.stdout or "[]")
    except json.JSONDecodeError as exc:
        raise RuntimeError(f"rclone lsjson çıktısı parse edilemedi: {exc}") from exc

    deleted = 0
    for item in entries:
        if item.get("IsDir"):
            continue
        name = item.get("Name")
        mod_time = item.get("ModTime")
        if not name or not mod_time:
            continue
        modified_at = parse_rclone_modtime(mod_time)
        if modified_at >= cutoff:
            continue
        remote_file = f"{RCLONE_REMOTE}:{backup_folder}/{name}"
        run_rclone(["deletefile", remote_file], logger, f"deletefile {name}")
        deleted += 1
        logger.info("Silindi: %s (ModTime=%s)", name, mod_time)

    logger.info("Eski yedek temizliği tamamlandı (%s dosya silindi)", deleted)


def main() -> int:
    logger = setup_logging()
    file_values = load_env_file(ENV_FILE)
    cfg = {**file_values, **{k: v for k, v in os.environ.items() if v}}

    required = ["DB_NAME", "DB_USER", "DB_PASSWORD"]
    missing = [k for k in required if not cfg.get(k)]
    if missing:
        msg = f"Eksik ortam değişkenleri: {', '.join(missing)}"
        logger.error(msg)
        send_failure_email(cfg, "HocaPuan Backup başarısız", msg, logger)
        return 1

    timestamp = datetime.now().strftime("%Y-%m-%d_%H-%M")
    archive_name = f"hocapuan_backup_{timestamp}.dump.gz"

    tmpdir = Path(tempfile.mkdtemp(prefix="hocapuan_backup_"))
    dump_path = tmpdir / f"hocapuan_backup_{timestamp}.dump"
    gz_path = tmpdir / archive_name

    try:
        backup_folder = get_backup_folder(cfg)
        logger.info("Yedek klasörü: %s", backup_folder)

        check_rclone(logger)
        run_pg_dump(cfg, dump_path, logger)
        compress_dump(dump_path, gz_path, logger)
        upload_via_rclone(gz_path, backup_folder, logger)
        prune_old_backups_rclone(backup_folder, logger)

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
