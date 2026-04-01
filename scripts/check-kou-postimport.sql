-- Post-import validation for Kocaeli University (Id=142)
\pset pager off
\timing on

SELECT 'Professors(KOU)' AS check, count(*)::bigint AS cnt
FROM "Professors" WHERE "UniversityId" = 142;

SELECT 'Faculties(KOU)' AS check, count(*)::bigint AS cnt
FROM "Faculties" WHERE "UniversityId" = 142;

SELECT 'Departments(KOU)' AS check, count(*)::bigint AS cnt
FROM "Departments" d
JOIN "Faculties" f ON f."Id" = d."FacultyId"
WHERE f."UniversityId" = 142;

SELECT f."Name" AS faculty, count(*)::bigint AS departments
FROM "Faculties" f
JOIN "Departments" d ON d."FacultyId" = f."Id"
WHERE f."UniversityId" = 142
GROUP BY f."Name"
ORDER BY departments DESC, faculty
LIMIT 20;

SELECT 'Corrupt Departments(KOU) has ?' AS check, count(*)::bigint AS cnt
FROM "Departments" d
JOIN "Faculties" f ON f."Id" = d."FacultyId"
WHERE f."UniversityId" = 142
  AND d."Name" LIKE '%?%';

SELECT 'Furkan Göz in KOU' AS check, count(*)::bigint AS cnt
FROM "Professors" p
WHERE p."UniversityId" = 142
  AND (p."FirstName" || ' ' || p."LastName") ILIKE '%Furkan%Göz%';

SELECT p."Id", p."FirstName", p."LastName"
FROM "Professors" p
WHERE p."UniversityId" = 142
  AND (p."FirstName" || ' ' || p."LastName") ILIKE '%Furkan%Göz%'
LIMIT 5;

SELECT 'Turkish-letter names in KOU (sample count)' AS check, count(*)::bigint AS cnt
FROM "Professors" p
WHERE p."UniversityId" = 142
  AND (p."FirstName" || ' ' || p."LastName") ~ '[İIıŞşĞğÜüÖöÇçÂâÎîÛû]';

SELECT p."Id", p."FirstName", p."LastName"
FROM "Professors" p
WHERE p."UniversityId" = 142
  AND (p."FirstName" || ' ' || p."LastName") ~ '[İIıŞşĞğÜüÖöÇçÂâÎîÛû]'
ORDER BY p."Id"
LIMIT 10;

