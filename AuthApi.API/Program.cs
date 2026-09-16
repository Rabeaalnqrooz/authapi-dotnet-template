using AuthApi.API.Middleware;
using AuthApi.Application.Services;
using AuthApi.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;

using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace AuthApi.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // =========================================================
            // 1. إضافة الخدمات
            // =========================================================
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();

            // =========================================================
            // 2. تسجيل طبقة الـ Infrastructure والـ Application
            // =========================================================
            builder.Services.AddInfrastructure(builder.Configuration);

            builder.Services.AddScoped<AuthService>();
            builder.Services.AddScoped<AdminService>();

            // =========================================================
            // 3. إعداد الـ JWT Authentication
            // =========================================================
            var jwtSettings = builder.Configuration.GetSection("Jwt");

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                // مهم: نخلي أسماء الـ Claims كما هي (sub, role...)
                options.MapInboundClaims = false;

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings["Issuer"],
                    ValidAudience = jwtSettings["Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwtSettings["AccessSecret"]!)),
                    ClockSkew = TimeSpan.Zero,

                    // عشان [Authorize(Roles = "Admin")] يشتغل
                    RoleClaimType = ClaimTypes.Role,
                    // عشان نقدر نقرأ userId من sub
                    NameClaimType = JwtRegisteredClaimNames.Sub
                };

                // قراءة التوكن من الكوكي
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        if (context.Request.Cookies.TryGetValue("accessToken", out var token))
                        {
                            context.Token = token;
                        }
                        return Task.CompletedTask;
                    }
                };
            });

            builder.Services.AddAuthorization();

            // =========================================================
            // 4. CORS
            // =========================================================
            var frontendUrl = builder.Configuration["FrontendUrl"] ?? "http://localhost:5173";

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowFrontend", policy =>
                {
                    policy.WithOrigins(frontendUrl)
                          .AllowAnyHeader()
                          .AllowAnyMethod()
                          .AllowCredentials(); // ضروري للكوكيز
                });
            });
            // =========================================================
            // Rate Limiting
            // =========================================================
            builder.Services.AddRateLimiter(options =>
            {
                options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

                // حد عام خفيف
                options.AddFixedWindowLimiter("general", opt =>
                {
                    opt.PermitLimit = 100;
                    opt.Window = TimeSpan.FromMinutes(1);
                    opt.QueueLimit = 0;
                });

                // Login: 5 محاولات كل دقيقة لكل IP
                options.AddFixedWindowLimiter("login", opt =>
                {
                    opt.PermitLimit = 5;
                    opt.Window = TimeSpan.FromMinutes(1);
                    opt.QueueLimit = 0;
                });

                // Forgot / OTP / Resend: 3 طلبات كل دقيقة
                options.AddFixedWindowLimiter("otp", opt =>
                {
                    opt.PermitLimit = 3;
                    opt.Window = TimeSpan.FromMinutes(1);
                    opt.QueueLimit = 0;
                });

                options.OnRejected = async (context, token) =>
                {
                    context.HttpContext.Response.ContentType = "application/json";
                    await context.HttpContext.Response.WriteAsync(
                        """{"success":false,"message":"محاولات كثيرة، حاول بعد دقيقة"}""",
                        token);
                };
            });

            var app = builder.Build();

            // =========================================================
            // 5. الـ Middleware Pipeline
            // =========================================================
            app.UseMiddleware<ExceptionMiddleware>();

            // في التطوير: لو حابب تتجنب تحويل HTTP→HTTPS داخل الـ API نفسها
            // خليه فقط في الإنتاج
            if (!app.Environment.IsDevelopment())
            {
                app.UseHttpsRedirection();
            }

            // CORS لازم قبل Authentication
            app.UseCors("AllowFrontend");
            app.UseRateLimiter();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}