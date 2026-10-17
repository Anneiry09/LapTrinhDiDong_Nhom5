using AnFit.Api.Models.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AnFit.Api.Repositories.Interfaces
{
    public interface IMedicalConditionRepository : IRepository<MedicalCondition>
    {
        Task<IEnumerable<MedicalCondition>> GetByTypeAsync(int type);
    }
}