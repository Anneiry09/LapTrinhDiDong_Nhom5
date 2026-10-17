using AnFit.Api.Data;
using AnFit.Api.Models.Entities;
using AnFit.Api.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace AnFit.Api.Repositories.Implementations
{
    public class UserIntegrationRepository : Repository<UserIntegration>, IUserIntegrationRepository
    {
        public UserIntegrationRepository(ApplicationDbContext context) : base(context) { }

        public async Task<UserIntegration?> GetByUserAndProviderAsync(Guid userId, int provider)
        {
            return await _dbSet.FirstOrDefaultAsync(u => u.UserId == userId && u.Provider == provider);
        }
    }
}