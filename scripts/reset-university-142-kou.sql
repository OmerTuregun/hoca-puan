-- Reset Kocaeli University (Id=142) related data for a clean re-import test
\pset pager off
\timing on

BEGIN;

-- 1) Delete reviews tied to professors in this university (if any)
DELETE FROM "ReviewVotes"
WHERE "ReviewId" IN (
  SELECT r."Id"
  FROM "Reviews" r
  JOIN "Professors" p ON p."Id" = r."ProfessorId"
  WHERE p."UniversityId" = 142
);

DELETE FROM "Reviews"
WHERE "ProfessorId" IN (
  SELECT "Id" FROM "Professors" WHERE "UniversityId" = 142
);

-- 2) Delete professors for university
DELETE FROM "Professors" WHERE "UniversityId" = 142;

-- 3) Delete departments/faculties for university (if schema uses these)
WITH fac AS (
  SELECT "Id" FROM "Faculties" WHERE "UniversityId" = 142
)
DELETE FROM "Departments"
WHERE "FacultyId" IN (SELECT "Id" FROM fac);

DELETE FROM "Faculties" WHERE "UniversityId" = 142;

-- 4) Reset university stats
UPDATE "Universities"
SET "TotalProfessors" = 0,
    "TotalReviews" = 0,
    "AverageRating" = 0
WHERE "Id" = 142;

COMMIT;

-- Verify
SELECT 'Professors(KOU)' AS check, count(*)::bigint AS cnt FROM "Professors" WHERE "UniversityId" = 142;
SELECT 'Faculties(KOU)'  AS check, count(*)::bigint AS cnt FROM "Faculties"  WHERE "UniversityId" = 142;
SELECT 'Departments(KOU)' AS check, count(*)::bigint AS cnt
FROM "Departments" d
JOIN "Faculties" f ON f."Id" = d."FacultyId"
WHERE f."UniversityId" = 142;

