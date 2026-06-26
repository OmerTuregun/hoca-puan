# Hocanı Yorumla — Backend

ASP.NET Core API katmanı. Genel proje dokümantasyonu, kurulum ve deployment için **[../README.md](../README.md)** ve **[../DEPLOYMENT.md](../DEPLOYMENT.md)** dosyalarına bakın.

## Yerel geliştirme (Docker dışı)

```bash
# Proje kökünde .env.development olmalı
cd hocapuan-backend
dotnet run --project src/HocaPuan.API
```

Swagger: http://localhost:5001 (Development ortamında)

## Testler

```bash
dotnet test HocaPuan.sln
```

## EF Core migration

```bash
cd hocapuan-backend
dotnet ef migrations add <Ad> \
  --project src/HocaPuan.Data \
  --startup-project src/HocaPuan.API

dotnet ef database update \
  --project src/HocaPuan.Data \
  --startup-project src/HocaPuan.API
```

## Mimari

```
HocaPuan.sln
├── src/HocaPuan.Core        → Entity, DTO, interface, validasyon
├── src/HocaPuan.Data        → DbContext, migration, seed
├── src/HocaPuan.Services    → İş mantığı, scraper, moderasyon
└── src/HocaPuan.API         → Controllers, middleware
```
