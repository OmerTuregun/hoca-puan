$ErrorActionPreference = "Stop"

$loginBody = @{
  email    = "admin@hocapuan.com"
  password = "Admin123!"
} | ConvertTo-Json -Compress

$token = (Invoke-RestMethod -Uri "http://localhost:5001/api/auth/login" `
  -Method Post `
  -ContentType "application/json" `
  -Body $loginBody).token

$headers = @{ Authorization = "Bearer $token" }

$importBody = @{
  universityIds     = @(142)
  maxPerUniversity  = 0
} | ConvertTo-Json -Compress

$r = Invoke-RestMethod -Uri "http://localhost:5001/api/import/yok/browser-import-professors" `
  -Method Post `
  -ContentType "application/json" `
  -Headers $headers `
  -Body $importBody

$r | ConvertTo-Json -Depth 6

