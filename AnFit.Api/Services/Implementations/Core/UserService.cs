using AnFit.Api.DTOs.Requests;
using AnFit.Api.DTOs.Responses;
using AnFit.Api.Exceptions;
using AnFit.Api.Extensions;
using AnFit.Api.Repositories.Interfaces;
using AnFit.Api.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AnFit.Api.Services.Implementations.Core
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;

        public UserService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<UserProfileResponse> GetProfileAsync(Guid userId)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null || user.DeletedAt != null) throw new NotFoundException("Không tìm thấy người dùng.");

            return user.ToResponse();
        }

        public async Task<UserProfileResponse> UpdateProfileAsync(Guid userId, UpdateProfileRequest request)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null || user.DeletedAt != null) throw new NotFoundException("Không tìm thấy người dùng.");

            user.FullName = request.FullName;
            user.Gender = request.Gender;
            user.HeightCm = request.HeightCm;
            user.DateOfBirth = request.DateOfBirth;
            user.TargetGoal = request.TargetGoal;
            user.DailyBudget = request.DailyBudget;
            user.IsCompensateExercise = request.IsCompensateExercise;
            user.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Users.Update(user);
            await _unitOfWork.SaveChangesAsync();

            return user.ToResponse();
        }

        public async Task UpdateMedicalConditionsAsync(Guid userId, UpdateMedicalConditionsRequest request)
        {
            var currentUser = await _unitOfWork.Users.GetWithDetailsAsync(userId);
            if (currentUser == null || currentUser.DeletedAt != null) throw new NotFoundException("Không tìm thấy người dùng.");

            currentUser.Conditions.Clear(); // Xóa quan hệ cũ (vì đã Include nên EF Core sẽ tự động xóa trong bảng trung gian)

            // Tìm và add các bệnh lý mới dựa trên ID truyền vào
            foreach (var conditionId in request.ConditionIds)
            {
                var condition = await _unitOfWork.MedicalConditions.GetByIdAsync(conditionId);
                if (condition != null)
                {
                    currentUser.Conditions.Add(condition);
                }
            }

            _unitOfWork.Users.Update(currentUser);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task DeleteAccountAsync(Guid userId)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null || user.DeletedAt != null) throw new NotFoundException("Không tìm thấy người dùng.");

            // Soft-delete
            user.DeletedAt = DateTime.UtcNow;

            // Xóa/Vô hiệu hóa Refresh Token
            var activeTokens = await _unitOfWork.RefreshTokens.FindAsync(rt => rt.UserId == userId && !rt.IsRevoked);
            foreach (var token in activeTokens)
            {
                token.IsRevoked = true;
                _unitOfWork.RefreshTokens.Update(token);
            }

            _unitOfWork.Users.Update(user);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task RestoreAccountAsync(Guid userId)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null) throw new NotFoundException("Không tìm thấy người dùng.");

            if (user.DeletedAt != null)
            {
                user.DeletedAt = null; // Phục hồi
                user.UpdatedAt = DateTime.UtcNow;

                _unitOfWork.Users.Update(user);
                await _unitOfWork.SaveChangesAsync();
            }
        }

        // --- Admin Methods ---

        public async Task<PagedResponse<List<AdminUserListResponse>>> GetAllUsersAsync(PaginationQuery query)
        {
            var allUsers = await _unitOfWork.Users.GetAllAsync();
            var usersList = allUsers.ToList();

            var totalItems = usersList.Count;
            var page = query.PageNumber;
            var size = query.PageSize;

            // Lấy tất cả body logs để lấy cân nặng gần nhất của từng user hiệu quả hơn
            var allBodyLogs = await _unitOfWork.UserBodyLogs.GetAllAsync();
            var latestWeights = allBodyLogs
                .GroupBy(b => b.UserId)
                .ToDictionary(
                    g => g.Key ?? Guid.Empty, 
                    g => g.OrderByDescending(b => b.RecordedAt).FirstOrDefault()?.Weight
                );

            var pagedUsers = usersList
                .OrderByDescending(u => u.CreatedAt)
                .Skip((page - 1) * size)
                .Take(size)
                .Select(u => new AdminUserListResponse
                {
                    Id = u.Id,
                    Email = u.Email,
                    FullName = u.FullName ?? "",
                    Role = u.Role ?? 1,
                    Gender = u.Gender,
                    IsDeleted = u.DeletedAt != null,
                    CreatedAt = u.CreatedAt,
                    DeletedAt = u.DeletedAt,
                    TargetGoal = u.TargetGoal,
                    Weight = latestWeights.TryGetValue(u.Id, out var w) && w.HasValue ? System.Math.Round(w.Value, 1) : null
                })
                .ToList();

            var totalPages = (int)Math.Ceiling(totalItems / (double)size);
            return new PagedResponse<List<AdminUserListResponse>>(pagedUsers, page, size, totalItems, totalPages);
        }


        public async Task<UserProfileResponse> GetUserByIdAsync(Guid userId)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null) throw new NotFoundException("Không tìm thấy người dùng.");
            
            // Lấy cân nặng thực tế gần nhất
            var latestWeightLog = await _unitOfWork.UserBodyLogs.GetLatestByUserAsync(userId);
            
            // Lấy lịch sử ghi nhận cân nặng
            var weightLogs = await _unitOfWork.UserBodyLogs.FindAsync(b => b.UserId == userId);
            
            var response = user.ToResponse();
            response.Weight = latestWeightLog?.Weight.HasValue == true ? System.Math.Round(latestWeightLog.Weight.Value, 1) : null;
            response.WeightHistory = weightLogs
                .OrderBy(b => b.RecordedAt)
                .Select(b => b.ToResponse())
                .ToList();
            
            return response;
        }

        public async Task ToggleUserStatusAsync(Guid userId, bool isDeleted)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null) throw new NotFoundException("Không tìm thấy người dùng.");

            user.DeletedAt = isDeleted ? DateTime.UtcNow : null;
            user.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Users.Update(user);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<AdminDashboardResponse> GetAdminDashboardAsync()
        {
            var allUsers = await _unitOfWork.Users.GetAllAsync();
            var today = DateTime.UtcNow.Date;

            return new AdminDashboardResponse
            {
                TotalUsers = allUsers.Count(),
                ActiveUsersToday = allUsers.Count(u => u.DeletedAt == null && u.UpdatedAt >= today),
                TotalMealPlans = (await _unitOfWork.MealPlans.GetAllAsync()).Count(),
                TotalWorkoutPlans = (await _unitOfWork.WorkoutPlans.GetAllAsync()).Count(),
                TotalExercises = (await _unitOfWork.Exercises.GetAllAsync()).Count(),
                TotalFoodItems = (await _unitOfWork.FoodItems.GetAllAsync()).Count()
            };
        }
    }
}