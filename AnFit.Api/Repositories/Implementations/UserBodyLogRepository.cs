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
    public class UserBodyLogRepository : Repository<UserBodyLog>, IUserBodyLogRepository
    {
        public UserBodyLogRepository(ApplicationDbContext context) : base(context) { }

        public async Task<UserBodyLog?> GetLatestByUserAsync(Guid userId)
        {
            return await _dbSet.Where(u => u.UserId == userId)
                               .OrderByDescending(u => u.RecordedAt)
                               .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<UserBodyLog>> GetHistoryAsync(Guid userId, int pageNumber, int pageSize)
        {
            return await _dbSet.Where(u => u.UserId == userId)
                               .OrderByDescending(u => u.RecordedAt)
                               .Skip((pageNumber - 1) * pageSize)
                               .Take(pageSize)
                               .ToListAsync();
        }
    }
}