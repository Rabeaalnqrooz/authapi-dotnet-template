using AuthApi.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace AuthApi.Infrastructure.Services
{
    public class PasswordHasher : IPasswordHasher
    {
        private const int WorkFactor = 12;

        public string Hash(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password, WorkFactor);
        }

        public bool Verify(string password, string passwordHash)
        {
           return BCrypt.Net.BCrypt.Verify(password, passwordHash);
        }
    }
}
