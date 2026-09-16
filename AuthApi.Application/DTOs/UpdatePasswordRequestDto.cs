using System;
using System.Collections.Generic;
using System.Text;

namespace AuthApi.Application.DTOs
{
    public class UpdatePasswordRequestDto
    {

        public string CurrentPassword { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
    }
}
