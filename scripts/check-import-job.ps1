param(
  [Parameter(Mandatory=$true)]
  [string]$JobId
)

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

$r = Invoke-RestMethod -Uri "http://localhost:5001/api/import/job/$JobId" -Headers $headers
$r | ConvertTo-Json -Depth 10

