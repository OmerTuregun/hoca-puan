# Faculties SQL Validation Report

- Source file: `db-faculties2.txt`
- Parsed rows: `1841`
- Format errors: `0`
- University FK missing rows: `0`
- Duplicate (UniversityId, FacultyName) groups: `0`
- Unique rows to insert: `1841`
- SQL output: `generated_faculties_insert.sql`

## Note
- `departments-compact.txt` format is `yokUniversityId|Department§Faculty`.
- Without a separate `yokId -> dbUniversityId` map, department SQL cannot be generated safely.
