import json

with open('scripts/output/hizli-onay.json', encoding='utf-8') as f:
    data = json.load(f)

print('Toplam:', data.get('toplam_aday'))
print('Dagilim:', data.get('kategori_dagilimi'))