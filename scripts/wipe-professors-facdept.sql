-- Wipe review/professor/faculty/department data (ALL universities)
-- Leaves Universities/Users/etc intact.
--
-- Order matters due to foreign keys:
-- ReviewVotes -> Reviews -> Professors -> Departments -> Faculties
\pset pager off
\timing on

BEGIN;

DELETE FROM "ReviewVotes";
DELETE FROM "Reviews";

DELETE FROM "Professors";
DELETE FROM "Departments";
DELETE FROM "Faculties";

-- Reset university aggregates so UI doesn't show stale totals.
UPDATE "Universities"
SET "TotalProfessors" = 0,
    "TotalReviews" = 0,
    "AverageRating" = 0;

COMMIT;

-- Verify
SELECT 'ReviewVotes' AS "table", count(*)::bigint AS cnt FROM "ReviewVotes";
SELECT 'Reviews' AS "table", count(*)::bigint AS cnt FROM "Reviews";
SELECT 'Professors' AS "table", count(*)::bigint AS cnt FROM "Professors";
SELECT 'Departments' AS "table", count(*)::bigint AS cnt FROM "Departments";
SELECT 'Faculties' AS "table", count(*)::bigint AS cnt FROM "Faculties";

