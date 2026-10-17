using AnFit.Api.Data;
using AnFit.Api.Models.Entities;
using AnFit.Api.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AnFit.Api.Repositories.Implementations
{
    public class MedicalConditionRepository : Repository<MedicalCondition>, IMedicalConditionRepository
    {
        public MedicalConditionRepository(ApplicationDbContext context) : base(context) { }

        public async Task<IEnumerable<MedicalCondition>> GetByTypeAsync(int type)
        {
            return await _dbSet.Where(m => m.Type == type).ToListAsync();
        }
    }
}