#!/usr/bin/env bash
# Sorunlu fakülte/bölüm/hoca kayıtlarını DB'de temizler (admin API alternatifi).
set -euo pipefail
ROOT="$(cd "$(dirname "$0")/.." && pwd)"
DB_USER=$(grep '^DB_USER=' "$ROOT/.env.production" | cut -d= -f2)
DB_NAME=$(grep '^DB_NAME=' "$ROOT/.env.production" | cut -d= -f2)
DB_PASS=$(grep '^DB_PASSWORD=' "$ROOT/.env.production" | cut -d= -f2)
CONTAINER="${DB_CONTAINER:-hocapuan_prod_db}"

echo "=== Hoca ünvan temizliği (Unvan: → kısa ünvan) ==="
docker exec -e PGPASSWORD="$DB_PASS" "$CONTAINER" psql -U "$DB_USER" -d "$DB_NAME" -v ON_ERROR_STOP=1 <<'SQL'
UPDATE "Professors" SET "Title" = 'Doç. Dr.', "UpdatedAt" = NOW()
WHERE NOT "IsDeleted" AND "Title" ~* '\(Unvan:\s*Doçent\)' OR "Title" ~* '\(Unvan:\s*Docent\)';

UPDATE "Professors" SET "Title" = 'Prof. Dr.', "UpdatedAt" = NOW()
WHERE NOT "IsDeleted" AND ("Title" ~* '\(Unvan:\s*Profesör\)' OR "Title" ~* '\(Unvan:\s*Profesor\)');

UPDATE "Professors" SET "Title" = 'Dr. Öğr. Üyesi', "UpdatedAt" = NOW()
WHERE NOT "IsDeleted" AND "Title" ~* '\(Unvan:.*[Öö]ğretim\s+[Üü]yesi\)';

UPDATE "Professors" SET "Title" = 'Öğr. Gör.', "UpdatedAt" = NOW()
WHERE NOT "IsDeleted" AND "Title" ~* '\(Unvan:\s*Öğretim\s+Görevlisi\)';

UPDATE "Professors" SET "Title" = 'Arş. Gör.', "UpdatedAt" = NOW()
WHERE NOT "IsDeleted" AND "Title" ~* '\(Unvan:\s*Araştırma\s+Görevlisi\)';

SELECT 'remaining_unvan_titles' AS k, count(*)::int FROM "Professors"
WHERE NOT "IsDeleted" AND "Title" ~* '\(unvan:';
SQL

echo ""
echo "Fakülte temizliği için API çağrısı gerekir (hocayı taşır):"
echo "  POST /api/import/cleanup-faculty-names  (admin cookie ile)"
