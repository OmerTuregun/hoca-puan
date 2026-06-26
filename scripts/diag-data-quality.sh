#!/usr/bin/env bash
# Tüm üniversitelerde sorunlu fakülte, bölüm ve hoca kayıtlarını tarar.
# Kullanım: ./scripts/diag-data-quality.sh
# Çıktı: scripts/output/data-quality-report.json

set -euo pipefail
ROOT="$(cd "$(dirname "$0")/.." && pwd)"
OUT="$ROOT/scripts/output"
mkdir -p "$OUT"

DB_USER=$(grep '^DB_USER=' "$ROOT/.env.production" | cut -d= -f2)
DB_NAME=$(grep '^DB_NAME=' "$ROOT/.env.production" | cut -d= -f2)
DB_PASS=$(grep '^DB_PASSWORD=' "$ROOT/.env.production" | cut -d= -f2)
CONTAINER="${DB_CONTAINER:-hocapuan_prod_db}"

export PGPASSWORD="$DB_PASS"

echo "=== Veri kalitesi taraması ($(date -Iseconds)) ==="

docker exec -i -e PGPASSWORD="$DB_PASS" "$CONTAINER" psql -U "$DB_USER" -d "$DB_NAME" -v ON_ERROR_STOP=1 <<'SQL'
\pset format unaligned
\pset tuples_only on

SELECT 'bad_faculties', count(*)::text
FROM "Faculties" f WHERE NOT f."IsDeleted"
AND (
  f."Name" ILIKE 'bilinmiyor' = false AND (
    f."Name" ~* '(orcid|akademik görev|öğrenim bilgisi|birlikte çalıştığı|araştırma görevlisi|öğretim görevlisi)'
    OR f."Name" ~ '^\d'
    OR f."Name" ~ '\d{4}'
    OR f."Name" ~* 'üniversitesi.*üniversitesi'
    OR length(f."Name") > 80
    OR f."Name" !~* '(fakültesi|fakultesi|enstitü|enstitu|yüksekokul|yukseokul|\bmyo\b|konservatuvar|rektörlük|rektorluk|araştırma merkezi|uygulama ve araştırma)'
  )
);

SELECT 'bad_departments', count(*)::text
FROM "Departments" d WHERE NOT d."IsDeleted"
AND (
  d."Name" ILIKE 'bilinmiyor' = false AND (
    d."Name" ~* '(orcid|akademik|öğrenim bilgisi|araştırma görevlisi)'
    OR length(d."Name") > 80
    OR (
      d."Name" !~* '(bölümü|bolumu|anabilim|programı|programi|\mpr\.)'
      AND d."Name" !~* '(mühendisliği|muhendisligi|hemşireliği|hemsireligi|mimarlığı|mimarligi)\s*$'
      AND d."Name" !~* '^(hukuk|tıp|tip|eczacılık|eczacilik|adalet|mimarlık|mimarlik|hemşirelik|hemsirelik|diş hekimliği|dis hekimligi|islami ilimler)(\s+pr\.)?$'
    )
  )
);

SELECT 'bad_professor_titles', count(*)::text
FROM "Professors" p WHERE NOT p."IsDeleted"
AND (
  p."Title" ~* '\(unvan:'
  OR length(p."Title") > 35
);

SELECT 'bad_professor_names', count(*)::text
FROM "Professors" p WHERE NOT p."IsDeleted"
AND (
  length(p."FirstName") > 40 OR length(p."LastName") > 50
  OR p."FirstName" ~* '(ünvan|öğretim üyesi|öğretim görevlisi)'
);

\echo '--- top_universities_by_issues ---'
SELECT u."Name",
  (SELECT count(*) FROM "Faculties" f WHERE f."UniversityId"=u."Id" AND NOT f."IsDeleted"
    AND f."Name" !~* '(fakültesi|yüksekokul|enstitü|myo|konservatuvar|rektörlük|araştırma merkezi)' 
    AND f."Name" NOT ILIKE 'bilinmiyor') AS bad_fac,
  (SELECT count(*) FROM "Professors" p WHERE p."UniversityId"=u."Id" AND NOT p."IsDeleted" AND p."Title" ~* '\(unvan:') AS bad_title
FROM "Universities" u WHERE NOT u."IsDeleted"
ORDER BY 2 DESC, 3 DESC
LIMIT 15;
SQL

echo ""
echo "API raporu için (admin girişi gerekir): GET /api/import/data-quality-report"
echo "Temizlik: POST /api/import/cleanup-faculty-names, POST /api/import/cleanup-department-names ve POST /api/import/cleanup-professor-names"
