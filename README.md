# AuthApi - ASP.NET Core 10 Clean Architecture Template

Template لمشروع مصادقة كامل باستخدام Clean Architecture.

## المميزات

- Register / Login / Logout
- JWT عبر HttpOnly Cookies + Refresh Token
- تأكيد البريد الإلكتروني
- نسيت كلمة المرور + OTP
- الملف الشخصي + رفع الصور (Cloudinary)
- لوحة أدمن (أدوار + Pagination)
- Rate Limiting

## هيكل المشروع

- `AuthApi.API` — Controllers + Middleware
- `AuthApi.Application` — Services + DTOs
- `AuthApi.Domain` — Entities + Enums
- `AuthApi.Infrastructure` — EF Core + External services

## التشغيل

1. انسخ الإعدادات إلى `appsettings.Development.json` أو استخدم User Secrets
2. طبّق الـ Migrations:

```bash
dotnet ef database update --project AuthApi.Infrastructure --startup-project AuthApi.API