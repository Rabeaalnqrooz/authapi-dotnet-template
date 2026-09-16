using System;
using System.Collections.Generic;
using System.Text;

namespace AuthApi.Application.DTOs
{
    public class UpdateProfileRequestDto
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? ZipCode { get; set; }
        public string? PhoneNo { get; set; }
    }
}
