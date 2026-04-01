# ─── Build Stage ─────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Önce sadece csproj dosyalarını kopyala (layer cache için)
COPY src/HocaPuan.Core/HocaPuan.Core.csproj       src/HocaPuan.Core/
COPY src/HocaPuan.Data/HocaPuan.Data.csproj       src/HocaPuan.Data/
COPY src/HocaPuan.Services/HocaPuan.Services.csproj src/HocaPuan.Services/
COPY src/HocaPuan.API/HocaPuan.API.csproj         src/HocaPuan.API/

# Restore (cache'lenir)
RUN dotnet restore src/HocaPuan.API/HocaPuan.API.csproj

# Kaynak kodu kopyala ve build et
COPY . .
RUN dotnet publish src/HocaPuan.API/HocaPuan.API.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

# Playwright Chromium'u indir — runtime stage'e kopyalanacak
RUN dotnet tool install --global Microsoft.Playwright.CLI
ENV PLAYWRIGHT_BROWSERS_PATH=/ms-playwright
ENV PATH="$PATH:/root/.dotnet/tools"
RUN playwright install chromium

# ─── Runtime Stage ────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# Ubuntu Noble (24.04) Chromium sistem bağımlılıkları.
# Not: 24.04'te bazı paketler 't64' suffix aldı (libasound2t64 vb.)
RUN apt-get update && apt-get install -y --no-install-recommends \
    fonts-liberation \
    libasound2t64 \
    libatk-bridge2.0-0 \
    libatk1.0-0 \
    libcairo2 \
    libcups2t64 \
    libdbus-1-3 \
    libdrm2 \
    libgbm1 \
    libglib2.0-0t64 \
    libgtk-3-0t64 \
    libnss3 \
    libnspr4 \
    libpango-1.0-0 \
    libpangocairo-1.0-0 \
    libx11-6 \
    libx11-xcb1 \
    libxcb1 \
    libxcomposite1 \
    libxdamage1 \
    libxext6 \
    libxfixes3 \
    libxrandr2 \
    libxrender1 \
    libxss1 \
    libxtst6 \
    && rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish .
COPY --from=build /ms-playwright /ms-playwright

ENV PLAYWRIGHT_BROWSERS_PATH=/ms-playwright

# Playwright'ın paket içindeki Node binary'sine execute yetkisi ver
RUN find /app/.playwright -name "node" -type f -exec chmod +x {} \; && \
    find /ms-playwright -name "chrome" -o -name "chrome-headless-shell" -o -name "ffmpeg" \
        | xargs -r chmod +x

# .NET 10 imajı dahili 'app' kullanıcısını içeriyor
USER app

EXPOSE 8080
ENTRYPOINT ["dotnet", "HocaPuan.API.dll"]
