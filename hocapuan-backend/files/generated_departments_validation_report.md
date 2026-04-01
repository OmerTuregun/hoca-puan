# Departments SAFE SQL Validation Report

- Source departments: `departments-compact.txt`
- Source universities map basis: `universities-compact.txt` + `db-faculties2.txt`
- High-confidence mappings: `165`
- Unresolved mappings: `58`
- Department parse format errors: `0`
- Department rows total: `11039`
- Skipped due to unmapped university: `2804`
- Skipped due to faculty mismatch: `131`
- Unique department insert candidates: `8067`
- SQL output: `generated_departments_insert_safe.sql`
- Safe mapping output: `generated_yok_to_db_map_safe.txt`

## Safety Rule
- Mapping included only if best candidate has full faculty coverage and second-best does not.

## Sample Unresolved yokUniversityId
- `3005`: ambiguous_or_low cover=0.684 jac=0.542
- `1001`: ambiguous_or_low cover=0.867 jac=0.650
- `2024`: ambiguous_or_low cover=0.857 jac=0.571
- `2031`: ambiguous_or_low cover=0.692 jac=0.600
- `2005`: ambiguous_or_low cover=0.727 jac=0.615
- `1002`: ambiguous_or_low cover=1.000 jac=1.000
- `2041`: ambiguous_or_low cover=0.700 jac=0.412
- `4052`: ambiguous_or_low cover=1.000 jac=0.556
- `1111`: ambiguous_or_low cover=1.000 jac=1.000
- `2021`: ambiguous_or_low cover=0.556 jac=0.208
- `2103`: ambiguous_or_low cover=0.889 jac=0.727
- `2025`: ambiguous_or_low cover=0.556 jac=0.357
- `2035`: ambiguous_or_low cover=0.778 jac=0.438
- `2060`: ambiguous_or_low cover=0.556 jac=0.294
- `1095`: ambiguous_or_low cover=1.000 jac=1.000
- `2090`: ambiguous_or_low cover=0.750 jac=0.273
- `2023`: ambiguous_or_low cover=1.000 jac=0.615
- `2030`: ambiguous_or_low cover=0.875 jac=0.700
- `1051`: ambiguous_or_low cover=1.000 jac=1.000
- `2076`: ambiguous_or_low cover=1.000 jac=1.000
- `2027`: ambiguous_or_low cover=0.714 jac=0.312
- `2039`: ambiguous_or_low cover=0.857 jac=0.375
- `2040`: ambiguous_or_low cover=0.714 jac=0.417
- `2048`: ambiguous_or_low cover=0.714 jac=0.250
- `2058`: ambiguous_or_low cover=0.857 jac=0.316
- `1022`: ambiguous_or_low cover=1.000 jac=1.000
- `1049`: ambiguous_or_low cover=1.000 jac=1.000
- `2012`: ambiguous_or_low cover=0.833 jac=0.333
- `2017`: ambiguous_or_low cover=0.833 jac=0.294
- `2094`: ambiguous_or_low cover=1.000 jac=1.000
- `2065`: ambiguous_or_low cover=1.000 jac=1.000
- `4039`: ambiguous_or_low cover=0.833 jac=0.417
- `1084`: ambiguous_or_low cover=1.000 jac=1.000
- `2001`: ambiguous_or_low cover=1.000 jac=1.000
- `2011`: ambiguous_or_low cover=1.000 jac=1.000
- `2097`: ambiguous_or_low cover=0.800 jac=0.571
- `2088`: ambiguous_or_low cover=1.000 jac=0.500
- `2067`: ambiguous_or_low cover=1.000 jac=1.000
- `4088`: ambiguous_or_low cover=0.600 jac=0.375
- `1126`: ambiguous_or_low cover=1.000 jac=1.000

