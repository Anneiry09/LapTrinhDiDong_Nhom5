using AnFit.Api.Models.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AnFit.Api.Repositories.Interfaces
{
    public interface IDailyHealthLogRepository : IRepository<DailyHealthLog>
    {
        Task<DailyHealthLog?> GetLatestByUserAsync(Guid userId);

        // Dùng DateOnly thay vì DateTime vì Entity RecordDate là DateOnly?
        Task<IEnumerable<DailyHealthLog>> GetByUserAndDateRangeAsync(Guid userId, DateOnly startDate, DateOnly endDate);

        // Lấy toàn bộ dữ liệu sức khỏe của user (phục vụ xóa sạch khi ngắt kết nối Garmin)
        Task<IEnumerable<DailyHealthLog>> GetAllByUserAsync(Guid userId);
    }
}