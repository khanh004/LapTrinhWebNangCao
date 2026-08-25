#requires -Version 5.1
$ErrorActionPreference = "Stop"

Write-Host "=== SchoolApi - Tuan 3 / PostgreSQL ===" -ForegroundColor Cyan

dotnet restore
dotnet build

Write-Host "`n[1] Nap PostgreSQL connection string vao User Secrets"
dotnet user-secrets set "ConnectionStrings:SchoolDb" "Host=localhost;Port=5432;Database=student_api_lab;Username=postgres;Password=YOUR_POSTGRES_PASSWORD"

Write-Host "`n[2] Tao secrets cho JWT"
dotnet user-secrets set "Seed:Password" "Lab#2026_ChangeMe"
dotnet user-secrets set "Jwt:Issuer" "SchoolApi"
dotnet user-secrets set "Jwt:Audience" "SchoolApiClients"
dotnet user-secrets set "Jwt:SigningKey" "SchoolApiJwtKey_2026_Change_This_To_Your_Own_32Byte_Key_12345"
dotnet user-secrets set "Jwt:AccessTokenMinutes" "30"

Write-Host "`n[3] Xoa migration MySQL cu neu ban dang dung PostgreSQL database moi"
if (Test-Path ".\Migrations") {
    Remove-Item ".\Migrations" -Recurse -Force
}
New-Item ".\Migrations" -ItemType Directory | Out-Null

Write-Host "`n[4] Tao migration PostgreSQL"
dotnet ef migrations add InitialPostgresSchoolSchema
dotnet ef migrations add AddIdentityAndRoles

Write-Host "`n[5] Ap dung migration"
dotnet ef database update

Write-Host "`nHoan tat. Chay: dotnet run"
