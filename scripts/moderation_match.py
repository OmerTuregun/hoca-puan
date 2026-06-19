"""
HocaPuan — ContentModerationService ile mantıksal uyumlu eşleştirme (test scriptleri için).

Tek otorite kaynak: hocapuan-backend/.../banned-words.tr.json
Mining/candidate JSON dosyaları bu modüle DAHİL DEĞİLDİR.
"""

from __future__ import annotations

import json
import re
from pathlib import Path
from typing import Iterable

SCRIPT_DIR = Path(__file__).resolve().parent
PROJECT_ROOT = SCRIPT_DIR.parent
BANNED_WORDS_PATH = (
    PROJECT_ROOT
    / "hocapuan-backend"
    / "src"
    / "HocaPuan.Services"
    / "Moderation"
    / "banned-words.tr.json"
)

LEET_MAP: dict[str, str] = {
    "4": "a", "@": "a", "3": "e", "1": "i", "!": "i", "|": "i",
    "0": "o", "5": "s", "$": "s", "7": "t",
}

_SEPARATOR_PY = re.compile(
    r"([\w\u00c0-\u024f])[\*\.·_\-/\\|]+(?=[\w\u00c0-\u024f])", re.UNICODE
)
REPEATED_LETTER = re.compile(r"(.)\1+", re.UNICODE)

TC_CANDIDATE = re.compile(r"\b\d{11}\b")
TURKISH_PHONE = re.compile(
    r"(?:\+90[\s\-.]?)?0?\s*5\d{2}[\s\-.]?\d{3}[\s\-.]?\d{2}[\s\-.]?\d{2}\b|"
    r"\b5\d{2}[\s\-.]?\d{3}[\s\-.]?\d{2}[\s\-.]?\d{2}\b|"
    r"\(\s*0?5\d{2}\s*\)[\s\-.]?\d{3}[\s\-.]?\d{2}[\s\-.]?\d{2}",
    re.UNICODE,
)
EMAIL = re.compile(
    r"\b[A-Za-z0-9._%+\-]+@[A-Za-z0-9.\-]+\.[A-Za-z]{2,}\b",
    re.IGNORECASE,
)

CATEGORY_DISPLAY = {
    "kufur_agir": "Küfür",
    "kufur_orta": "Küfür",
    "hakaret_kisilik": "Hakaret",
    "hakaret_kucumseme": "Hakaret",
    "argo_kucuk_dusurucu": "Hakaret",
    "ayrimcilik_nefret": "Nefret",
    "tehdit_siddet": "Tehdit",
}


def turkish_lower(text: str) -> str:
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
    return " ".join(_SEPARATOR_PY.sub(r"\1", t) for t in tokens)


def normalize_word(word: str) -> str:
    w = turkish_lower(word)
    w = fold_turkish_chars(w)
    w = apply_leet(w)
    w = collapse_repeated_letters(w)
    return w


def normalize_phrase(phrase: str) -> str:
    tokens = phrase.split()
    return " ".join(normalize_word(t) for t in tokens if t.strip())


def letters_only_per_token(text: str) -> str:
    tokens = turkish_lower(text).split()
    cleaned = []
    for token in tokens:
        letters = "".join(ch for ch in token if ch.isalpha())
        if letters:
            cleaned.append(turkish_lower(letters))
    return " ".join(cleaned)


def build_match_variants(text: str) -> list[str]:
    if not text or not text.strip():
        return []
    lowered = turkish_lower(text)
    variants = [
        lowered,
        fold_turkish_chars(lowered),
        apply_leet(fold_turkish_chars(lowered)),
        join_separated_letters(lowered),
        fold_turkish_chars(join_separated_letters(lowered)),
        apply_leet(fold_turkish_chars(join_separated_letters(lowered))),
        letters_only_per_token(lowered),
        fold_turkish_chars(letters_only_per_token(lowered)),
        apply_leet(fold_turkish_chars(letters_only_per_token(lowered))),
        collapse_repeated_letters(letters_only_per_token(lowered)),
        collapse_repeated_letters(lowered),
        fold_turkish_chars(collapse_repeated_letters(lowered)),
        collapse_repeated_letters(apply_leet(fold_turkish_chars(lowered))),
    ]
    return list(dict.fromkeys(variants))


def is_multi_word_phrase(entry: str) -> bool:
    return len(entry.split()) > 1


def build_word_regex(normalized_word: str) -> re.Pattern[str]:
    escaped = re.escape(normalized_word)
    return re.compile(
        rf"(?<![\w\d]){escaped}(?![\w\d])",
        re.IGNORECASE | re.UNICODE,
    )


def build_phrase_regex(normalized_phrase: str) -> re.Pattern[str]:
    parts = normalized_phrase.split()
    pattern = r"[\W_]*".join(re.escape(p) for p in parts)
    return re.compile(pattern, re.IGNORECASE | re.UNICODE)


def is_valid_tc_kimlik(digits: str) -> bool:
    if len(digits) != 11 or not digits.isdigit() or digits[0] == "0":
        return False
    d = [int(c) for c in digits]
    odd = d[0] + d[2] + d[4] + d[6] + d[8]
    even = d[1] + d[3] + d[5] + d[7]
    digit10 = (odd * 7 - even) % 10
    if digit10 < 0:
        digit10 += 10
    if d[9] != digit10:
        return False
    return d[10] == sum(d[:10]) % 10


def contains_personal_data(text: str) -> bool:
    for match in TC_CANDIDATE.finditer(text):
        if is_valid_tc_kimlik(match.group()):
            return True
    if TURKISH_PHONE.search(text):
        return True
    if EMAIL.search(text):
        return True
    return False


def matches_word_pattern(pattern: re.Pattern[str], variant: str) -> bool:
    if pattern.search(variant):
        return True
    for token in variant.split():
        if token and pattern.search(token):
            return True
    return False


class ContentModerationMatcher:
    """banned-words.tr.json tabanlı eşleştirici (backend ContentModerationService ile uyumlu)."""

    def __init__(self, banned_path: Path | None = None) -> None:
        path = banned_path or BANNED_WORDS_PATH
        self._banned_unigrams: set[str] = set()
        self._banned_phrases: set[str] = set()
        self._word_patterns: list[tuple[str, re.Pattern[str]]] = []
        self._phrase_patterns: list[tuple[str, re.Pattern[str]]] = []
        self._load(path)

    def _load(self, path: Path) -> None:
        with path.open(encoding="utf-8") as f:
            data = json.load(f)

        entries_by_category: dict[str, list[str]] = {}
        for key, value in data.items():
            if key.startswith("_") or not isinstance(value, list):
                continue
            cat = CATEGORY_DISPLAY.get(key, key)
            entries_by_category.setdefault(cat, []).extend(
                str(item).strip() for item in value if isinstance(item, str) and item.strip()
            )

        for category, entries in entries_by_category.items():
            for entry in entries:
                if is_multi_word_phrase(entry):
                    norm = normalize_phrase(entry)
                    if len(norm.split()) < 2:
                        continue
                    self._banned_phrases.add(norm)
                    self._phrase_patterns.append((category, build_phrase_regex(norm)))
                else:
                    norm = normalize_word(entry)
                    if len(norm) < 2:
                        continue
                    self._banned_unigrams.add(norm)
                    self._word_patterns.append((category, build_word_regex(norm)))

    @property
    def banned_unigrams(self) -> frozenset[str]:
        return frozenset(self._banned_unigrams)

    @property
    def banned_phrases(self) -> frozenset[str]:
        return frozenset(self._banned_phrases)

    def moderate(self, text: str) -> tuple[bool, list[str]]:
        """(is_allowed, matched_categories)"""
        if not text or not text.strip():
            return True, []

        if contains_personal_data(text):
            return False, ["KisiselVeri"]

        categories: list[str] = []
        variants = build_match_variants(text)

        for category, pattern in self._word_patterns:
            if any(matches_word_pattern(pattern, v) for v in variants):
                categories.append(category)

        for category, pattern in self._phrase_patterns:
            if any(pattern.search(v) for v in variants):
                categories.append(category)

        if categories:
            return False, list(dict.fromkeys(categories))
        return True, []

    def is_blocked(self, text: str) -> bool:
        return not self.moderate(text)[0]


def tokenize_for_analysis(text: str) -> list[str]:
    base = letters_only_per_token(str(text))
    tokens: list[str] = []
    for raw in base.split():
        norm = normalize_word(raw)
        if len(norm) >= 2 and norm.isalpha():
            tokens.append(norm)
    return tokens


def extract_ngrams(tokens: list[str], n: int) -> list[str]:
    if len(tokens) < n:
        return []
    return [" ".join(tokens[i : i + n]) for i in range(len(tokens) - n + 1)]
