using AuthApi.Application.Interfaces;
using AuthApi.Infrastructure.Options;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace AuthApi.Infrastructure.Services;

/// <summary>
/// تنفيذ إرسال الإيميلات باستخدام MailKit.
/// </summary>
public class EmailService : IEmailService
{
    private readonly EmailSettings _settings;

    // IOptions<T> هي الطريقة الرسمية في ASP.NET Core لقراءة الإعدادات
    public EmailService(IOptions<EmailSettings> options)
    {
        _settings = options.Value;
    }

    public async Task SendEmailVerificationAsync(string toEmail, string firstName, string token)
    {
        // رابط التحقق (هنعدله لاحقاً حسب الـ Frontend)
        var frontendUrl = "http://localhost:5173"; // أو اقرأه من الإعدادات
        var verificationLink = $"{frontendUrl}/verify/{token}";

        var subject = "تأكيد البريد الإلكتروني";
        var body = $@"
            <h2>مرحباً {firstName} 👋</h2>
            <p>شكراً لتسجيلك معنا.</p>
            <p>اضغط على الزر التالي لتأكيد بريدك الإلكتروني:</p>
            <a href='{verificationLink}' 
               style='padding:10px 20px; background:#4CAF50; color:white; text-decoration:none; border-radius:5px;'>
               تأكيد الإيميل
            </a>
            <p>الرابط صالح لمدة 24 ساعة.</p>
        ";

        await SendEmailAsync(toEmail, subject, body);
    }

    public async Task SendOtpAsync(string toEmail, string firstName, string otp)
    {
        var subject = "رمز إعادة تعيين كلمة المرور";
        var body = $@"
            <h2>مرحباً {firstName}</h2>
            <p>رمز التحقق الخاص بك هو:</p>
            <h1 style='letter-spacing: 8px; color: #333;'>{otp}</h1>
            <p>الرمز صالح لمدة 10 دقائق فقط.</p>
            <p>إذا لم تطلب هذا الرمز، تجاهل هذه الرسالة.</p>
        ";

        await SendEmailAsync(toEmail, subject, body);
    }

    /// <summary>
    /// الدالة الداخلية المشتركة لإرسال أي إيميل.
    /// </summary>
    private async Task SendEmailAsync(string toEmail, string subject, string htmlBody)
    {
        var message = new MimeMessage();

        // المرسل
        message.From.Add(new MailboxAddress(_settings.SenderName, _settings.SenderEmail));

        // المستقبل
        message.To.Add(MailboxAddress.Parse(toEmail));

        message.Subject = subject;

        // محتوى الإيميل (HTML)
        message.Body = new TextPart("html")
        {
            Text = htmlBody
        };

        using var client = new SmtpClient();

        // الاتصال بالسيرفر
        await client.ConnectAsync(
            _settings.SmtpServer,
            _settings.SmtpPort,
            SecureSocketOptions.StartTls);

        // تسجيل الدخول
        await client.AuthenticateAsync(_settings.Username, _settings.Password);

        // إرسال
        await client.SendAsync(message);

        // إغلاق الاتصال بشكل نظيف
        await client.DisconnectAsync(true);
    }
}