using AnFit.Api.DTOs.Requests;
using AnFit.Api.DTOs.Responses;
using System.Threading.Tasks;

namespace AnFit.Api.Services.Interfaces
{
    public interface IExerciseService
    {
        Task<PagedResponse<System.Collections.Generic.IEnumerable<ExerciseResponse>>> GetAllAsync(ExerciseFilterQuery query);
        Task<ExerciseResponse?> GetByIdAsync(int id);
        Task<ExerciseResponse> CreateAsync(CreateExerciseRequest request);
        Task<ExerciseResponse> UpdateAsync(int id, UpdateExerciseRequest request);
        Task DeleteAsync(int id);
    }
}