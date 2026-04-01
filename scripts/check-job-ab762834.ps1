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

Invoke-RestMethod -Uri "http://localhost:5001/api/import/job/ab762834" `
  -Method Get `
  -Headers $headers `
| ConvertTo-Json -Depth 10

