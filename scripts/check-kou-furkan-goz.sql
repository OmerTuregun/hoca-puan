-- Check whether "Furkan Göz" exists within Kocaeli Üniversitesi (Id=142)
\pset pager off
\timing on

SELECT "Id", "Name" FROM "Universities" WHERE "Id" = 142;

SELECT
  p."Id",
  p."FirstName",
  p."LastName",
  (p."FirstName" || ' ' || p."LastName") AS "FullName",
  p."UniversityId"
FROM "Professors" p
WHERE p."UniversityId" = 142
  AND (p."FirstName" || ' ' || p."LastName") ILIKE '%Furkan%Göz%'
ORDER BY p."Id"
LIMIT 50;

SELECT
  count(*)::bigint AS "CountInKOU"
FROM "Professors" p
WHERE p."UniversityId" = 142
  AND (p."FirstName" || ' ' || p."LastName") ILIKE '%Furkan%Göz%';

