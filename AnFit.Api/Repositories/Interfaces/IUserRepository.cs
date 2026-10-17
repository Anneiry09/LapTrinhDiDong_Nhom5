using AnFit.Api.Models.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AnFit.Api.Repositories.Interfaces
{
    // Kế thừa IRepository<User> để có sẵn Thêm/Sửa/Xóa, sau đó khai báo thêm hàm riêng
    public interface IUserRepository : IRepository<User>
    {
        Task<User?> GetByEmailAsync(string email);
        Task<bool> IsEmailExistsAsync(string email);

        // Lấy danh sách user đang hoạt động (chưa bị soft-delete)
        Task<IEnumerable<User>> GetActiveUsersAsync();

        // Soft-delete: đánh dấu IsDeleted = true thay vì xóa thật
        Task SoftDeleteAsync(Guid userId);

        Task<User?> GetWithDetailsAsync(Guid userId);
    }
}