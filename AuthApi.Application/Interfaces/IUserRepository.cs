using AuthApi.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace AuthApi.Application.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> GetByIdAsync(Guid id);
        Task<User?> GetByEmailAsync(string email);
        Task<IEnumerable<User>> GetAllAsync();
        Task AddAsync(User user);
        Task UpdateAsync(User user);
        Task DeleteAsync(User user);
        Task<bool> ExistsByEmailAsync(string email);
        Task<User?> GetByEmailVerifyTokenAsync(string hashedToken);
        Task<(IEnumerable<User> Users, int Total)> GetPagedAsync(int page, int limit);
    }
}
