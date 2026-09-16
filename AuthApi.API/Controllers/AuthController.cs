using AuthApi.Application.DTOs;
using AuthApi.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;

namespace AuthApi.API.Controllers;

[ApiController]
[Route("api/v1/user")]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;

    public AuthController(AuthService authService)
    {
        _authService = authService;
    }

    // =========================================================
    // إعدادات الكوكي — SameSite=None + Secure=true
    // ضروري لأن Frontend (http://localhost:5173)
    // و Backend (https://localhost:xxxx) Cross-Site
    // =========================================================
    private static CookieOptions AccessCookieOptions() => new()
    {
        HttpOnly = true,
        Secure = true,                 // إجباري مع SameSite=None
        SameSite = SameSiteMode.None,  // يسمح بإرسال الكوكي عبر المواقع
        Path = "/",
        Expires = DateTime.UtcNow.AddMinutes(15)
    };

    private static CookieOptions RefreshCookieOptions() => new()
    {
        HttpOnly = true,
        Secure = true,
        SameSite = SameSiteMode.None,
        Path = "/",
        Expires = DateTime.UtcNow.AddDays(7)
    };

    private static CookieOptions DeleteCookieOptions() => new()
    {
        HttpOnly = true,
        Secure = true,
        SameSite = SameSiteMode.None,
        Path = "/"
    };

    // =========================================================
    // التسجيل
    // =========================================================
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
    {
        try
        {
            var result = await _authService.RegisterAsync(request);
            return Ok(new { success = true, user = result });
        }
        catch (Exception ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    // =========================================================
    // التحقق من الإيميل
    // =========================================================
    [HttpGet("verify/{token}")]
    public async Task<IActionResult> VerifyEmail(string token)
    {
        try
        {
            await _authService.VerifyEmailAsync(token);
            return Ok(new { success = true, message = "تم تأكيد البريد الإلكتروني بنجاح" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    // =========================================================
    // تسجيل الدخول
    // =========================================================
    [EnableRateLimiting("login")]
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
    {
        try
        {
            var (user, accessToken, refreshToken) = await _authService.LoginAsync(request);

            Response.Cookies.Append("accessToken", accessToken, AccessCookieOptions());
            Response.Cookies.Append("refreshToken", refreshToken, RefreshCookieOptions());

            return Ok(new { success = true, user });
        }
        catch (Exception ex)
        {
            return Unauthorized(new { success = false, message = ex.Message });
        }
    }

    // =========================================================
    // تجديد التوكن
    // =========================================================
    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken()
    {
        try
        {
            var refreshToken = Request.Cookies["refreshToken"];

            if (string.IsNullOrEmpty(refreshToken))
                return Unauthorized(new { success = false, message = "انتهت الجلسة" });

            var newAccessToken = await _authService.RefreshTokenAsync(refreshToken);

            Response.Cookies.Append("accessToken", newAccessToken, AccessCookieOptions());

            return Ok(new { success = true, message = "تم تجديد الجلسة" });
        }
        catch (Exception ex)
        {
            return Unauthorized(new { success = false, message = ex.Message });
        }
    }

    // =========================================================
    // تسجيل الخروج
    // =========================================================
    [Authorize]
    [HttpPost("logout")]
    public IActionResult Logout()
    {
        Response.Cookies.Delete("accessToken", DeleteCookieOptions());
        Response.Cookies.Delete("refreshToken", DeleteCookieOptions());

        return Ok(new { success = true, message = "تم تسجيل الخروج بنجاح" });
    }

    // =========================================================
    // بيانات المستخدم الحالي
    // =========================================================
    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> GetMe()
    {
        try
        {
            var userId = GetCurrentUserId();
            var user = await _authService.GetMeAsync(userId);
            return Ok(new { success = true, user });
        }
        catch (Exception ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    // =========================================================
    // نسيت كلمة المرور
    // =========================================================
    [EnableRateLimiting("otp")]
    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequestDto request)
    {
        try
        {
            await _authService.ForgotPasswordAsync(request);
            return Ok(new { success = true, message = "إذا كان الإيميل موجوداً، سيتم إرسال رمز التحقق" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    // =========================================================
    // التحقق من OTP
    // =========================================================
    [EnableRateLimiting("otp")]
    [HttpPost("verify-reset-otp")]
    public async Task<IActionResult> VerifyResetOtp([FromBody] VerifyOtpRequestDto request)
    {
        try
        {
            await _authService.VerifyResetOtpAsync(request);
            return Ok(new { success = true, message = "رمز التحقق صحيح" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    // =========================================================
    // إعادة تعيين كلمة المرور
    // =========================================================
    [EnableRateLimiting("otp")]
    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequestDto request)
    {
        try
        {
            await _authService.ResetPasswordAsync(request);
            return Ok(new { success = true, message = "تم تغيير كلمة المرور بنجاح" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    // =========================================================
    // إعادة إرسال OTP
    // =========================================================
    [EnableRateLimiting("otp")]
    [HttpPost("resend-reset-otp")]
    public async Task<IActionResult> ResendResetOtp([FromBody] ForgotPasswordRequestDto request)
    {
        try
        {
            await _authService.ResendResetOtpAsync(request);
            return Ok(new
            {
                success = true,
                message = "إذا كان الإيميل موجوداً، سيتم إرسال رمز تحقق جديد"
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    // =========================================================
    // تحديث كلمة المرور
    // =========================================================
    [Authorize]
    [HttpPut("update-password")]
    public async Task<IActionResult> UpdatePassword([FromBody] UpdatePasswordRequestDto request)
    {
        try
        {
            var userId = GetCurrentUserId();
            await _authService.UpdatePasswordAsync(userId, request);
            return Ok(new { success = true, message = "تم تحديث كلمة المرور بنجاح" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    // =========================================================
    // تحديث الملف الشخصي (مع صورة)
    // =========================================================
    [Authorize]
    [HttpPut("update-profile")]
    public async Task<IActionResult> UpdateProfile(
        [FromForm] UpdateProfileRequestDto request,
        IFormFile? profilePic)
    {
        try
        {
            var userId = GetCurrentUserId();

            Stream? imageStream = null;
            string? fileName = null;

            if (profilePic != null && profilePic.Length > 0)
            {
                imageStream = profilePic.OpenReadStream();
                fileName = profilePic.FileName;
            }

            var updatedUser = await _authService.UpdateProfileAsync(
                userId, request, imageStream, fileName);

            return Ok(new { success = true, user = updatedUser });
        }
        catch (Exception ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    // =========================================================
    // استخراج الـ UserId من التوكن
    // =========================================================
    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                       ?? User.FindFirst("sub")?.Value;

        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            throw new Exception("مستخدم غير صالح");

        return userId;
    }
}