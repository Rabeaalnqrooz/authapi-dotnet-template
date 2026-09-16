using System;
using System.Collections.Generic;
using System.Text;

namespace AuthApi.Application.DTOs
{
    public class UpdateUserRoleRequestDto
    {
        public string Role { get; set; } = string.Empty;
    }
}
