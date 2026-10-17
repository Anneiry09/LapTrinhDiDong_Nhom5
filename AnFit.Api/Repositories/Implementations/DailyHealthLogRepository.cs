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
    public class DailyHealthLogRepository : Repository<DailyHealthLog>, IDailyHealthLogRepository
    {
        public DailyHealthLogRepository(ApplicationDbContext context) : base(context) { }

        public async Task<DailyHealthLog?> GetLatestByUserAsync(Guid userId)
        {
            return await _dbSet.Where(d => d.UserId == userId)
                               .OrderByDescending(d => d.RecordDate)
                               .FirstOrDefaultAsync();
        }

        // Dùng DateOnly để khớp với kiểu dữ liệu của Entity
        public async Task<IEnumerable<DailyHealthLog>> GetByUserAndDateRangeAsync(Guid userId, DateOnly startDate, DateOnly endDate)
        {
            return await _dbSet.Where(d => d.UserId == userId
                                        && d.RecordDate >= startDate
                                        && d.RecordDate <= endDate)
                               .OrderBy(d => d.RecordDate)
                               .ToListAsync();
        }

        // Lấy toàn bộ dữ liệu sức khỏe của user (phục vụ xóa sạch khi ngắt kết nối Garmin - theo cam kết Privacy Policy)
        public async Task<IEnumerable<DailyHealthLog>> GetAllByUserAsync(Guid userId)
        {
            return await _dbSet.Where(d => d.UserId == userId)
                               .ToListAsync();
        }
    }
}