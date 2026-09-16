using System.Security.Cryptography;
using System.Text;

namespace AuthApi.Application.Common;

/// <summary>
/// دوال مساعدة للأمان (توليد توكنات وتشفيرها).
/// </summary>
public static class SecurityHelper
{
    /// <summary>
    /// يولد توكن عشوائي قوي (لتحقق الإيميل).
    /// </summary>
    public static string GenerateRandomToken(int bytes = 32)
    {
        var randomBytes = new byte[bytes];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToHexString(randomBytes).ToLower(); // مثال: a3f8b2c1...
    }

    /// <summary>
    /// يشفر أي نص باستخدام SHA256 (نفس فكرة crypto.createHash في Node).
    /// </summary>
    public static string HashToken(string rawToken)
    {
        using var sha = SHA256.Create();
        var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(rawToken));
        return Convert.ToHexString(bytes).ToLower();
    }

    /// <summary>
    /// يولد OTP من 6 أرقام.
    /// </summary>
    public static string GenerateOtp()
    {
        // من 100000 إلى 999999
        return RandomNumberGenerator.GetInt32(100000, 1000000).ToString();
    }
}