-- YÖK Playwright import stores professors under department "Bilinmiyor".
-- Soft-delete those rows (app uses IsDeleted global filter on Professors).

BEGIN;

SELECT COUNT(*) AS before_count
FROM "Professors" p
INNER JOIN "Departments" d ON p."DepartmentId" = d."Id"
WHERE d."Name" = 'Bilinmiyor' AND p."IsDeleted" = false;

UPDATE "Courses"
SET "IsDeleted" = true, "UpdatedAt" = NOW()
WHERE "ProfessorId" IN (
    SELECT p."Id"
    FROM "Professors" p
    INNER JOIN "Departments" d ON p."DepartmentId" = d."Id"
    WHERE d."Name" = 'Bilinmiyor' AND p."IsDeleted" = false
)
AND "IsDeleted" = false;

UPDATE "Reviews"
SET "IsDeleted" = true, "UpdatedAt" = NOW()
WHERE "ProfessorId" IN (
    SELECT p."Id"
    FROM "Professors" p
    INNER JOIN "Departments" d ON p."DepartmentId" = d."Id"
    WHERE d."Name" = 'Bilinmiyor' AND p."IsDeleted" = false
)
AND "IsDeleted" = false;

UPDATE "Professors" p
SET "IsDeleted" = true, "UpdatedAt" = NOW()
FROM "Departments" d
WHERE p."DepartmentId" = d."Id"
  AND d."Name" = 'Bilinmiyor'
  AND p."IsDeleted" = false;

SELECT COUNT(*) AS remaining_active
FROM "Professors" p
INNER JOIN "Departments" d ON p."DepartmentId" = d."Id"
WHERE d."Name" = 'Bilinmiyor' AND p."IsDeleted" = false;

COMMIT;
