using AuthApi.Application.Interfaces;
using AuthApi.Application.Services;
using AuthApi.Infrastructure.Options;
using AuthApi.Infrastructure.Persistence;
using AuthApi.Infrastructure.Repositories;
using AuthApi.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AuthApi.Infrastructure;

/// <summary>
/// هذا الملف هو المكان الوحيد اللي بنسجل فيه كل خدمات الـ Infrastructure.
/// بنستدعيه من Program.cs في مشروع الـ API.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // ─────────────────────────────
        // 1. قاعدة البيانات
        // ─────────────────────────────
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection")));

        // ─────────────────────────────
        // 2. قراءة الإعدادات من appsettings.json
        // ─────────────────────────────
        services.Configure<EmailSettings>(
            configuration.GetSection("EmailSettings"));

        services.Configure<CloudinarySettings>(
            configuration.GetSection("CloudinarySettings"));

        // ─────────────────────────────
        // 3. تسجيل الـ Repositories والـ Services
        // ─────────────────────────────
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<ICloudinaryService, CloudinaryService>();
        // في نهاية دالة AddInfrastructure قبل return
        services.AddScoped<AuthService>();
        services.AddScoped<AdminService>();
        return services;
    }
}