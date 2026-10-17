using AnFit.Api.Data;
using AnFit.Api.Models.Entities;
using AnFit.Api.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AnFit.Api.Repositories.Implementations
{
    public class UserRepository : Repository<User>, IUserRepository
    {
        // Gọi constructor của lớp cha (Repository<User>)
        public UserRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _dbSet.FirstOrDefaultAsync(u => u.Email == email && !u.IsDeleted);
        }

        public async Task<bool> IsEmailExistsAsync(string email)
        {
            return await _dbSet.AnyAsync(u => u.Email == email);
        }

        public async Task<IEnumerable<User>> GetActiveUsersAsync()
        {
            return await _dbSet.Where(u => !u.IsDeleted).ToListAsync();
        }

        public async Task SoftDeleteAsync(Guid userId)
        {
            var user = await _dbSet.FirstOrDefaultAsync(u => u.Id == userId);
            if (user != null)
            {
                user.IsDeleted = true;
                user.DeletedAt = DateTime.UtcNow;
                _dbSet.Update(user);
            }
        }

        public async Task<User?> GetWithDetailsAsync(Guid userId)
        {
            return await _dbSet
                .Include(u => u.Conditions)
                .Include(u => u.PreferredActivities)
                .FirstOrDefaultAsync(u => u.Id == userId);
        }
    }
}