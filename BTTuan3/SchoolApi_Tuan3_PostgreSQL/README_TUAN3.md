# SchoolApi - Tuan 3 (PostgreSQL + JWT + RBAC)

Project nay duoc noi tiep truc tiep tu SchoolApi Tuan 2.

## Da thay doi

- MySQL/Pomelo -> PostgreSQL/Npgsql
- Them AppUser, Role, UserRole
- Seed Admin / Staff / Student
- JWT Bearer
- Role-based authorization
- Student owner policy chong BOLA
- CORS allowlist
- Rate limit login: 5 request/phut/IP
- Security headers
- Swagger Bearer Authorize
- SchoolApi.http voi 4 test bat buoc

## 1. Tao PostgreSQL

Trong pgAdmin 4 tao database:

`student_api_lab`

## 2. Cau hinh User Secrets

Chay trong thu muc SchoolApi:

```powershell
dotnet user-secrets set "ConnectionStrings:SchoolDb" "Host=localhost;Port=5432;Database=student_api_lab;Username=postgres;Password=MAT_KHAU_POSTGRES"
dotnet user-secrets set "Seed:Password" "Lab#2026_ChangeMe"
dotnet user-secrets set "Jwt:Issuer" "SchoolApi"
dotnet user-secrets set "Jwt:Audience" "SchoolApiClients"
dotnet user-secrets set "Jwt:SigningKey" "TAO_MOT_KEY_RIENG_DAI_IT_NHAT_32_KY_TU"
dotnet user-secrets set "Jwt:AccessTokenMinutes" "30"
```

Khong commit cac secret nay len Git.

## 3. Tao migration PostgreSQL

Neu database PostgreSQL nay la database moi:

```powershell
Remove-Item .\Migrations -Recurse -Force
dotnet ef migrations add InitialPostgresSchoolSchema
dotnet ef migrations add AddIdentityAndRoles
dotnet ef database update
```

## 4. Chay

```powershell
dotnet run
```

Swagger: `https://localhost:7155/swagger`

## 5. Tai khoan lab

- `admin@hnmu.edu.vn`
- `staff@hnmu.edu.vn`
- `sv001@hnmu.edu.vn`

Mat khau seed mac dinh trong lab: `Lab#2026_ChangeMe`

Neu Student trong database chua co, hay tao mot Student bang Admin/Staff. Sau do khoi dong lai app de seeder tu dong gan student dau tien cho `sv001@hnmu.edu.vn`.

## 6. Quyen

- Student: GET/PUT chi ho so cua minh; khong duoc GET toan bo, POST, DELETE.
- Staff: GET toan bo, tao, sua; khong DELETE.
- Admin: GET, tao, sua, DELETE.
- Programme/Course: yeu cau dang nhap.
- Login: public.

## 7. Test am

Mo `SchoolApi.http` va kiem tra:

- khong token -> 401
- Student DELETE -> 403
- Student sua student khac -> 403
- spam login lan thu 6 trong 1 phut -> 429
