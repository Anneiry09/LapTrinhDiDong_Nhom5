using AnFit.Api.Data;
using AnFit.Api.Models.Entities;
using AnFit.Api.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AnFit.Api.Repositories.Implementations
{
    public class ExerciseRepository : Repository<Exercise>, IExerciseRepository
    {
        public ExerciseRepository(ApplicationDbContext context) : base(context) { }

        public async Task<IEnumerable<Exercise>> GetByMuscleGroupAsync(string muscleGroup)
        {
            // Kiểm tra null trước khi gọi Contains vì MuscleGroup là nullable
            return await _dbSet.Where(e => e.MuscleGroup != null && e.MuscleGroup.Contains(muscleGroup))
                               .ToListAsync();
        }

        public async Task<IEnumerable<Exercise>> SearchByNameAsync(string searchTerm)
        {
            // Kiểm tra null trước khi gọi Contains vì Name là nullable
            return await _dbSet.Where(e => e.Name != null && e.Name.Contains(searchTerm))
                               .ToListAsync();
        }
    }
}