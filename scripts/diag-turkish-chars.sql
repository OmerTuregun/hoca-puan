-- Diagnose whether Turkish diacritics are preserved in DB
\pset pager off
\timing on

-- Specific spot checks
SELECT
  'LastName=Yılmaz' AS check,
  count(*)::bigint  AS cnt
FROM "Professors"
WHERE "LastName" ILIKE '%Yılmaz%';

SELECT
  'LastName=Yilmaz' AS check,
  count(*)::bigint  AS cnt
FROM "Professors"
WHERE "LastName" ILIKE '%Yilmaz%';

SELECT
  'FirstName=Pınar' AS check,
  count(*)::bigint  AS cnt
FROM "Professors"
WHERE "FirstName" ILIKE '%Pınar%';

SELECT
  'FirstName=Pinar' AS check,
  count(*)::bigint  AS cnt
FROM "Professors"
WHERE "FirstName" ILIKE '%Pinar%';

-- Broad scan: how many rows contain any Turkish-specific letters in first/last name?
SELECT
  'HasTurkishChars(first+last)' AS check,
  count(*)::bigint              AS cnt
FROM "Professors"
WHERE ("FirstName" || ' ' || "LastName") ~ '[İIıŞşĞğÜüÖöÇçÂâÎîÛû]';

SELECT
  'HasNoTurkishChars(first+last)' AS check,
  count(*)::bigint                AS cnt
FROM "Professors"
WHERE ("FirstName" || ' ' || "LastName") !~ '[İIıŞşĞğÜüÖöÇçÂâÎîÛû]';

-- Samples (if any) to visually verify stored encoding
SELECT
  "Id",
  "FirstName",
  "LastName",
  "UniversityId"
FROM "Professors"
WHERE ("FirstName" || ' ' || "LastName") ~ '[İIıŞşĞğÜüÖöÇçÂâÎîÛû]'
ORDER BY "Id"
LIMIT 20;

