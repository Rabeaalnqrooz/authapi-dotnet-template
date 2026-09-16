using System;
using System.Collections.Generic;
using System.Text;

namespace AuthApi.Application.Interfaces
{
    public interface IEmailService
    {
        Task SendEmailVerificationAsync(string toEmail, string firstName, string token);
        Task SendOtpAsync(string toEmail, string firstName, string otp);
    }
}
