-- Detect actual corruption characters in DB (question mark / replacement char)
\pset pager off
\timing on

-- Counts of literal '?' in names
SELECT 'Departments contains ?' AS check, count(*)::bigint AS cnt
FROM "Departments" WHERE "Name" LIKE '%?%';

SELECT 'Faculties contains ?' AS check, count(*)::bigint AS cnt
FROM "Faculties" WHERE "Name" LIKE '%?%';

SELECT 'Professors first/last contains ?' AS check, count(*)::bigint AS cnt
FROM "Professors"
WHERE "FirstName" LIKE '%?%' OR "LastName" LIKE '%?%';

-- Counts of unicode replacement character U+FFFD (�)
SELECT 'Departments contains �' AS check, count(*)::bigint AS cnt
FROM "Departments" WHERE "Name" LIKE '%�%';

SELECT 'Faculties contains �' AS check, count(*)::bigint AS cnt
FROM "Faculties" WHERE "Name" LIKE '%�%';

SELECT 'Professors first/last contains �' AS check, count(*)::bigint AS cnt
FROM "Professors"
WHERE "FirstName" LIKE '%�%' OR "LastName" LIKE '%�%';

-- Show a few samples with '?' if any
SELECT 'Dept sample ?' AS label, "Id", "Name" FROM "Departments" WHERE "Name" LIKE '%?%' LIMIT 10;
SELECT 'Fac sample ?'  AS label, "Id", "Name" FROM "Faculties"  WHERE "Name" LIKE '%?%' LIMIT 10;
SELECT 'Prof sample ?' AS label, "Id", "FirstName", "LastName" FROM "Professors"
WHERE "FirstName" LIKE '%?%' OR "LastName" LIKE '%?%' LIMIT 10;

-- Show encoding details for a few department names that look suspicious in output:
-- pick those that include only ASCII but may have been displayed badly; also show hex bytes.
SELECT
  d."Id",
  d."Name",
  length(d."Name")          AS chars,
  octet_length(d."Name")    AS bytes,
  encode(convert_to(d."Name",'UTF8'),'hex') AS utf8_hex
FROM "Departments" d
WHERE d."Name" ILIKE '%muhendis%'
ORDER BY d."Id"
LIMIT 5;

