using AnFit.Api.DTOs.Responses;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AnFit.Api.Services.Interfaces
{
    public interface IMedicalConditionService
    {
        Task<IEnumerable<MedicalConditionResponse>> GetAllAsync();
        Task<MedicalConditionResponse?> GetByIdAsync(int id);
        Task<IEnumerable<MedicalConditionResponse>> GetByTypeAsync(int type);
        Task<MedicalConditionResponse> CreateAsync(string conditionName, int type);
        Task UpdateAsync(int id, string conditionName, int type);
        Task DeleteAsync(int id);
    }
}