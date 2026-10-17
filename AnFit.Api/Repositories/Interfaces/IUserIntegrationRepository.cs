using AnFit.Api.Models.Entities;
using System;
using System.Threading.Tasks;

namespace AnFit.Api.Repositories.Interfaces
{
    public interface IUserIntegrationRepository : IRepository<UserIntegration>
    {
        Task<UserIntegration?> GetByUserAndProviderAsync(Guid userId, int provider);
    }
}