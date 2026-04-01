-- Counts for core tables (before/after wipes)
\pset pager off
\timing on

SELECT 'ReviewVotes' AS "table", count(*)::bigint AS cnt FROM "ReviewVotes";
SELECT 'Reviews' AS "table", count(*)::bigint AS cnt FROM "Reviews";
SELECT 'Professors' AS "table", count(*)::bigint AS cnt FROM "Professors";
SELECT 'Departments' AS "table", count(*)::bigint AS cnt FROM "Departments";
SELECT 'Faculties' AS "table", count(*)::bigint AS cnt FROM "Faculties";

