# YOK Import Live Test Guide (Independent Environment)

Bu dokuman, testlerin **senin mevcut local projeni kullanmadan**, Claude'un kendi canli/ayri ortami uzerinden yapilmasi icindir.

## 1) Test amaci

`/api/import/yok/preview` ve `/api/import/yok/import-professors` endpointlerinin YOK'ten veri cekebildigini dogrulamak.

Kritik not: `parsedCandidates=0` her zaman parser hatasi degildir. `post=418` ve "Your Access To This Page Has Been Blocked" gorulurse sorun **WAF/IP block**'tur.

## 2) Gerekli bilgiler (canli ortama ait)

Claude bu bilgileri kendi canli ortamindan kullanmali:

- `BASE_URL` (canli API adresi, or. `https://api.example.com`)
- Admin/Moderator rolune sahip JWT token
- Test edilecek universite ID listesi (canli DB'de var olan id'ler)

## 3) Test sirası (zorunlu)

1. Once `yok/preview` cagrisi yap.
2. Sonra `yok/import-professors` cagrisi yap.
3. Sonuclari birlikte yorumla.

## 4) Preview testi

Endpoint: `POST {BASE_URL}/api/import/yok/preview`

Ornek request:

```json
{
  "formData": {
    "aramaTerim": "Abdullah Gül Üniversitesi",
    "islem": "1",
    "yazarCheckbox": "on",
    "kitapCheckbox": "on",
    "PatentCheckbox": "on",
    "projeCheckbox": "on",
    "MakaleCheckbox": "on",
    "BildiriCheckbox": "on",
    "SanatsalCheckbox": "on",
    "TezCheckbox": "on"
  }
}
```

Beklenen inceleme alanlari:

- `postStatusCode`
- `finalStatusCode`
- `htmlLength`
- `htmlPreview`

Yorum:

- `418` + block mesaji => WAF/IP block.
- `200/302` + anlamli html => import testine gec.

## 5) Import testi

Endpoint: `POST {BASE_URL}/api/import/yok/import-professors`

Ornek request:

```json
{
  "universityIds": [1, 2, 3, 4, 5],
  "maxPerUniversity": 50,
  "formDataTemplate": {
    "islem": "1",
    "yazarCheckbox": "on",
    "kitapCheckbox": "on",
    "PatentCheckbox": "on",
    "projeCheckbox": "on",
    "MakaleCheckbox": "on",
    "BildiriCheckbox": "on",
    "SanatsalCheckbox": "on",
    "TezCheckbox": "on"
  }
}
```

Beklenen basari sinyalleri:

- `parsedCandidates > 0`
- `insertedProfessors > 0` (ilk calistirmalarda)
- `notes` icinde `X aday islendi.`

## 6) Sonuc yorumlama tablosu

- `401/403`: JWT veya rol hatasi.
- `400`: request body eksik/hatali.
- `200 + parsedCandidates=0 + post/final=418`: WAF/IP block (parser degil).
- `200 + parsedCandidates>0 + insertedProfessors=0 + skippedDuplicates>0`: duplicate kayitlar.

## 7) cURL komutlari (canli ortam)

`<BASE_URL>` ve `<TOKEN>` alanlarini canli degerlerle degistir.

Preview:

```bash
curl -X POST "<BASE_URL>/api/import/yok/preview" \
  -H "Authorization: Bearer <TOKEN>" \
  -H "Content-Type: application/json" \
  -d '{
    "formData": {
      "aramaTerim": "Abdullah Gül Üniversitesi",
      "islem": "1",
      "yazarCheckbox": "on",
      "kitapCheckbox": "on",
      "PatentCheckbox": "on",
      "projeCheckbox": "on",
      "MakaleCheckbox": "on",
      "BildiriCheckbox": "on",
      "SanatsalCheckbox": "on",
      "TezCheckbox": "on"
    }
  }'
```

Import:

```bash
curl -X POST "<BASE_URL>/api/import/yok/import-professors" \
  -H "Authorization: Bearer <TOKEN>" \
  -H "Content-Type: application/json" \
  -d '{
    "universityIds": [1,2,3,4,5],
    "maxPerUniversity": 50,
    "formDataTemplate": {
      "islem": "1",
      "yazarCheckbox": "on",
      "kitapCheckbox": "on",
      "PatentCheckbox": "on",
      "projeCheckbox": "on",
      "MakaleCheckbox": "on",
      "BildiriCheckbox": "on",
      "SanatsalCheckbox": "on",
      "TezCheckbox": "on"
    }
  }'
```

## 8) WAF/IP block cikarsa ne yapilmali

Claude sunlari raporlamali:

- `timestamp`
- `Request ID`
- blocklanan `IP Address`
- endpoint ve payload ozeti

Sonra:

1. Farkli egress IP ile ayni testi tekrar et.
2. Sonuclar farkliysa IP block kesinlesir.
3. YOK tarafina whitelist/support talebi acilir.

## 9) Claude'a verilecek tek satir gorev

"Lutfen bu testleri kendi canli ortaminda calistir; local projeyi veya local docker'i kullanma. Once `/yok/preview`, sonra `/yok/import-professors` dene. `post/final/html/hint` alanlariyla sonucu raporla; 418 + blocked mesaji varsa WAF/IP block olarak isaretle."

