using AuthApi.Application.Common;
using AuthApi.Application.DTOs;

using AuthApi.Application.Exceptions;
using AuthApi.Application.Interfaces;
using AuthApi.Domain.Entities;
using AuthApi.Domain.Enums;

namespace AuthApi.Application.Services;

public class AuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;
    private readonly IEmailService _emailService;
    private readonly ICloudinaryService _cloudinaryService;

    public AuthService(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        ITokenService tokenService,
        IEmailService emailService,
        ICloudinaryService cloudinaryService)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
        _emailService = emailService;
        _cloudinaryService = cloudinaryService;
    }

    // =========================================================
    // 1. التسجيل (Register)
    // =========================================================
    public async Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request)
    {
        if (await _userRepository.ExistsByEmailAsync(request.Email))
            throw new BadRequestException("البريد الإلكتروني مستخدم بالفعل");

        var rawToken = SecurityHelper.GenerateRandomToken();
        var hashedToken = SecurityHelper.HashToken(rawToken);

        var user = new User
        {
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            Email = request.Email.ToLower().Trim(),
            PasswordHash = _passwordHasher.Hash(request.Password),
            EmailVerifyToken = hashedToken,
            EmailVerifyTokenExpiry = DateTime.UtcNow.AddHours(24),
            Role = UserRole.User,
            IsVerified = false
        };

        await _userRepository.AddAsync(user);

        await _emailService.SendEmailVerificationAsync(
            user.Email,
            user.FirstName,
            rawToken);

        return MapToAuthResponse(user);
    }

    // =========================================================
    // 2. التحقق من الإيميل
    // =========================================================
    public async Task VerifyEmailAsync(string rawToken)
    {
        var hashedToken = SecurityHelper.HashToken(rawToken);

        var user = await _userRepository.GetByEmailVerifyTokenAsync(hashedToken);

        if (user == null)
            throw new BadRequestException("رابط التحقق غير صالح");

        if (user.EmailVerifyTokenExpiry < DateTime.UtcNow)
            throw new BadRequestException("رابط التحقق منتهي الصلاحية");

        user.IsVerified = true;
        user.EmailVerifyToken = null;
        user.EmailVerifyTokenExpiry = null;

        await _userRepository.UpdateAsync(user);
    }

    // =========================================================
    // 3. تسجيل الدخول (Login)
    // =========================================================
    public async Task<(AuthResponseDto User, string AccessToken, string RefreshToken)> LoginAsync(LoginRequestDto request)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email);

        if (user == null)
            throw new UnauthorizedException("بيانات الدخول غير صحيحة");

        if (!_passwordHasher.Verify(request.Password, user.PasswordHash))
            throw new UnauthorizedException("بيانات الدخول غير صحيحة");

        if (user.Role == UserRole.Disabled)
            throw new UnauthorizedException("هذا الحساب معطل");

        if (!user.IsVerified)
            throw new BadRequestException("يرجى تأكيد بريدك الإلكتروني أولاً");

        var accessToken = _tokenService.GenerateAccessToken(user);
        var refreshToken = _tokenService.GenerateRefreshToken(user);

        return (MapToAuthResponse(user), accessToken, refreshToken);
    }

    // =========================================================
    // 4. تجديد الـ Access Token
    // =========================================================
    public async Task<string> RefreshTokenAsync(string refreshToken)
    {
        var userId = _tokenService.ValidateRefreshToken(refreshToken);

        if (userId == null)
            throw new UnauthorizedException("جلسة غير صالحة، يرجى تسجيل الدخول مجدداً");

        var user = await _userRepository.GetByIdAsync(userId.Value);

        if (user == null || user.Role == UserRole.Disabled)
            throw new UnauthorizedException("الحساب غير موجود أو معطل");

        return _tokenService.GenerateAccessToken(user);
    }

    // =========================================================
    // 5. نسيت كلمة المرور (إرسال OTP)
    // =========================================================
    public async Task ForgotPasswordAsync(ForgotPasswordRequestDto request)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email);

        // للأمان: ما نخبرش المستخدم إن الإيميل موجود أو لا
        if (user == null)
            return;

        var rawOtp = SecurityHelper.GenerateOtp();
        var hashedOtp = SecurityHelper.HashToken(rawOtp);

        user.Otp = hashedOtp;
        user.OtpExpiry = DateTime.UtcNow.AddMinutes(10);

        await _userRepository.UpdateAsync(user);

        await _emailService.SendOtpAsync(user.Email, user.FirstName, rawOtp);
    }

    // =========================================================
    // 6. التحقق من الـ OTP
    // =========================================================
    public async Task VerifyResetOtpAsync(VerifyOtpRequestDto request)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email);

        if (user == null)
            throw new BadRequestException("طلب غير صالح");

        var hashedOtp = SecurityHelper.HashToken(request.Otp);

        if (user.Otp != hashedOtp || user.OtpExpiry < DateTime.UtcNow)
            throw new BadRequestException("رمز التحقق غير صحيح أو منتهي الصلاحية");
    }

    // =========================================================
    // 7. إعادة تعيين كلمة المرور
    // =========================================================
    public async Task ResetPasswordAsync(ResetPasswordRequestDto request)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email);

        if (user == null)
            throw new BadRequestException("طلب غير صالح");

        var hashedOtp = SecurityHelper.HashToken(request.Otp);

        if (user.Otp != hashedOtp || user.OtpExpiry < DateTime.UtcNow)
            throw new BadRequestException("رمز التحقق غير صحيح أو منتهي الصلاحية");

        user.PasswordHash = _passwordHasher.Hash(request.NewPassword);
        user.Otp = null;
        user.OtpExpiry = null;

        await _userRepository.UpdateAsync(user);
    }

    // =========================================================
    // 8. جلب بيانات المستخدم الحالي (GetMe)
    // =========================================================
    public async Task<UserDto> GetMeAsync(Guid userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);

        if (user == null)
            throw new NotFoundException("المستخدم غير موجود");

        return MapToUserDto(user);
    }

    // =========================================================
    // 9. تحديث كلمة المرور
    // =========================================================
    public async Task UpdatePasswordAsync(Guid userId, UpdatePasswordRequestDto request)
    {
        var user = await _userRepository.GetByIdAsync(userId);

        if (user == null)
            throw new NotFoundException("المستخدم غير موجود");

        if (!_passwordHasher.Verify(request.CurrentPassword, user.PasswordHash))
            throw new BadRequestException("كلمة المرور الحالية غير صحيحة");

        user.PasswordHash = _passwordHasher.Hash(request.NewPassword);
        await _userRepository.UpdateAsync(user);
    }

    // =========================================================
    // 10. تحديث الملف الشخصي (مع الصورة)
    // =========================================================
    public async Task<UserDto> UpdateProfileAsync(
        Guid userId,
        UpdateProfileRequestDto request,
        Stream? imageStream = null,
        string? imageFileName = null)
    {
        var user = await _userRepository.GetByIdAsync(userId);

        if (user == null)
            throw new NotFoundException("المستخدم غير موجود");

        if (!string.IsNullOrWhiteSpace(request.FirstName))
            user.FirstName = request.FirstName.Trim();

        if (!string.IsNullOrWhiteSpace(request.LastName))
            user.LastName = request.LastName.Trim();

        if (request.Address != null)
            user.Address = request.Address.Trim();

        if (request.City != null)
            user.City = request.City.Trim();

        if (request.ZipCode != null)
            user.ZipCode = request.ZipCode.Trim();

        if (request.PhoneNo != null)
            user.PhoneNo = request.PhoneNo.Trim();

        if (imageStream != null && !string.IsNullOrEmpty(imageFileName))
        {
            var (url, publicId) = await _cloudinaryService.UploadImageAsync(imageStream, imageFileName);

            await _cloudinaryService.DeleteImageAsync(user.ProfilePublicId);

            user.ProfilePic = url;
            user.ProfilePublicId = publicId;
        }

        await _userRepository.UpdateAsync(user);

        return MapToUserDto(user);
    }

    // =========================================================
    // دوال مساعدة
    // =========================================================
    private static AuthResponseDto MapToAuthResponse(User user)
    {
        return new AuthResponseDto
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            Role = user.Role.ToString().ToLower(),
            IsVerified = user.IsVerified,
            ProfilePic = user.ProfilePic
        };
    }

    private static UserDto MapToUserDto(User user)
    {
        return new UserDto
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            Role = user.Role.ToString().ToLower(),
            IsVerified = user.IsVerified,
            ProfilePic = user.ProfilePic,
            Address = user.Address,
            City = user.City,
            ZipCode = user.ZipCode,
            PhoneNo = user.PhoneNo,
            CreatedAt = user.CreatedAt
        };
    }
    // =========================================================
    // إعادة إرسال OTP لإعادة تعيين كلمة المرور
    // =========================================================
    public async Task ResendResetOtpAsync(ForgotPasswordRequestDto request)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email);

        // للأمان: ما نخبرش المستخدم إن الإيميل موجود أو لا
        if (user == null)
            return;

        var rawOtp = SecurityHelper.GenerateOtp();
        var hashedOtp = SecurityHelper.HashToken(rawOtp);

        user.Otp = hashedOtp;
        user.OtpExpiry = DateTime.UtcNow.AddMinutes(10);

        await _userRepository.UpdateAsync(user);

        await _emailService.SendOtpAsync(user.Email, user.FirstName, rawOtp);
    }
}