-- Debug: verify professor existence and university match
\pset pager off
\timing on

-- 1) Global matches for name contains
SELECT
  p."Id",
  p."FirstName",
  p."LastName",
  p."UniversityId"
FROM "Professors" p
WHERE (p."FirstName" || ' ' || p."LastName") ILIKE '%Furkan%Göz%'
ORDER BY p."Id"
LIMIT 50;

-- 2) Same matches joined to university name (if FK is populated)
SELECT
  p."Id",
  (p."FirstName" || ' ' || p."LastName") AS "FullName",
  u."Id" AS "UniId",
  u."Name" AS "University"
FROM "Professors" p
LEFT JOIN "Universities" u ON u."Id" = p."UniversityId"
WHERE (p."FirstName" || ' ' || p."LastName") ILIKE '%Furkan%Göz%'
ORDER BY u."Name" NULLS LAST, p."Id"
LIMIT 200;

-- 3) Verify current UI filter universityId=142
SELECT "Id", "Name" FROM "Universities" WHERE "Id" = 142;

-- 4) Check matches within universityId=142 specifically
SELECT
  p."Id",
  p."FirstName",
  p."LastName",
  p."UniversityId"
FROM "Professors" p
WHERE p."UniversityId" = 142
  AND (p."FirstName" || ' ' || p."LastName") ILIKE '%Furkan%Göz%'
ORDER BY p."Id"
LIMIT 50;

