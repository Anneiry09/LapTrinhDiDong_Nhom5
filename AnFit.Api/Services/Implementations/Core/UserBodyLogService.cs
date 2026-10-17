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
    public class UserBodyLogService : IUserBodyLogService
    {
        private readonly IUnitOfWork _unitOfWork;

        public UserBodyLogService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<BodyLogResponse> CreateAsync(Guid userId, CreateBodyLogRequest request)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null) throw new NotFoundException("Không tìm thấy người dùng.");

            // Tính BMI nếu User đã khai báo chiều cao
            double? bmi = null;
            if (user.HeightCm.HasValue && user.HeightCm.Value > 0)
            {
                double heightInMeters = user.HeightCm.Value / 100.0;
                bmi = Math.Round(request.Weight / (heightInMeters * heightInMeters), 2);
            }

            var bodyLog = new UserBodyLog
            {
                UserId = userId,
                Weight = request.Weight,
                BodyFatPercentage = request.BodyFatPercentage,
                Bmi = bmi,
                RecordedAt = request.RecordedAt,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _unitOfWork.UserBodyLogs.AddAsync(bodyLog);
            await _unitOfWork.SaveChangesAsync();

            return bodyLog.ToResponse();
        }

        public async Task<BodyLogResponse?> GetLatestAsync(Guid userId)
        {
            var latestLog = await _unitOfWork.UserBodyLogs.GetLatestByUserAsync(userId);
            return latestLog?.ToResponse();
        }

        public async Task<PagedResponse<System.Collections.Generic.IEnumerable<BodyLogResponse>>> GetHistoryAsync(Guid userId, PaginationQuery query)
        {
            var logs = await _unitOfWork.UserBodyLogs.FindAsync(u => u.UserId == userId);

            // Xử lý phân trang
            var totalRecords = logs.Count();
            var totalPages = (int)Math.Ceiling(totalRecords / (double)query.PageSize);

            var pagedData = logs.OrderByDescending(x => x.RecordedAt)
                                .Skip((query.PageNumber - 1) * query.PageSize)
                                .Take(query.PageSize)
                                .Select(x => x.ToResponse())
                                .ToList();

            return new PagedResponse<System.Collections.Generic.IEnumerable<BodyLogResponse>>(
                pagedData, query.PageNumber, query.PageSize, totalRecords, totalPages);
        }
    }
}