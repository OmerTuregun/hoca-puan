# YOK Import - 3 Soru Cevap

## 1) "YOK Import controller'ini nasil yazdin - formData olarak ne bekliyor?"

`ImportController` tarafinda iki endpoint var:

- `POST /api/import/yok/preview`
- `POST /api/import/yok/import-professors`

`preview` endpoint'i `YokScrapeRequestDto` aliyor ve su alan bekliyor:

- `formData: Dictionary<string,string>` (bos olamaz, yoksa `400 BadRequest`)

Yani backend teknik olarak **key/value form alanlari** bekliyor. Tip bagimsiz degil; hepsi string olarak gonderilmeli.

Pratikte kullanilan alanlar:

- `aramaTerim`
- `islem`
- `yazarCheckbox`
- `kitapCheckbox`
- `PatentCheckbox`
- `projeCheckbox`
- `MakaleCheckbox` (ayrica geriye donuk typo anahtar da destekli)
- `BildiriCheckbox`
- `SanatsalCheckbox`
- `TezCheckbox`

`import-professors` endpoint'i `YokBulkImportRequestDto` aliyor:

- `universityIds: number[]`
- `maxPerUniversity: number` (backend `10..250` araligina clamp ediyor)
- `formDataTemplate?: Dictionary<string,string>` (opsiyonel, varsa import aramalarinda template olarak kullaniliyor)

## 2) "Frontend su an calisiyor mu, hangi portta?"

Evet, su an container durumu "Up".

`docker compose ps` ciktisina gore:

- `hocapuan_frontend` calisiyor
- port mapping: `0.0.0.0:5173 -> 5173/tcp`

Yani host uzerinden frontend adresi: `http://localhost:5173`

## 3) "Admin hesabiyla giris yapmayi denedin mi?"

Hayir, bu oturumda admin hesabiyla login testi yapmadim.

Yaptigim seyler:

- backend kod degisiklikleri
- import endpoint cagrilarindan donen cevaplari analiz etme
- docker compose ile servisleri build/up etme

Ama UI veya API login endpoint'i ile admin kimlik bilgileri kullanarak aktif giris denemesi yapmadim.

