using AuthApi.Application.Interfaces;
using AuthApi.Domain.Entities;
using AuthApi.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace AuthApi.Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _context;
        public UserRepository(AppDbContext context)
        {
            _context = context;
        }
        public async Task<User?> GetByIdAsync(Guid id)
        {
            return await _context.Users.FindAsync(id);
        }
        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Email == email.ToLower()); 
        }
        public async Task<IEnumerable<User>> GetAllAsync()
        {
            return await _context.Users.AsNoTracking().OrderByDescending(u =>u.CreatedAt).ToListAsync();
        }

        public async Task AddAsync(User user)
        {
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(User user)
        {
            user.UpdatedAt = DateTime.UtcNow; // نحدث التاريخ تلقائياً
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(User user)
        {
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> ExistsByEmailAsync(string email)
        {
            return await _context.Users
                .AnyAsync(u => u.Email == email.ToLower());
        }
        public async Task<User?> GetByEmailVerifyTokenAsync(string hashedToken)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.EmailVerifyToken == hashedToken);
        }
        public async Task<(IEnumerable<User> Users, int Total)> GetPagedAsync(int page, int limit)
        {
            if (page < 1) page = 1;
            if (limit < 1) limit = 10;
            if (limit > 100) limit = 100;

            var query = _context.Users.AsNoTracking().OrderByDescending(u => u.CreatedAt);

            var total = await query.CountAsync();

            var users = await query
                .Skip((page - 1) * limit)
                .Take(limit)
                .ToListAsync();

            return (users, total);
        }
    }
}
