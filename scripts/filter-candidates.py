#!/usr/bin/env python3
"""
HocaPuan — madencilik adaylarını alt kümelere böler (TEST/REFERANS ARACI).

candidates-for-review.json çıktısını okur; hizli-onay.json ve
dikkatli-inceleme.json üretir. Ana filtreye beslenmez.
banned-words.tr.json'a OTOMATİK YAZMAZ.
"""

from __future__ import annotations

import argparse
import json
import sys
from collections import defaultdict
from datetime import datetime, timezone
from pathlib import Path
from typing import Any

# ---------------------------------------------------------------------------
# Yollar
# ---------------------------------------------------------------------------

SCRIPT_DIR = Path(__file__).resolve().parent
OUTPUT_DIR = SCRIPT_DIR / "output"
DEFAULT_INPUT = OUTPUT_DIR / "candidates-for-review.json"
HIZLI_ONAY_OUTPUT = OUTPUT_DIR / "hizli-onay.json"
DIKKATLI_OUTPUT = OUTPUT_DIR / "dikkatli-inceleme.json"

HIZLI_KATEGORILER = ("kufur_agir", "kufur_orta", "tehdit_siddet")
DIKKATLI_KATEGORILER = (
    "hakaret_kisilik",
    "hakaret_kucumseme",
    "ayrimcilik_nefret",
    "argo_kucuk_dusurucu",
)

NEDEN_DIKKATLI: dict[str, str] = {
    "hakaret_kisilik": (
        "Kişilik hakareti kategorisi; bağlama göre yanlış pozitif riski olabilir."
    ),
    "hakaret_kucumseme": (
        "Küçümseme hakareti kategorisi; akademik yorumlarda bağlam önemlidir."
    ),
    "ayrimcilik_nefret": (
        "Ayrımcılık/nefret kategorisi bağlama duyarlı olabilir, "
        "etnik/dini kimlik ifadesi içerebilir."
    ),
    "argo_kucuk_dusurucu": (
        "Argo/küçük düşürücü ifade kategorisi; bağlama göre değerlendirilmelidir."
    ),
}


# ---------------------------------------------------------------------------
# Yardımcılar
# ---------------------------------------------------------------------------


def load_candidates(path: Path) -> list[dict[str, Any]]:
    if not path.exists():
        print(f"HATA: Girdi dosyası bulunamadı: {path}", file=sys.stderr)
        sys.exit(1)

    with path.open(encoding="utf-8") as f:
        data = json.load(f)

    adaylar = data.get("adaylar")
    if not isinstance(adaylar, list):
        print("HATA: 'adaylar' listesi bulunamadı veya geçersiz.", file=sys.stderr)
        sys.exit(1)

    return adaylar


def sort_by_ratio(adaylar: list[dict[str, Any]]) -> list[dict[str, Any]]:
    return sorted(
        adaylar,
        key=lambda x: (-x.get("offensive_ratio", 0), -x.get("frekans_ofansif_satirlarda", 0)),
    )


def group_by_category(
    adaylar: list[dict[str, Any]],
    categories: tuple[str, ...],
) -> dict[str, list[dict[str, Any]]]:
    buckets: dict[str, list[dict[str, Any]]] = {cat: [] for cat in categories}
    for aday in adaylar:
        cat = aday.get("onerilen_kategori", "")
        if cat in buckets:
            buckets[cat].append(aday)
    for cat in categories:
        buckets[cat] = sort_by_ratio(buckets[cat])
    return buckets


def category_distribution(grouped: dict[str, list[dict[str, Any]]]) -> dict[str, int]:
    return {cat: len(items) for cat, items in grouped.items()}


def enrich_dikkatli(aday: dict[str, Any]) -> dict[str, Any]:
    enriched = dict(aday)
    cat = aday.get("onerilen_kategori", "")
    if "dikkat_notu" in aday and aday["dikkat_notu"]:
        enriched["neden_dikkatli"] = aday["dikkat_notu"]
    else:
        enriched["neden_dikkatli"] = NEDEN_DIKKATLI.get(
            cat, "Bu kategori bağlama duyarlı olabilir, tek tek inceleyin."
        )
    return enriched


def matches_hizli(aday: dict[str, Any], ratio_threshold: float) -> bool:
    cat = aday.get("onerilen_kategori", "")
    ratio = aday.get("offensive_ratio", 0)
    return cat in HIZLI_KATEGORILER and ratio >= ratio_threshold


def matches_dikkatli(aday: dict[str, Any], ratio_threshold: float) -> bool:
    cat = aday.get("onerilen_kategori", "")
    ratio = aday.get("offensive_ratio", 0)
    return cat in DIKKATLI_KATEGORILER and ratio >= ratio_threshold


def write_json(path: Path, payload: dict[str, Any]) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    with path.open("w", encoding="utf-8") as f:
        json.dump(payload, f, ensure_ascii=False, indent=2)


def print_category_breakdown(label: str, dist: dict[str, int], total: int) -> None:
    print(f"  {label}: {total} adet")
    for cat, count in sorted(dist.items(), key=lambda x: -x[1]):
        if count > 0:
            print(f"    {cat}: {count}")


# ---------------------------------------------------------------------------
# Ana işlem
# ---------------------------------------------------------------------------


def filter_candidates(
    adaylar: list[dict[str, Any]],
    ratio_threshold: float,
    dikkatli_ratio_threshold: float,
) -> tuple[list[dict], list[dict], dict[str, int]]:
    hizli: list[dict[str, Any]] = []
    dikkatli: list[dict[str, Any]] = []

    belirsiz_count = 0
    esik_alti_count = 0

    for aday in adaylar:
        if matches_hizli(aday, ratio_threshold):
            hizli.append(aday)
        elif matches_dikkatli(aday, dikkatli_ratio_threshold):
            dikkatli.append(enrich_dikkatli(aday))
        else:
            if aday.get("onerilen_kategori") == "belirsiz":
                belirsiz_count += 1
            else:
                esik_alti_count += 1

    excluded = {"belirsiz": belirsiz_count, "esik_alti": esik_alti_count}
    return hizli, dikkatli, excluded


def build_hizli_output(
    hizli: list[dict[str, Any]],
    source_name: str,
    ratio_threshold: float,
) -> dict[str, Any]:
    grouped = group_by_category(hizli, HIZLI_KATEGORILER)
    return {
        "uretim_tarihi": datetime.now(timezone.utc).isoformat(),
        "kaynak_dosya": source_name,
        "kullanilan_esik": ratio_threshold,
        "toplam_aday": len(hizli),
        "kategori_dagilimi": category_distribution(grouped),
        "adaylar": grouped,
    }


def build_dikkatli_output(
    dikkatli: list[dict[str, Any]],
    source_name: str,
    ratio_threshold: float,
) -> dict[str, Any]:
    grouped = group_by_category(dikkatli, DIKKATLI_KATEGORILER)
    return {
        "uretim_tarihi": datetime.now(timezone.utc).isoformat(),
        "kaynak_dosya": source_name,
        "kullanilan_esik": ratio_threshold,
        "toplam_aday": len(dikkatli),
        "kategori_dagilimi": category_distribution(grouped),
        "adaylar": grouped,
    }


def print_summary(
    hizli_dist: dict[str, int],
    hizli_total: int,
    dikkatli_dist: dict[str, int],
    dikkatli_total: int,
    excluded: dict[str, int],
    ratio_threshold: float,
    dikkatli_ratio_threshold: float,
) -> None:
    print()
    print("=" * 60)
    print("ADAY FİLTRELEME ÖZETİ")
    print("=" * 60)
    print(f"Hızlı onay eşiği (offensive_ratio >= {ratio_threshold}):")
    print_category_breakdown("hizli-onay.json", hizli_dist, hizli_total)
    print()
    print(f"Dikkatli inceleme eşiği (offensive_ratio >= {dikkatli_ratio_threshold}):")
    print_category_breakdown("dikkatli-inceleme.json", dikkatli_dist, dikkatli_total)
    print()
    elenen_toplam = excluded["belirsiz"] + excluded["esik_alti"]
    print(f"Elenen/belirsiz (dosyaya yazılmadı): {elenen_toplam} adet")
    print(f"  belirsiz kategori: {excluded['belirsiz']}")
    print(f"  eşik altı / diğer: {excluded['esik_alti']}")
    print("=" * 60)
    print()
    print(f"hizli-onay.json -> {HIZLI_ONAY_OUTPUT}")
    print(f"dikkatli-inceleme.json -> {DIKKATLI_OUTPUT}")
    print()
    print(
        "İlk önce hizli-onay.json'ı gözden geçirmenizi öneririm, "
        "sonra dikkatli-inceleme.json'ı kategori kategori inceleyin."
    )
    print("banned-words.tr.json otomatik güncellenmedi — manuel onay gerekir.")


# ---------------------------------------------------------------------------
# CLI
# ---------------------------------------------------------------------------


def parse_args() -> argparse.Namespace:
    parser = argparse.ArgumentParser(
        description="candidates-for-review.json adaylarını yönetilebilir alt kümelere böler."
    )
    parser.add_argument(
        "--input",
        type=Path,
        default=DEFAULT_INPUT,
        help=f"Girdi JSON (varsayılan: {DEFAULT_INPUT})",
    )
    parser.add_argument(
        "--ratio-threshold",
        type=float,
        default=95.0,
        help="Hızlı onay için minimum offensive_ratio (varsayılan: 95)",
    )
    parser.add_argument(
        "--dikkatli-ratio-threshold",
        type=float,
        default=70.0,
        help="Dikkatli inceleme için minimum offensive_ratio (varsayılan: 70)",
    )
    return parser.parse_args()


def main() -> int:
    args = parse_args()
    adaylar = load_candidates(args.input)
    print(f"Girdi: {args.input} ({len(adaylar)} aday)")

    hizli, dikkatli, excluded = filter_candidates(
        adaylar,
        args.ratio_threshold,
        args.dikkatli_ratio_threshold,
    )

    source_name = args.input.name
    hizli_payload = build_hizli_output(hizli, source_name, args.ratio_threshold)
    dikkatli_payload = build_dikkatli_output(
        dikkatli, source_name, args.dikkatli_ratio_threshold
    )

    write_json(HIZLI_ONAY_OUTPUT, hizli_payload)
    write_json(DIKKATLI_OUTPUT, dikkatli_payload)

    print_summary(
        hizli_payload["kategori_dagilimi"],
        hizli_payload["toplam_aday"],
        dikkatli_payload["kategori_dagilimi"],
        dikkatli_payload["toplam_aday"],
        excluded,
        args.ratio_threshold,
        args.dikkatli_ratio_threshold,
    )
    return 0


if __name__ == "__main__":
    sys.exit(main())
