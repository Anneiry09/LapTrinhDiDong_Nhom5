using AnFit.Api.DTOs.Requests;
using System;
using System.Threading.Tasks;

namespace AnFit.Api.Services.Interfaces
{
    public interface IGarminSyncService
    {
        Task<string> InitiateConnectionAsync(Guid userId, string callbackUrl);
        Task HandleCallbackAsync(Guid userId, GarminCallbackRequest request);
        Task SyncDailyDataAsync(Guid userId, DateOnly date);
        Task DisconnectAsync(Guid userId);

        // Bridge mode: Kết nối đơn giản qua Python microservice (không cần OAuth redirect)
        Task ConnectBridgeAsync(Guid userId, string email, string password);

        // Lấy thông tin trạng thái liên kết Garmin hiện tại
        Task<object> GetIntegrationStatusAsync(Guid userId);
    }
}