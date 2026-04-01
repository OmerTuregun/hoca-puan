from pathlib import Path
from collections import Counter


root = Path(r"c:/Users/omerfaruk.turegun/source/repos/HocaPuan/hocapuan-backend/files")
fac_file = root / "db-faculties2.txt"
dbu_file = root / "db-universities.txt"
out_sql = root / "generated_faculties_insert.sql"
out_report = root / "generated_faculties_validation_report.md"


def load_universities():
    uni_ids = set()
    uni_names = {}
    for line in dbu_file.read_text(encoding="utf-8", errors="replace").splitlines():
        p = line.split("|", 1)
        if len(p) == 2 and p[0].strip().isdigit():
            uid = int(p[0].strip())
            uni_ids.add(uid)
            uni_names[uid] = p[1].strip()
    return uni_ids, uni_names


def parse_faculties():
    rows = []
    format_errors = []
    for i, line in enumerate(fac_file.read_text(encoding="utf-8", errors="replace").splitlines(), start=1):
        p = line.split("|")
        if len(p) < 3:
            format_errors.append((i, line))
            continue
        fid_raw, uid_raw = p[0].strip(), p[1].strip()
        name = "|".join(p[2:]).strip()
        if not (fid_raw.isdigit() and uid_raw.isdigit()) or not name:
            format_errors.append((i, line))
            continue
        rows.append((int(fid_raw), int(uid_raw), name))
    return rows, format_errors


def generate():
    uni_ids, uni_names = load_universities()
    rows, format_errors = parse_faculties()

    fk_missing = [r for r in rows if r[1] not in uni_ids]
    pair_counts = Counter((r[1], r[2].casefold()) for r in rows)
    dup_pairs = [(uid, name_cf, cnt) for (uid, name_cf), cnt in pair_counts.items() if cnt > 1]

    seen = set()
    unique_rows = []
    for _, uid, name in rows:
        key = (uid, name.casefold())
        if key in seen:
            continue
        seen.add(key)
        unique_rows.append((uid, name))

    sql_lines = []
    sql_lines.append("-- Auto-generated from db-faculties2.txt")
    sql_lines.append("-- Validated: format, FK(university), duplicate faculty names per university")
    sql_lines.append("BEGIN;")
    for uid, name in sorted(unique_rows, key=lambda x: (x[0], x[1].casefold())):
        esc = name.replace("'", "''")
        sql_lines.append(
            f"INSERT INTO \"Faculties\" (\"Name\", \"UniversityId\", \"CreatedAt\", \"UpdatedAt\") "
            f"SELECT '{esc}', {uid}, NOW(), NOW() "
            f"WHERE EXISTS (SELECT 1 FROM \"Universities\" u WHERE u.\"Id\" = {uid}) "
            f"AND NOT EXISTS (SELECT 1 FROM \"Faculties\" f WHERE f.\"UniversityId\" = {uid} AND lower(f.\"Name\") = lower('{esc}'));"
        )
    sql_lines.append("COMMIT;")
    out_sql.write_text("\n".join(sql_lines) + "\n", encoding="utf-8")

    report = []
    report.append("# Faculties SQL Validation Report")
    report.append("")
    report.append(f"- Source file: `{fac_file.name}`")
    report.append(f"- Parsed rows: `{len(rows)}`")
    report.append(f"- Format errors: `{len(format_errors)}`")
    report.append(f"- University FK missing rows: `{len(fk_missing)}`")
    report.append(f"- Duplicate (UniversityId, FacultyName) groups: `{len(dup_pairs)}`")
    report.append(f"- Unique rows to insert: `{len(unique_rows)}`")
    report.append(f"- SQL output: `{out_sql.name}`")
    report.append("")

    if format_errors:
        report.append("## Sample Format Errors")
        for ln, txt in format_errors[:10]:
            report.append(f"- line {ln}: `{txt[:140]}`")
        report.append("")

    if fk_missing:
        report.append("## Sample FK Missing Rows")
        for fid, uid, name in fk_missing[:10]:
            report.append(f"- facultyRowId={fid}, universityId={uid}, name=`{name}`")
        report.append("")

    if dup_pairs:
        report.append("## Sample Duplicate Groups")
        name_lookup = {(uid, n.casefold()): n for _, uid, n in rows}
        for uid, name_cf, cnt in dup_pairs[:20]:
            dname = name_lookup[(uid, name_cf)]
            report.append(f"- universityId={uid} ({uni_names.get(uid, '?')}), faculty=`{dname}`, count={cnt}")
        report.append("")

    report.append("## Note")
    report.append("- `departments-compact.txt` format is `yokUniversityId|Department§Faculty`.")
    report.append("- Without a separate `yokId -> dbUniversityId` map, department SQL cannot be generated safely.")
    out_report.write_text("\n".join(report) + "\n", encoding="utf-8")

    print("generated_sql", out_sql)
    print("generated_report", out_report)
    print(
        "rows",
        len(rows),
        "format_errors",
        len(format_errors),
        "fk_missing",
        len(fk_missing),
        "dup_groups",
        len(dup_pairs),
        "unique_rows",
        len(unique_rows),
    )


if __name__ == "__main__":
    generate()
