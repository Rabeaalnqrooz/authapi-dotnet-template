using AuthApi.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace AuthApi.Application.Interfaces
{
    public interface ITokenService
    {
        string GenerateAccessToken(User user);
        string GenerateRefreshToken(User user);
        Guid? ValidateRefreshToken(string token);
    }
}
