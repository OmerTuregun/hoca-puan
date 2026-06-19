#!/usr/bin/env python3
"""
HocaPuan — ofansif terim adayı madenciliği (TEST/REFERANS ARACI).

Hugging Face train.csv veri setinden ham aday üretir. Çıktılar ana filtreye
beslenmez; tek otorite kaynak banned-words.tr.json'dır.
banned-words.tr.json'a OTOMATİK YAZMAZ.
"""

from __future__ import annotations

import argparse
import json
import re
import sys
import urllib.request
from collections import defaultdict
from datetime import datetime, timezone
from pathlib import Path

import pandas as pd

# ---------------------------------------------------------------------------
# Yollar
# ---------------------------------------------------------------------------

SCRIPT_DIR = Path(__file__).resolve().parent
PROJECT_ROOT = SCRIPT_DIR.parent
CACHE_DIR = SCRIPT_DIR / ".cache"
OUTPUT_DIR = SCRIPT_DIR / "output"
CACHE_CSV = CACHE_DIR / "train.csv"
BANNED_WORDS_PATH = (
    PROJECT_ROOT
    / "hocapuan-backend"
    / "src"
    / "HocaPuan.Services"
    / "Moderation"
    / "banned-words.tr.json"
)
OUTPUT_JSON = OUTPUT_DIR / "candidates-for-review.json"
HF_TRAIN_URL = (
    "https://huggingface.co/datasets/Toygar/turkish-offensive-language-detection"
    "/resolve/main/train.csv"
)

DIKKAT_NOTU = (
    "Bu kategori bağlama duyarlı olabilir, etnik/dini/siyasi kimlik ifadeleri "
    "içerebilir, otomatik ekleme YAPILMAMALI, tek tek incele."
)

# ---------------------------------------------------------------------------
# Normalizasyon — backend TextNormalizer ile mantıksal eşleşme
# (HocaPuan.Services/Moderation/TextNormalizer.cs)
# ---------------------------------------------------------------------------

LEET_MAP: dict[str, str] = {
    "4": "a",
    "@": "a",
    "3": "e",
    "1": "i",
    "!": "i",
    "|": "i",
    "0": "o",
    "5": "s",
    "$": "s",
    "7": "t",
}

_SEPARATOR_PY = re.compile(
    r"([\w\u00c0-\u024f])[\*\.·_\-/\\|]+(?=[\w\u00c0-\u024f])", re.UNICODE
)
REPEATED_LETTER = re.compile(r"(.)\1+", re.UNICODE)


def turkish_lower(text: str) -> str:
    """tr-TR küçük harf — İ→i, I→ı; diğerleri standart lower."""
    out: list[str] = []
    for ch in text:
        if ch == "İ":
            out.append("i")
        elif ch == "I":
            out.append("ı")
        else:
            out.append(ch.lower())
    return "".join(out)


def fold_turkish_chars(text: str) -> str:
    """FoldTurkishChars — eşleştirme için ASCII-benzeri forma."""
    result: list[str] = []
    for ch in text:
        if ch in ("ı", "I", "İ", "i"):
            result.append("i")
        elif ch in ("ş", "Ş"):
            result.append("s")
        elif ch in ("ç", "Ç"):
            result.append("c")
        elif ch in ("ğ", "Ğ"):
            result.append("g")
        elif ch in ("ü", "Ü"):
            result.append("u")
        elif ch in ("ö", "Ö"):
            result.append("o")
        else:
            result.append(turkish_lower(ch))
    return "".join(result)


def apply_leet(text: str) -> str:
    return "".join(LEET_MAP.get(ch, ch) for ch in text)


def collapse_repeated_letters(text: str) -> str:
    return REPEATED_LETTER.sub(r"\1", text)


def join_separated_letters(text: str) -> str:
    tokens = text.split()
    joined = [_SEPARATOR_PY.sub(r"\1", t) for t in tokens]
    return " ".join(joined)


def normalize_word(word: str) -> str:
    """TextNormalizer.NormalizeWord ile aynı sıra."""
    w = turkish_lower(word)
    w = fold_turkish_chars(w)
    w = apply_leet(w)
    w = collapse_repeated_letters(w)
    return w


def normalize_phrase(phrase: str) -> str:
    """TextNormalizer.NormalizePhrase ile aynı mantık."""
    tokens = phrase.split()
    normalized = [normalize_word(t) for t in tokens if t.strip()]
    return " ".join(t for t in normalized if t)


def letters_only_per_token(text: str) -> str:
    """TextNormalizer.LettersOnlyPerToken — token başına yalnızca harfler."""
    tokens = turkish_lower(text).split()
    cleaned: list[str] = []
    for token in tokens:
        letters = "".join(ch for ch in token if ch.isalpha())
        if letters:
            cleaned.append(turkish_lower(letters))
    return " ".join(cleaned)


def tokenize_for_mining(text: str) -> list[str]:
    """
    Madencilik için normalize edilmiş 1-gram token listesi.
    LettersOnlyPerToken + NormalizeWord; 2 karakterden kısa token'lar elenir.
    """
    if not text or not str(text).strip():
        return []
    base = letters_only_per_token(str(text))
    tokens: list[str] = []
    for raw in base.split():
        norm = normalize_word(raw)
        if len(norm) >= 2 and norm.isalpha():
            tokens.append(norm)
    return tokens


# ---------------------------------------------------------------------------
# Türkçe stopword'ler (1-gram madenciliğinde elenir)
# ---------------------------------------------------------------------------

STOPWORDS: frozenset[str] = frozenset(
    normalize_word(w)
    for w in """
    acaba acep adeta ama ancak artık aslında az bir biri birçok birkaç birşey
    biz bize bizim bu bunlar bunu buna bundan bütün çok çünkü da daha de defa
    diye eğer en etmesi gibi göre ha hala halde hele hem henüz hep hepsi her
    herhangi hiç hiçbir ile ilgili ise işte iyi kadar ki kim kime kimse mi mı
    mu mü nasıl ne neden nerede nereye niçin niye o ona onlar onu onun oysa
    pek rağmen sadece sanki sen siz şey şu şunu tabii tamamen tüm üzere var
    ve veya ya yani yine yok zaten user https http com tr co
    """.split()
)

# ---------------------------------------------------------------------------
# Kategori heuristikleri (banned-words.tr.json anahtarlarıyla uyumlu)
# ---------------------------------------------------------------------------

CATEGORY_KEYWORDS: dict[str, list[str]] = {
    "tehdit_siddet": [
        "oldur", "oldurur", "gebert", "geber", "dover", "dov", "kirarim", "kirar",
        "yakarim", "intikam", "hesap", "canina", "canini", "basarim", "pesini",
        "bulurum", "mahved", "ezersin", "ezersin", "parcala", "kopar", "olum",
        "oldureceg", "gebertir", "affetme", "unutma", "bela", "sonun",
    ],
    "kufur_agir": [
        "sik", "siktir", "siker", "sikey", "sikik", "sikko", "amk", "amq", "amina",
        "amcik", "amci", "amini", "anan", "orospu", "oc", "ibne", "pic", "pici",
        "yarrak", "yarak", "got", "göt", "kahpe", "pezevenk", "pust", "puşt",
        "dalyarak", "fahise", "kevase", "dolyatag", "yarra", "sokay", "sokar",
        "bacini", "kodugum", "kodum", "siktig", "sokim", "sokarim",
    ],
    "kufur_orta": [
        "salak", "salagi", "aptal", "mal", "bok", "lan", "ahmak", "dangalak",
        "embesil", "moron", "ezik", "pislik", "kic", "hassiktir", "lanet", "ucube",
        "manyak", "budala", "andaval", "igrenc", "rezil",
    ],
    "ayrimcilik_nefret": [
        "gavur", "kafir", "zenci", "ermeni", "kurt", "kürt", "yahudi", "arap",
        "laz", "domuz", "dinsiz", "imansiz", "ateist", "escinsel", "homofob",
        "nazi", "irkci", "nefret", "multeci", "cingene", "rum", "fasist",
    ],
    "hakaret_kisilik": [
        "yalanci", "dolandirici", "sahtekar", "ahlaksiz", "sarlatan", "rusvet",
        "taciz", "sapik", "pedofil", "hirsiz", "yobaz", "cahil", "beyinsiz",
        "kafasiz", "irkci", "fasist", "diktator", "zalim", "vicdansiz",
        "karaktersiz", "sahsiyetsiz", "zorba", "baskici", "mobbing",
    ],
    "hakaret_kucumseme": [
        "degersiz", "rezalet", "rezillik", "facia", "yetersiz", "liyakatsiz",
        "torpil", "sahte", "beceriksiz", "berbat", "utanc", "skandal", "gulunc",
        "sacma", "haksiz", "adil degil", "saygisiz", "kibirli", "narsist",
    ],
    "argo_kucuk_dusurucu": [
        "soytari", "maskara", "asalak", "parazit", "les", "ezik", "ucube",
        "tiksinc", "mide bulandir", "hor gor", "kucuk gor", "asagila",
    ],
}

CATEGORY_PRIORITY = [
    "tehdit_siddet",
    "kufur_agir",
    "kufur_orta",
    "ayrimcilik_nefret",
    "hakaret_kisilik",
    "hakaret_kucumseme",
    "argo_kucuk_dusurucu",
]


def suggest_category(term: str) -> str:
    """Basit anahtar kelime heuristiği ile kategori öner."""
    norm = normalize_phrase(term) if " " in term else normalize_word(term)
    for cat in CATEGORY_PRIORITY:
        for kw in CATEGORY_KEYWORDS[cat]:
            if kw in norm:
                return cat
    return "belirsiz"


# ---------------------------------------------------------------------------
# Veri yükleme
# ---------------------------------------------------------------------------


def download_csv_if_needed() -> Path:
    CACHE_DIR.mkdir(parents=True, exist_ok=True)
    if CACHE_CSV.exists() and CACHE_CSV.stat().st_size > 0:
        print(f"Önbellek kullanılıyor: {CACHE_CSV}")
        return CACHE_CSV

    print(f"CSV indiriliyor: {HF_TRAIN_URL}")
    urllib.request.urlretrieve(HF_TRAIN_URL, CACHE_CSV)
    print(f"Kaydedildi: {CACHE_CSV}")
    return CACHE_CSV


def load_banned_normalized() -> tuple[set[str], set[str]]:
    """Mevcut listedeki normalize 1-gram ve çok kelimeli ifadeler."""
    if not BANNED_WORDS_PATH.exists():
        print(f"UYARI: banned-words bulunamadı: {BANNED_WORDS_PATH}", file=sys.stderr)
        return set(), set()

    with BANNED_WORDS_PATH.open(encoding="utf-8") as f:
        data = json.load(f)

    unigrams: set[str] = set()
    phrases: set[str] = set()

    for key, value in data.items():
        if key.startswith("_") or not isinstance(value, list):
            continue
        for entry in value:
            if not isinstance(entry, str) or not entry.strip():
                continue
            stripped = entry.strip()
            if " " in stripped:
                phrases.add(normalize_phrase(stripped))
            else:
                unigrams.add(normalize_word(stripped))

    return unigrams, phrases


def load_dataset(csv_path: Path) -> pd.DataFrame:
    df = pd.read_csv(csv_path, dtype={"text": str})
    df["text"] = df["text"].fillna("")
    df["label"] = pd.to_numeric(df["label"], errors="coerce").fillna(0).astype(int)
    return df


# ---------------------------------------------------------------------------
# Madencilik
# ---------------------------------------------------------------------------


class TermStats:
    __slots__ = (
        "offensive_rows",
        "all_rows",
        "example_rows_offensive",
        "example_texts_offensive",
    )

    def __init__(self) -> None:
        self.offensive_rows: set[int] = set()
        self.all_rows: set[int] = set()
        self.example_rows_offensive: list[int] = []
        self.example_texts_offensive: list[str] = []


def _record_example(stats: TermStats, row_idx: int, original_text: str) -> None:
    if row_idx in stats.example_rows_offensive:
        return
    if len(stats.example_texts_offensive) >= 3:
        return
    stats.example_rows_offensive.append(row_idx)
    stats.example_texts_offensive.append(original_text.strip())


def _pick_diverse_examples(stats: TermStats) -> list[str]:
    """En fazla 3 örnek; mümkünse farklı uzunlukta tweet'ler."""
    texts = stats.example_texts_offensive
    if len(texts) <= 3:
        return texts[:3]
    sorted_by_len = sorted(texts, key=len)
    if len(sorted_by_len) == 1:
        return sorted_by_len
    picks = [sorted_by_len[0]]
    picks.append(sorted_by_len[len(sorted_by_len) // 2])
    picks.append(sorted_by_len[-1])
    # Benzersiz tut
    seen: set[str] = set()
    unique: list[str] = []
    for t in picks:
        if t not in seen:
            seen.add(t)
            unique.append(t)
    for t in sorted_by_len:
        if len(unique) >= 3:
            break
        if t not in seen:
            seen.add(t)
            unique.append(t)
    return unique[:3]


def mine_terms(
    df: pd.DataFrame,
    banned_unigrams: set[str],
    banned_phrases: set[str],
    min_freq_1gram: int,
    min_freq_ngram: int,
) -> list[dict]:
    unigram_stats: dict[str, TermStats] = defaultdict(TermStats)
    bigram_stats: dict[str, TermStats] = defaultdict(TermStats)
    trigram_stats: dict[str, TermStats] = defaultdict(TermStats)

    for row_idx, row in df.iterrows():
        text = str(row["text"])
        label = int(row["label"])
        tokens = tokenize_for_mining(text)
        if not tokens:
            continue

        is_offensive = label == 1
        seen_uni: set[str] = set()
        seen_bi: set[str] = set()
        seen_tri: set[str] = set()

        for i, tok in enumerate(tokens):
            if tok in STOPWORDS:
                continue
            if tok not in seen_uni:
                seen_uni.add(tok)
                unigram_stats[tok].all_rows.add(row_idx)
                if is_offensive:
                    unigram_stats[tok].offensive_rows.add(row_idx)
                    _record_example(unigram_stats[tok], row_idx, text)

            if i + 1 < len(tokens):
                bi = f"{tokens[i]} {tokens[i + 1]}"
                if bi not in seen_bi:
                    seen_bi.add(bi)
                    bigram_stats[bi].all_rows.add(row_idx)
                    if is_offensive:
                        bigram_stats[bi].offensive_rows.add(row_idx)
                        _record_example(bigram_stats[bi], row_idx, text)

            if i + 2 < len(tokens):
                tri = f"{tokens[i]} {tokens[i + 1]} {tokens[i + 2]}"
                if tri not in seen_tri:
                    seen_tri.add(tri)
                    trigram_stats[tri].all_rows.add(row_idx)
                    if is_offensive:
                        trigram_stats[tri].offensive_rows.add(row_idx)
                        _record_example(trigram_stats[tri], row_idx, text)

    candidates: list[dict] = []

    def maybe_add(
        term: str,
        tip: str,
        stats: TermStats,
        min_offensive_rows: int,
        banned: set[str],
    ) -> None:
        if term in banned:
            return
        off_count = len(stats.offensive_rows)
        all_count = len(stats.all_rows)
        if off_count < min_offensive_rows or all_count == 0:
            return
        ratio = round(off_count / all_count * 100, 2)
        category = suggest_category(term)
        entry: dict = {
            "terim": term,
            "tip": tip,
            "frekans_ofansif_satirlarda": off_count,
            "frekans_tum_satirlarda": all_count,
            "offensive_ratio": ratio,
            "ornek_baglamlar": _pick_diverse_examples(stats),
            "onerilen_kategori": category,
        }
        if category in ("ayrimcilik_nefret", "hakaret_kisilik"):
            entry["dikkat_notu"] = DIKKAT_NOTU
        candidates.append(entry)

    for term, stats in unigram_stats.items():
        maybe_add(term, "1gram", stats, min_freq_1gram, banned_unigrams)

    for term, stats in bigram_stats.items():
        maybe_add(term, "2gram", stats, min_freq_ngram, banned_phrases)

    for term, stats in trigram_stats.items():
        maybe_add(term, "3gram", stats, min_freq_ngram, banned_phrases)

    candidates.sort(key=lambda x: (-x["offensive_ratio"], -x["frekans_ofansif_satirlarda"]))
    return candidates


# ---------------------------------------------------------------------------
# Konsol özeti
# ---------------------------------------------------------------------------


def print_summary(candidates: list[dict], total_rows: int, min_freq: int) -> None:
    by_cat: dict[str, int] = defaultdict(int)
    for c in candidates:
        by_cat[c["onerilen_kategori"]] += 1

    print()
    print("=" * 60)
    print(f"Toplam taranan satır: {total_rows}")
    print(f"Min frekans eşiği (1-gram): {min_freq}")
    print(f"Yeni aday sayısı: {len(candidates)}")
    print("Kategori kırılımı:")
    for cat in sorted(by_cat.keys(), key=lambda k: -by_cat[k]):
        print(f"  {cat}: {by_cat[cat]}")
    print()
    print("En yüksek offensive_ratio — ilk 10 aday:")
    print(f"{'Terim':<35} {'Tip':<8} {'Oran%':>7} {'Off':>6} {'Tüm':>6} {'Kategori'}")
    print("-" * 90)
    for c in candidates[:10]:
        term = c["terim"][:34]
        print(
            f"{term:<35} {c['tip']:<8} {c['offensive_ratio']:>7.1f} "
            f"{c['frekans_ofansif_satirlarda']:>6} {c['frekans_tum_satirlarda']:>6} "
            f"{c['onerilen_kategori']}"
        )
    print("=" * 60)
    print(f"\ncandidates-for-review.json dosyasına yazıldı: {OUTPUT_JSON}")
    print("Lütfen adayları manuel inceleyin; otomatik liste güncellemesi YAPILMADI.")


# ---------------------------------------------------------------------------
# main
# ---------------------------------------------------------------------------


def parse_args() -> argparse.Namespace:
    parser = argparse.ArgumentParser(
        description="HF ofansif dil veri setinden yasaklı kelime adayları üretir."
    )
    parser.add_argument(
        "--min-freq",
        type=int,
        default=5,
        help="1-gram için minimum farklı ofansif satır sayısı (varsayılan: 5)",
    )
    parser.add_argument(
        "--min-freq-ngram",
        type=int,
        default=3,
        help="2/3-gram için minimum farklı ofansif satır sayısı (varsayılan: 3)",
    )
    return parser.parse_args()


def main() -> int:
    args = parse_args()

    csv_path = download_csv_if_needed()
    df = load_dataset(csv_path)
    offensive_count = int((df["label"] == 1).sum())
    print(f"Veri seti: {len(df)} satır ({offensive_count} ofansif)")

    banned_unigrams, banned_phrases = load_banned_normalized()
    print(f"Mevcut liste: {len(banned_unigrams)} kelime, {len(banned_phrases)} kalıp")

    candidates = mine_terms(
        df,
        banned_unigrams,
        banned_phrases,
        min_freq_1gram=args.min_freq,
        min_freq_ngram=args.min_freq_ngram,
    )

    OUTPUT_DIR.mkdir(parents=True, exist_ok=True)
    output = {
        "uretim_tarihi": datetime.now(timezone.utc).isoformat(),
        "toplam_taranan_satir": len(df),
        "min_frekans_esigi": args.min_freq,
        "min_frekans_esigi_ngram": args.min_freq_ngram,
        "adaylar": candidates,
    }
    with OUTPUT_JSON.open("w", encoding="utf-8") as f:
        json.dump(output, f, ensure_ascii=False, indent=2)

    print_summary(candidates, len(df), args.min_freq)
    return 0


if __name__ == "__main__":
    sys.exit(main())
