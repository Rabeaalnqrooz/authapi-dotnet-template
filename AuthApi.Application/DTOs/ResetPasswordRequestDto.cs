using System;
using System.Collections.Generic;
using System.Text;

namespace AuthApi.Application.DTOs
{
    public class ResetPasswordRequestDto
    {
        public string Email { get; set; } = string.Empty;
        public string Otp { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
    }
}
