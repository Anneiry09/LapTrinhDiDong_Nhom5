using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace AnFit.Api.Repositories.Interfaces
{
    // Chữ 'T' đại diện cho bất kỳ Entity nào (User, Exercise, FoodItem...)
    public interface IRepository<T> where T : class
    {
        Task<IEnumerable<T>> GetAllAsync();

        // Trả về nullable vì FindAsync có thể không tìm thấy entity
        Task<T?> GetByIdAsync(object id);

        // Hàm FindAsync này dùng để tìm kiếm theo điều kiện (VD: x => x.Name == "Yoga")
        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);

        Task AddAsync(T entity);
        Task AddRangeAsync(IEnumerable<T> entities);

        // Update và Delete không có bản chất bất đồng bộ (async) trong EF Core
        void Update(T entity);
        void Remove(T entity);
        void RemoveRange(IEnumerable<T> entities);
    }
}