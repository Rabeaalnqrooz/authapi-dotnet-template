using AuthApi.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace AuthApi.Domain.Entities
{
    public class User
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string ProfilePic { get; set; } =
            "https://res.cloudinary.com/dfkc2tojy/image/upload/v12345678/defaults/default-avatar.webp";
        public string ProfilePublicId { get; set; } = "defaults/default-avatar";
        public UserRole Role { get; set; } = UserRole.User;
        public bool IsVerified { get; set; } = false;
        public string? EmailVerifyToken { get; set; }
        public DateTime? EmailVerifyTokenExpiry { get; set; }
        public string? Otp { get; set; }

        public DateTime? OtpExpiry { get; set; }
        public string Address { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string ZipCode { get; set; } = string.Empty;
        public string PhoneNo { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;




    }
}
