# HocaPuan yardımcı scriptleri

Bu klasördeki Python araçları **build/CI sürecine dahil değildir**; yalnızca manuel test ve referans amaçlıdır.

## Kelime listesi stratejisi

| Kaynak | Rol |
|--------|-----|
| **`banned-words.tr.json`** | **Tek otorite** — elle hazırlanmış, kategorilenmiş, gözden geçirilmiş ana filtre listesi. `ContentModerationService` yalnızca bu dosyayı okur. |
| `candidates-for-review.json`, `hizli-onay.json`, `dikkatli-inceleme.json` | **Test/referans** — HF/Kaggle veri setinden madencilikle üretilmiş, gözden geçirilmemiş ham adaylar. Ana filtreye **otomatik beslenmez**. |

Madencilik çıktılarında bilinen sorunlar: gerçek kişi adları, tarihi şahıs isimleri, `amk + X` n-gram şişmesi, yanlış kategori atamaları. Bu nedenle bu dosyalar doğrudan listeye eklenmez.

### Önerilen iş akışı

1. Zaman zaman `test-filter-against-dataset.py` çalıştırılır.
2. `scripts/output/filtrenin-kacirdiklari.json` incelenir — mevcut filtrenin **kaçırdığı** (false negative) ofansif satırlar ve öneri terimler.
3. Gerçekten genel geçer, bağlamdan bağımsız küfür/hakaret olan kelimeler **manuel** olarak `banned-words.tr.json`'a eklenir (doğru kategoriyle).
4. Özel isimler, siyasi/etnik bağlama duyarlı ifadeler, gereksiz n-gram kombinasyonları ana listeye **eklenmez**; yalnızca not edilir.

İsteğe bağlı: madencilik scriptleri (`mine-offensive-terms.py`, `filter-candidates.py`) ek keşif için çalıştırılabilir; çıktılar yine manuel inceleme içindir, otomatik ekleme yapılmaz.

---

## Filtre testi: veri setine karşı false negative raporu

`test-filter-against-dataset.py`, backend `ContentModerationService` ile **mantıksal olarak uyumlu** eşleştirme (`scripts/moderation_match.py`) kullanarak HF `train.csv` içindeki `label=1` satırları tarar.

**Amaç:** Mevcut `banned-words.tr.json` filtresinin kaçırdığı gerçek küfürleri bulmak — ana listeyi otomatik genişletmek değil.

### Kurulum

```bash
pip install -r scripts/requirements-mining.txt
```

### Çalıştırma

```bash
python scripts/test-filter-against-dataset.py
```

| Parametre | Varsayılan | Açıklama |
|-----------|------------|----------|
| `--csv` | `scripts/.cache/train.csv` | HF train.csv yolu |
| `--banned-words` | backend `banned-words.tr.json` | Tek otorite liste |
| `--limit` | `0` (tümü) | Hızlı test için satır limiti |
| `--max-terms` | `8` | Kaçırılan satır başına öneri terim sayısı |
| `--no-download` | — | CSV yoksa indirme |

### Çıktı

`scripts/output/filtrenin-kacirdiklari.json`:

- `kacirilan_satirlar` — filtre tarafından yakalanmayan ofansif tweet'ler
- Her satırda `muhtemel_kacan_kelimeler` / `muhtemel_kacan_ngramlar` (listede olmayan sık terimler)
- `en_sik_oneri_terimler` — tüm kaçırılan satırlarda öne çıkan genel terim önerileri
- `olası_ozel_isimler` — yaygın ad/soyad veya `@kullanıcı` bağlamı nedeniyle **DİKKAT: olası özel isim, incele** notuyla ayrılmış bölüm

---

## Madencilik araçları (test/referans — ana filtreye beslenmez)

### `mine-offensive-terms.py`

HF veri setinden ham aday üretir → `scripts/output/candidates-for-review.json`

```bash
python scripts/mine-offensive-terms.py --min-freq 5 --min-freq-ngram 3
```

### `filter-candidates.py`

`candidates-for-review.json` dosyasını yönetilebilir alt kümelere böler:

- `scripts/output/hizli-onay.json`
- `scripts/output/dikkatli-inceleme.json`

```bash
python scripts/filter-candidates.py --ratio-threshold 95 --dikkatli-ratio-threshold 70
```

Bu çıktılar **gözden geçirilmemiş** aday listeleridir; `banned-words.tr.json`'a otomatik eklenmez.

CSV önbelleği: `scripts/.cache/train.csv`

---

### Diğer scriptler

Bu klasörde ayrıca veritabanı import/diagnostic SQL ve PowerShell scriptleri bulunur.
