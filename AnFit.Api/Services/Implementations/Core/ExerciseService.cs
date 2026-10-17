using AnFit.Api.DTOs.Requests;
using AnFit.Api.DTOs.Responses;
using AnFit.Api.Exceptions;
using AnFit.Api.Extensions;
using AnFit.Api.Models.Entities;
using AnFit.Api.Repositories.Interfaces;
using AnFit.Api.Services.Interfaces;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace AnFit.Api.Services.Implementations.Core
{
    public class ExerciseService : IExerciseService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ExerciseService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<PagedResponse<System.Collections.Generic.IEnumerable<ExerciseResponse>>> GetAllAsync(ExerciseFilterQuery query)
        {
            var exercises = await _unitOfWork.Exercises.GetAllAsync();

            // 1. Lọc theo SearchTerm
            if (!string.IsNullOrWhiteSpace(query.SearchTerm))
            {
                exercises = exercises.Where(e => e.Name != null && e.Name.Contains(query.SearchTerm, StringComparison.OrdinalIgnoreCase));
            }

            // 2. Lọc theo Nhóm cơ (MuscleGroup)
            if (!string.IsNullOrWhiteSpace(query.MuscleGroup))
            {
                exercises = exercises.Where(e => e.MuscleGroup != null && e.MuscleGroup.Contains(query.MuscleGroup, StringComparison.OrdinalIgnoreCase));
            }

            // 3. Phân trang
            var totalRecords = exercises.Count();
            var totalPages = (int)Math.Ceiling(totalRecords / (double)query.PageSize);

            var pagedData = exercises.Skip((query.PageNumber - 1) * query.PageSize)
                                     .Take(query.PageSize)
                                     .Select(e => e.ToResponse())
                                     .ToList();

            return new PagedResponse<System.Collections.Generic.IEnumerable<ExerciseResponse>>(
                pagedData, query.PageNumber, query.PageSize, totalRecords, totalPages);
        }

        public async Task<ExerciseResponse?> GetByIdAsync(int id)
        {
            var exercise = await _unitOfWork.Exercises.GetByIdAsync(id);
            if (exercise == null) throw new NotFoundException("Không tìm thấy bài tập.");
            return exercise.ToResponse();
        }

        public async Task<ExerciseResponse> CreateAsync(CreateExerciseRequest request)
        {
            var newExercise = new Exercise
            {
                Name = request.Name,
                MuscleGroup = request.MuscleGroup,
                BaseCaloriesBurned = request.BaseCaloriesBurned,
                InstructionUrl = request.InstructionUrl,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Exercises.AddAsync(newExercise);
            await _unitOfWork.SaveChangesAsync();

            return newExercise.ToResponse();
        }

        public async Task<ExerciseResponse> UpdateAsync(int id, UpdateExerciseRequest request)
        {
            var exercise = await _unitOfWork.Exercises.GetByIdAsync(id);
            if (exercise == null) throw new NotFoundException("Không tìm thấy bài tập.");

            if (request.Name != null) exercise.Name = request.Name;
            if (request.MuscleGroup != null) exercise.MuscleGroup = request.MuscleGroup;
            if (request.BaseCaloriesBurned.HasValue) exercise.BaseCaloriesBurned = request.BaseCaloriesBurned;
            if (request.InstructionUrl != null) exercise.InstructionUrl = request.InstructionUrl;

            exercise.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Exercises.Update(exercise);
            await _unitOfWork.SaveChangesAsync();

            return exercise.ToResponse();
        }

        public async Task DeleteAsync(int id)
        {
            var exercise = await _unitOfWork.Exercises.GetByIdAsync(id);
            if (exercise == null) throw new NotFoundException("Không tìm thấy bài tập.");

            _unitOfWork.Exercises.Remove(exercise);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}