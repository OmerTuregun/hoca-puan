#!/usr/bin/env python3
"""
HocaPuan — mevcut banned-words.tr.json filtresini HF veri setine karşı test eder.

Yalnızca rapor üretir; banned-words.tr.json veya mining çıktılarına OTOMATİK YAZMAZ.
Tek otorite kaynak: banned-words.tr.json (ContentModerationService ile aynı mantık).
"""

from __future__ import annotations

import argparse
import json
import re
import sys
import urllib.request
from collections import Counter
from datetime import datetime, timezone
from pathlib import Path
from typing import Iterable

import pandas as pd

from moderation_match import (
    BANNED_WORDS_PATH,
    ContentModerationMatcher,
    extract_ngrams,
    normalize_word,
    tokenize_for_analysis,
)

SCRIPT_DIR = Path(__file__).resolve().parent
CACHE_DIR = SCRIPT_DIR / ".cache"
OUTPUT_DIR = SCRIPT_DIR / "output"
CACHE_CSV = CACHE_DIR / "train.csv"
OUTPUT_JSON = OUTPUT_DIR / "filtrenin-kacirdiklari.json"
HF_TRAIN_URL = (
    "https://huggingface.co/datasets/Toygar/turkish-offensive-language-detection"
    "/resolve/main/train.csv"
)

STOPWORDS = frozenset(
    normalize_word(w)
    for w in """
    acaba acep adeta ama ancak artık aslında az bir biri birçok birkaç birşey
    biz bize bizim bu bunlar bunu buna bundan bütün çok çünkü da daha de defa
    diye eğer en etmesi gibi göre ha hala halde hele hem henüz hep hepsi her
    herhangi hiç hiçbir ile ilgili ise işte iyi kadar ki kim kime kimse mi mı
    mu mü nasıl ne neden nerede nereye niçin niye o ona onlar onu onun oysa
    pek rağmen sadece sanki sen siz şey şu şunu tabii tamamen tüm üzere var
    ve veya ya yani yine yok zaten user https http com tr co rt
    bi icin degil boyle gibi olan olur olmaz cok cunku artik hic hala
    sadece sonra once simdi burada orada neden nasil niye zaten bile
    """.split()
)

# Yaygın Türkçe ad/soyad ve sık karışan ünlü/sporcu isimleri (özel isim heuristic)
COMMON_PROPER_NAMES = frozenset(
    normalize_word(n)
    for n in """
    mehmet ahmet mustafa ali hasan huseyin ibrahim osman yusuf murat emre
    burak serkan omer faruk furkan enes can emir arda kerem baris cenk
    ayse fatma emine hatice zeynep elif meryem busra esra sibel derya
    yilmaz kaya demir celik sahin yildiz arslan dogan kilic aslan
    erdogan kilicdaroglu bahceli imamoglu
    messi ronaldo neymar ibrahovic ozil messi cristiano
    recep tayyip kemal ataturk menderes demirel
    baris manco tarkan hadise aleyna
    """.split()
)

AT_MENTION = re.compile(r"@([\w\u00c0-\u024f]+)", re.UNICODE)
DIKKAT_OZEL_ISIM = "DİKKAT: olası özel isim, incele"


def download_csv_if_needed() -> Path:
    CACHE_DIR.mkdir(parents=True, exist_ok=True)
    if CACHE_CSV.exists() and CACHE_CSV.stat().st_size > 0:
        return CACHE_CSV
    print(f"CSV indiriliyor: {HF_TRAIN_URL}")
    urllib.request.urlretrieve(HF_TRAIN_URL, CACHE_CSV)
    return CACHE_CSV


def load_offensive_rows(csv_path: Path, limit: int | None) -> pd.DataFrame:
    df = pd.read_csv(csv_path, dtype={"text": str})
    df["text"] = df["text"].fillna("")
    df["label"] = pd.to_numeric(df["label"], errors="coerce").fillna(0).astype(int)
    offensive = df[df["label"] == 1].copy()
    if limit and limit > 0:
        offensive = offensive.head(limit)
    return offensive


def term_in_banned_list(term: str, matcher: ContentModerationMatcher) -> bool:
    norm = normalize_word(term) if " " not in term else term
    if " " in term:
        from moderation_match import normalize_phrase
        return normalize_phrase(term) in matcher.banned_phrases
    return norm in matcher.banned_unigrams


def guess_missed_terms(
    text: str,
    matcher: ContentModerationMatcher,
    max_terms: int = 8,
) -> tuple[list[str], list[str]]:
    """Satırda listede olmayan sık 1-gram ve 2-gram adayları."""
    tokens = [t for t in tokenize_for_analysis(text) if t not in STOPWORDS]
    if not tokens:
        return [], []

    token_counts = Counter(tokens)
    missed_unigrams = [
        t for t, _ in token_counts.most_common()
        if not term_in_banned_list(t, matcher)
    ][:max_terms]

    bigram_counts: Counter[str] = Counter()
    for bg in extract_ngrams(tokens, 2):
        if not term_in_banned_list(bg, matcher):
            bigram_counts[bg] += 1
    missed_bigrams = [t for t, _ in bigram_counts.most_common(max_terms)]

    return missed_unigrams[:max_terms], missed_bigrams[:max_terms]


def collect_at_mention_stats(texts: Iterable[str]) -> Counter[str]:
    counts: Counter[str] = Counter()
    for text in texts:
        for m in AT_MENTION.finditer(text):
            handle = normalize_word(m.group(1))
            if len(handle) >= 2:
                counts[handle] += 1
    return counts


def is_probable_proper_name(
    token: str,
    original_text: str,
    at_mention_counts: Counter[str],
) -> bool:
    norm = normalize_word(token)
    if norm in COMMON_PROPER_NAMES:
        return True
    if at_mention_counts.get(norm, 0) >= 3:
        return True
    if re.search(rf"@{re.escape(norm)}\b", original_text, re.IGNORECASE):
        return True
    return False


def run_test(
    matcher: ContentModerationMatcher,
    offensive_df: pd.DataFrame,
    max_terms_per_row: int,
) -> dict:
    missed_rows: list[dict] = []
    caught = 0
    proper_name_agg: dict[str, dict] = {}

    texts = offensive_df["text"].tolist()
    at_stats = collect_at_mention_stats(texts)

    for idx, row in offensive_df.iterrows():
        text = str(row["text"])
        row_id = row.get("id")
        if pd.isna(row_id):
            row_id = int(idx)

        if not matcher.is_blocked(text):
            uni, bi = guess_missed_terms(text, matcher, max_terms_per_row)
            missed_rows.append({
                "satir_id": row_id if not isinstance(row_id, float) else int(row_id),
                "metin": text,
                "muhtemel_kacan_kelimeler": uni,
                "muhtemel_kacan_ngramlar": bi,
            })

            for term in uni:
                if is_probable_proper_name(term, text, at_stats):
                    entry = proper_name_agg.setdefault(term, {
                        "terim": term,
                        "kacirilan_satir_sayisi": 0,
                        "dikkat_notu": DIKKAT_OZEL_ISIM,
                        "ornek_baglamlar": [],
                    })
                    entry["kacirilan_satir_sayisi"] += 1
                    if len(entry["ornek_baglamlar"]) < 3 and text not in entry["ornek_baglamlar"]:
                        entry["ornek_baglamlar"].append(text)
        else:
            caught += 1

    total = len(offensive_df)
    missed = len(missed_rows)

    # Genel öneri: tüm kaçırılan satırlarda sık geçen, listede olmayan terimler
    global_term_counts: Counter[str] = Counter()
    for item in missed_rows:
        for t in item["muhtemel_kacan_kelimeler"]:
            if not is_probable_proper_name(t, item["metin"], at_stats):
                global_term_counts[t] += 1

    oneri_terimler = [
        {"terim": t, "kacirilan_satir_sayisi": c}
        for t, c in global_term_counts.most_common(50)
    ]

    olasi_ozel_isimler = sorted(
        proper_name_agg.values(),
        key=lambda x: -x["kacirilan_satir_sayisi"],
    )

    return {
        "uretim_tarihi": datetime.now(timezone.utc).isoformat(),
        "kaynak": {
            "filtre": str(BANNED_WORDS_PATH.relative_to(SCRIPT_DIR.parent)),
            "veri_seti": "Toygar/turkish-offensive-language-detection train.csv (label=1)",
        },
        "aciklama": (
            "Bu rapor yalnızca test/referans amaçlıdır. Çıktıdaki terimler "
            "banned-words.tr.json'a otomatik eklenmez."
        ),
        "toplam_ofansif_satir": total,
        "filtre_yakaladi": caught,
        "filtre_kacirdi": missed,
        "yakalama_orani_yuzde": round(caught / total * 100, 2) if total else 0.0,
        "kacirilan_satirlar": missed_rows,
        "en_sik_oneri_terimler": oneri_terimler,
        "olası_ozel_isimler": olasi_ozel_isimler,
    }


def print_summary(report: dict) -> None:
    print()
    print("=" * 60)
    print("FİLTRE TEST ÖZETİ (banned-words.tr.json)")
    print("=" * 60)
    print(f"Toplam ofansif satır: {report['toplam_ofansif_satir']}")
    print(f"Filtre yakaladı:       {report['filtre_yakaladi']}")
    print(f"Filtre kaçırdı:        {report['filtre_kacirdi']}")
    print(f"Yakalama oranı:        %{report['yakalama_orani_yuzde']}")
    print()
    if report["en_sik_oneri_terimler"][:5]:
        print("En sık önerilen terimler (özel isimler hariç, ilk 5):")
        for item in report["en_sik_oneri_terimler"][:5]:
            print(f"  {item['terim']}: {item['kacirilan_satir_sayisi']} satır")
    print()
    print(f"Olası özel isim uyarısı: {len(report['olası_ozel_isimler'])} terim")
    print(f"Çıktı: {OUTPUT_JSON}")
    print("=" * 60)
    print("Rapor incelendikten sonra uygun terimler MANUEL olarak")
    print("banned-words.tr.json'a eklenmelidir; otomatik ekleme yapılmadı.")


def parse_args() -> argparse.Namespace:
    parser = argparse.ArgumentParser(
        description="Mevcut banned-words.tr.json filtresinin HF veri setindeki kaçırdıklarını raporlar."
    )
    parser.add_argument(
        "--csv",
        type=Path,
        default=CACHE_CSV,
        help="train.csv yolu (varsayılan: scripts/.cache/train.csv)",
    )
    parser.add_argument(
        "--banned-words",
        type=Path,
        default=BANNED_WORDS_PATH,
        help="Tek otorite kelime listesi (varsayılan: backend banned-words.tr.json)",
    )
    parser.add_argument(
        "--limit",
        type=int,
        default=0,
        help="Test için satır limiti (0 = tüm label=1 satırlar)",
    )
    parser.add_argument(
        "--max-terms",
        type=int,
        default=8,
        help="Kaçırılan satır başına max öneri terim sayısı",
    )
    parser.add_argument(
        "--no-download",
        action="store_true",
        help="CSV yoksa indirme, hata ver",
    )
    return parser.parse_args()


def main() -> int:
    args = parse_args()

    if not args.banned_words.exists():
        print(f"HATA: banned-words bulunamadı: {args.banned_words}", file=sys.stderr)
        return 1

    csv_path = args.csv
    if not csv_path.exists():
        if args.no_download:
            print(f"HATA: CSV bulunamadı: {csv_path}", file=sys.stderr)
            return 1
        csv_path = download_csv_if_needed()
    else:
        print(f"CSV: {csv_path}")

    limit = args.limit if args.limit > 0 else None
    offensive_df = load_offensive_rows(csv_path, limit)
    print(f"Test edilecek ofansif satır: {len(offensive_df)}")
    print(f"Filtre kaynağı: {args.banned_words}")

    matcher = ContentModerationMatcher(args.banned_words)
    print(f"Yüklü kelime: {len(matcher.banned_unigrams)}, kalıp: {len(matcher.banned_phrases)}")

    report = run_test(matcher, offensive_df, args.max_terms)
    OUTPUT_DIR.mkdir(parents=True, exist_ok=True)
    with OUTPUT_JSON.open("w", encoding="utf-8") as f:
        json.dump(report, f, ensure_ascii=False, indent=2)

    print_summary(report)
    return 0


if __name__ == "__main__":
    sys.exit(main())
