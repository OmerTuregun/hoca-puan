SELECT "Id", "Username", "Email", "Role", "IsEmailVerified", "CreatedAt"
FROM "Users"
WHERE "Username" ILIKE '%ömer%'
   OR "Username" ILIKE '%omer%'
   OR "Email" ILIKE '%ömer%'
   OR "Email" ILIKE '%omer%'
   OR "Username" ILIKE '%faruk%'
ORDER BY "Id";
