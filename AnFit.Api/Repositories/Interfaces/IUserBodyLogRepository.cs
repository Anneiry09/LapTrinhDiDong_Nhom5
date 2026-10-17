using AnFit.Api.Models.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AnFit.Api.Repositories.Interfaces
{
    public interface IUserBodyLogRepository : IRepository<UserBodyLog>
    {
        Task<UserBodyLog?> GetLatestByUserAsync(Guid userId);

        // Lấy lịch sử đo lường cơ thể phân trang
        Task<IEnumerable<UserBodyLog>> GetHistoryAsync(Guid userId, int pageNumber, int pageSize);
    }
}