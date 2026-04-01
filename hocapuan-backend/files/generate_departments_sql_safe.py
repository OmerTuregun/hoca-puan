from pathlib import Path
from collections import defaultdict
import re


root = Path(r"c:/Users/omerfaruk.turegun/source/repos/HocaPuan/hocapuan-backend/files")
universities_compact = root / "universities-compact.txt"   # yokUniversityId:faculty|faculty|...
db_faculties = root / "db-faculties2.txt"                  # facultyRowId|dbUniversityId|facultyName
departments_compact = root / "departments-compact.txt"     # yokUniversityId|Department§Faculty

out_sql = root / "generated_departments_insert_safe.sql"
out_report = root / "generated_departments_validation_report.md"
out_map = root / "generated_yok_to_db_map_safe.txt"


def norm(s: str) -> str:
    s = s.strip().lower()
    tr = {
        "ı": "i",
        "İ": "i",
        "ş": "s",
        "ğ": "g",
        "ü": "u",
        "ö": "o",
        "ç": "c",
        "â": "a",
        "î": "i",
        "û": "u",
    }
    for a, b in tr.items():
        s = s.replace(a, b)
    s = re.sub(r"[^a-z0-9 ]+", " ", s)
    s = re.sub(r"\s+", " ", s).strip()
    return s


def parse_universities_compact():
    m = {}
    for line in universities_compact.read_text(encoding="utf-8", errors="replace").splitlines():
        if ":" not in line:
            continue
        yok_raw, faculties_raw = line.split(":", 1)
        yok_raw = yok_raw.strip()
        if not yok_raw.isdigit():
            continue
        yid = int(yok_raw)
        fset = {norm(x) for x in faculties_raw.split("|") if x.strip()}
        m[yid] = fset
    return m


def parse_db_faculties():
    fac_by_uni = defaultdict(set)
    fac_name_lookup = defaultdict(dict)  # uni -> normName -> original
    fac_id_lookup = defaultdict(dict)    # uni -> normName -> facultyId
    for line in db_faculties.read_text(encoding="utf-8", errors="replace").splitlines():
        p = line.split("|")
        if len(p) < 3 or not p[0].strip().isdigit() or not p[1].strip().isdigit():
            continue
        fid = int(p[0].strip())
        uid = int(p[1].strip())
        name = "|".join(p[2:]).strip()
        n = norm(name)
        fac_by_uni[uid].add(n)
        if n not in fac_name_lookup[uid]:
            fac_name_lookup[uid][n] = name
        if n not in fac_id_lookup[uid]:
            fac_id_lookup[uid][n] = fid
    return fac_by_uni, fac_name_lookup, fac_id_lookup


def build_safe_map(yok_fac, db_fac):
    # Strict confidence rule:
    # - best candidate has full coverage of yok faculty set (cover=1.0)
    # - second best cover < 1.0 (unique on strongest criterion)
    safe = {}
    unresolved = []

    for yid, yset in yok_fac.items():
        candidates = []
        for uid, dset in db_fac.items():
            inter = len(yset & dset)
            if inter == 0:
                continue
            cover = inter / len(yset) if yset else 0.0
            union = len(yset | dset)
            jac = inter / union if union else 0.0
            candidates.append((cover, jac, inter, uid))

        if not candidates:
            unresolved.append((yid, "no_candidate"))
            continue

        candidates.sort(reverse=True)
        top = candidates[0]
        second = candidates[1] if len(candidates) > 1 else None

        if top[0] == 1.0 and (second is None or second[0] < 1.0):
            safe[yid] = top[3]
        else:
            unresolved.append((yid, f"ambiguous_or_low cover={top[0]:.3f} jac={top[1]:.3f}"))

    return safe, unresolved


def parse_departments():
    rows = []
    format_errors = []
    for i, line in enumerate(departments_compact.read_text(encoding="utf-8", errors="replace").splitlines(), start=1):
        # expected: yokId|Department§Faculty
        if "|" not in line or "§" not in line:
            format_errors.append((i, line))
            continue
        yok_raw, rest = line.split("|", 1)
        if not yok_raw.strip().isdigit():
            format_errors.append((i, line))
            continue
        dep, fac = rest.split("§", 1)
        dep = dep.strip()
        fac = fac.strip()
        if not dep or not fac:
            format_errors.append((i, line))
            continue
        rows.append((int(yok_raw.strip()), dep, fac))
    return rows, format_errors


def main():
    yok_fac = parse_universities_compact()
    db_fac, _, fac_id_lookup = parse_db_faculties()
    safe_map, unresolved_map = build_safe_map(yok_fac, db_fac)
    dep_rows, dep_format_errors = parse_departments()

    sql_lines = [
        "-- Auto-generated SAFE departments SQL",
        "-- Only high-confidence yokUniversityId -> dbUniversityId mappings are included",
        "BEGIN;",
    ]

    inserted_candidates = 0
    skipped_unmapped = 0
    skipped_missing_faculty = 0

    unique_pairs = set()  # (dbUniId, facultyNorm, departmentNorm)
    for yid, dep_name, fac_name in dep_rows:
        db_uid = safe_map.get(yid)
        if db_uid is None:
            skipped_unmapped += 1
            continue

        fac_norm = norm(fac_name)
        dep_norm = norm(dep_name)
        if fac_norm not in db_fac.get(db_uid, set()):
            skipped_missing_faculty += 1
            continue

        faculty_id = fac_id_lookup[db_uid].get(fac_norm)
        if faculty_id is None:
            skipped_missing_faculty += 1
            continue

        key = (db_uid, fac_norm, dep_norm)
        if key in unique_pairs:
            continue
        unique_pairs.add(key)

        dep_esc = dep_name.replace("'", "''")
        sql_lines.append(
            f"INSERT INTO \"Departments\" (\"Name\", \"FacultyId\", \"CreatedAt\", \"UpdatedAt\", \"IsDeleted\") "
            f"SELECT '{dep_esc}', {faculty_id}, NOW(), NOW(), FALSE "
            f"WHERE EXISTS (SELECT 1 FROM \"Faculties\" f WHERE f.\"Id\" = {faculty_id}) "
            f"AND NOT EXISTS (SELECT 1 FROM \"Departments\" d WHERE d.\"FacultyId\" = {faculty_id} AND lower(d.\"Name\") = lower('{dep_esc}'));"
        )
        inserted_candidates += 1

    sql_lines.append("COMMIT;")
    out_sql.write_text("\n".join(sql_lines) + "\n", encoding="utf-8")

    map_lines = [f"{yid}|{db_uid}" for yid, db_uid in sorted(safe_map.items())]
    out_map.write_text("\n".join(map_lines) + "\n", encoding="utf-8")

    report = []
    report.append("# Departments SAFE SQL Validation Report")
    report.append("")
    report.append(f"- Source departments: `{departments_compact.name}`")
    report.append(f"- Source universities map basis: `{universities_compact.name}` + `{db_faculties.name}`")
    report.append(f"- High-confidence mappings: `{len(safe_map)}`")
    report.append(f"- Unresolved mappings: `{len(unresolved_map)}`")
    report.append(f"- Department parse format errors: `{len(dep_format_errors)}`")
    report.append(f"- Department rows total: `{len(dep_rows)}`")
    report.append(f"- Skipped due to unmapped university: `{skipped_unmapped}`")
    report.append(f"- Skipped due to faculty mismatch: `{skipped_missing_faculty}`")
    report.append(f"- Unique department insert candidates: `{inserted_candidates}`")
    report.append(f"- SQL output: `{out_sql.name}`")
    report.append(f"- Safe mapping output: `{out_map.name}`")
    report.append("")
    report.append("## Safety Rule")
    report.append("- Mapping included only if best candidate has full faculty coverage and second-best does not.")
    report.append("")
    if unresolved_map:
        report.append("## Sample Unresolved yokUniversityId")
        for yid, reason in unresolved_map[:40]:
            report.append(f"- `{yid}`: {reason}")
        report.append("")
    out_report.write_text("\n".join(report) + "\n", encoding="utf-8")

    print("safe_map", len(safe_map))
    print("unresolved_map", len(unresolved_map))
    print("dep_rows", len(dep_rows))
    print("dep_format_errors", len(dep_format_errors))
    print("skipped_unmapped", skipped_unmapped)
    print("skipped_missing_faculty", skipped_missing_faculty)
    print("insert_candidates", inserted_candidates)
    print("sql", out_sql)
    print("report", out_report)
    print("map", out_map)


if __name__ == "__main__":
    main()
