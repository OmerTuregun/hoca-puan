-- Diagnose Faculties/Departments data quality
\pset pager off
\timing on

-- Basic counts
SELECT 'Faculties' AS table, count(*)::bigint AS rows FROM "Faculties";
SELECT 'Departments' AS table, count(*)::bigint AS rows FROM "Departments";

-- Suspicious names (UI words / navigation leftovers) in Faculties
SELECT
  'Suspicious Faculties' AS check,
  count(*)::bigint AS cnt
FROM "Faculties"
WHERE "Name" ~* '(filtrele|kaldır|kaldir|arama|sayfa|sonraki|önceki|onceki|üniversite|universite|öğretim|ogretim|görevli|gorevli|enstitü|enstitu)';

SELECT "Id", "UniversityId", "Name"
FROM "Faculties"
WHERE "Name" ~* '(filtrele|kaldır|kaldir|arama|sayfa|sonraki|önceki|onceki|üniversite|universite|öğretim|ogretim|görevli|gorevli|enstitü|enstitu)'
ORDER BY "UniversityId", "Id"
LIMIT 50;

-- Suspicious names in Departments
SELECT
  'Suspicious Departments' AS check,
  count(*)::bigint AS cnt
FROM "Departments"
WHERE "Name" ~* '(filtrele|kaldır|kaldir|arama|sayfa|sonraki|önceki|onceki|üniversite|universite|öğretim|ogretim|görevli|gorevli)';

SELECT "Id", "FacultyId", "Name"
FROM "Departments"
WHERE "Name" ~* '(filtrele|kaldır|kaldir|arama|sayfa|sonraki|önceki|onceki|üniversite|universite|öğretim|ogretim|görevli|gorevli)'
ORDER BY "FacultyId", "Id"
LIMIT 50;

-- Bilinmiyor duplicates
SELECT
  'Bilinmiyor Faculties (total)' AS check,
  count(*)::bigint AS cnt
FROM "Faculties"
WHERE "Name" ILIKE 'bilinmiyor';

SELECT
  'Bilinmiyor Departments (total)' AS check,
  count(*)::bigint AS cnt
FROM "Departments"
WHERE "Name" ILIKE 'bilinmiyor';

-- Per-university duplicates for Bilinmiyor faculty
SELECT
  "UniversityId",
  count(*)::int AS bilinmiyor_faculties
FROM "Faculties"
WHERE "Name" ILIKE 'bilinmiyor'
GROUP BY "UniversityId"
HAVING count(*) > 1
ORDER BY bilinmiyor_faculties DESC, "UniversityId"
LIMIT 50;

-- Per-faculty duplicates for Bilinmiyor department
SELECT
  "FacultyId",
  count(*)::int AS bilinmiyor_departments
FROM "Departments"
WHERE "Name" ILIKE 'bilinmiyor'
GROUP BY "FacultyId"
HAVING count(*) > 1
ORDER BY bilinmiyor_departments DESC, "FacultyId"
LIMIT 50;

-- Most common faculty/department names (sanity)
SELECT "Name", count(*)::bigint AS cnt
FROM "Faculties"
GROUP BY "Name"
ORDER BY cnt DESC
LIMIT 20;

SELECT "Name", count(*)::bigint AS cnt
FROM "Departments"
GROUP BY "Name"
ORDER BY cnt DESC
LIMIT 20;

