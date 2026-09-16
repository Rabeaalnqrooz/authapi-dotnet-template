using System;
using System.Collections.Generic;
using System.Text;

namespace AuthApi.Application.DTOs
{
    public class ForgotPasswordRequestDto
    {
        public string Email { get; set; } = string.Empty;
    }
}
