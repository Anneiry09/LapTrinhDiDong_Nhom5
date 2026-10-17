using AnFit.Api.DTOs.Requests;
using AnFit.Api.Exceptions;
using AnFit.Api.Integrations.Garmin;
using AnFit.Api.Models.Entities;
using AnFit.Api.Models.Enums;
using AnFit.Api.Repositories.Interfaces;
using AnFit.Api.Services.Interfaces;
using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace AnFit.Api.Services.Implementations.Core
{
    public class GarminSyncService : IGarminSyncService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IGarminClient _garminClient;

        public GarminSyncService(IUnitOfWork unitOfWork, IGarminClient garminClient)
        {
            _unitOfWork = unitOfWork;
            _garminClient = garminClient;
        }

        public async Task<string> InitiateConnectionAsync(Guid userId, string callbackUrl)
        {
            // 1. Gọi Garmin lấy Token tạm thời
            var (requestToken, requestSecret) = await _garminClient.GetRequestTokenAsync();

            // 2. Lưu tạm Secret vào DB để lát nữa dùng đổi Access Token
            var integration = await _unitOfWork.UserIntegrations.GetByUserAndProviderAsync(userId, (int)IntegrationProvider.Garmin);
            if (integration == null)
            {
                integration = new UserIntegration
                {
                    UserId = userId,
                    Provider = (int)IntegrationProvider.Garmin,
                    ExternalAccountId = $"TEMP|{requestSecret}", // Lưu tạm mã bí mật
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                await _unitOfWork.UserIntegrations.AddAsync(integration);
            }
            else
            {
                integration.ExternalAccountId = $"TEMP|{requestSecret}";
                integration.UpdatedAt = DateTime.UtcNow;
                _unitOfWork.UserIntegrations.Update(integration);
            }

            await _unitOfWork.SaveChangesAsync();

            // 3. Trả về link URL để Frontend bật Popup cho User đăng nhập Garmin
            return _garminClient.GetAuthorizeUrl(requestToken, callbackUrl);
        }

        public async Task HandleCallbackAsync(Guid userId, GarminCallbackRequest request)
        {
            var integration = await _unitOfWork.UserIntegrations.GetByUserAndProviderAsync(userId, (int)IntegrationProvider.Garmin);
            if (integration == null || string.IsNullOrEmpty(integration.ExternalAccountId) || !integration.ExternalAccountId.StartsWith("TEMP|"))
            {
                throw new BadRequestException("Phiên kết nối không hợp lệ hoặc đã hết hạn.");
            }

            // Lấy lại mã Secret tạm thời đã lưu
            var requestSecret = integration.ExternalAccountId.Split('|')[1];

            // Gọi Garmin đổi lấy Token chính thức
            var (accessToken, accessSecret) = await _garminClient.ExchangeAccessTokenAsync(request.OAuthToken, requestSecret, request.OAuthVerifier);

            // Lưu Token chính thức vào DB
            integration.ExternalAccountId = $"{accessToken}|{accessSecret}";
            integration.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.UserIntegrations.Update(integration);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task SyncDailyDataAsync(Guid userId, DateOnly date)
        {
            var integration = await _unitOfWork.UserIntegrations.GetByUserAndProviderAsync(userId, (int)IntegrationProvider.Garmin);
            if (integration == null || string.IsNullOrEmpty(integration.ExternalAccountId) || integration.ExternalAccountId.StartsWith("TEMP|"))
            {
                throw new BadRequestException("Tài khoản chưa được kết nối với Garmin.");
            }

            string accessToken;
            string accessSecret;

            if (integration.ExternalAccountId.Contains("|"))
            {
                var parts = integration.ExternalAccountId.Split('|');
                accessToken = parts[0];
                accessSecret = parts[1];
            }
            else
            {
                accessToken = integration.ExternalAccountId;
                accessSecret = integration.AccessToken ?? string.Empty;
            }

            // 1. Kéo dữ liệu từ Garmin
            string jsonResponse = await _garminClient.GetDailySummaryAsync(accessToken, accessSecret, date);
            if (string.IsNullOrEmpty(jsonResponse))
            {
                throw new BadRequestException($"Không nhận được phản hồi dữ liệu từ Garmin cho ngày {date.ToString("yyyy-MM-dd")}.");
            }

            // 2. Bóc tách JSON của Garmin
            try
            {
                using var doc = JsonDocument.Parse(jsonResponse);
                JsonElement root;
                if (doc.RootElement.ValueKind == JsonValueKind.Array)
                {
                    if (doc.RootElement.GetArrayLength() == 0)
                    {
                        throw new BadRequestException("Phản hồi dữ liệu từ Garmin trống (mảng rỗng).");
                    }
                    root = doc.RootElement[0];
                }
                else if (doc.RootElement.ValueKind == JsonValueKind.Object)
                {
                    root = doc.RootElement;
                }
                else
                {
                    throw new BadRequestException("Định dạng phản hồi dữ liệu từ Garmin không hợp lệ (không phải mảng hoặc đối tượng).");
                }

                // Hàm local helper bóc tách dữ liệu an toàn kể cả khi trả về dạng Chuỗi
                double GetDoubleValue(JsonElement element, string propName)
                {
                    if (element.TryGetProperty(propName, out var prop))
                    {
                        if (prop.ValueKind == JsonValueKind.Number) return prop.GetDouble();
                        if (prop.ValueKind == JsonValueKind.String && double.TryParse(prop.GetString(), out var val)) return val;
                    }
                    return 0;
                }

                int GetIntValue(JsonElement element, string propName)
                {
                    if (element.TryGetProperty(propName, out var prop))
                    {
                        if (prop.ValueKind == JsonValueKind.Number) return prop.GetInt32();
                        if (prop.ValueKind == JsonValueKind.String && int.TryParse(prop.GetString(), out var val)) return val;
                    }
                    return 0;
                }

                int? GetNullableIntValue(JsonElement element, string propName)
                {
                    if (element.TryGetProperty(propName, out var prop))
                    {
                        if (prop.ValueKind == JsonValueKind.Number) return prop.GetInt32();
                        if (prop.ValueKind == JsonValueKind.String && int.TryParse(prop.GetString(), out var val)) return val;
                    }
                    return null;
                }

                double activeCalories = GetDoubleValue(root, "activeKilocalories");
                double consumedCalories = GetDoubleValue(root, "consumedKilocalories");
                int steps = GetIntValue(root, "steps");
                int restingHeartRate = GetIntValue(root, "restingHeartRate");
                int? maxHeartRate = GetNullableIntValue(root, "maxHeartRate");
                int? sleepDuration = GetNullableIntValue(root, "sleepDurationMinutes");
                int? stressLevel = GetNullableIntValue(root, "stressLevel");

                // 3. Cập nhật vào hệ thống AnFit (Bảng DailyHealthLog)
                var logs = await _unitOfWork.DailyHealthLogs.GetByUserAndDateRangeAsync(userId, date, date);
                var dailyLog = logs.FirstOrDefault() ?? new DailyHealthLog { UserId = userId, RecordDate = date, CreatedAt = DateTime.UtcNow };

                dailyLog.ActiveCalories = activeCalories;
                dailyLog.CaloriesConsumed = consumedCalories > 0 ? consumedCalories : dailyLog.CaloriesConsumed;
                dailyLog.TotalSteps = steps;
                dailyLog.RestingHeartRate = restingHeartRate;
                
                // Cập nhật các trường mới
                if (maxHeartRate > 0) dailyLog.MaxHeartRate = maxHeartRate;
                if (sleepDuration > 0) dailyLog.SleepDurationMinutes = sleepDuration;
                if (stressLevel > 0) dailyLog.StressLevel = stressLevel;

                // Tính toán hoặc cập nhật TDEE Mifflin-St Jeor nếu chưa có hoặc bằng 0
                if (dailyLog.CalculatedTdee == null || dailyLog.CalculatedTdee == 0)
                {
                    var user = await _unitOfWork.Users.GetByIdAsync(userId);
                    var latestBody = await _unitOfWork.UserBodyLogs.GetLatestByUserAsync(userId);
                    double tdee = 2000; // Mặc định nếu thiếu thông tin chiều cao/cân nặng/sinh nhật

                    if (user != null && latestBody != null && user.HeightCm > 0 && user.DateOfBirth.HasValue)
                    {
                        int age = DateTime.UtcNow.Year - user.DateOfBirth.Value.Year;
                        double weight = latestBody.Weight ?? 60;
                        double height = user.HeightCm ?? 160;

                        double bmr = (10 * weight) + (6.25 * height) - (5 * age);

                        // Nam (1) cộng 5, Nữ (2) trừ 161
                        bmr = user.Gender == 1 ? bmr + 5 : bmr - 161;

                        // TDEE = BMR * Hệ số vận động (Mặc định lấy 1.2)
                        tdee = bmr * 1.2;
                    }

                    dailyLog.CalculatedTdee = Math.Round(tdee, 2);
                }

                dailyLog.UpdatedAt = DateTime.UtcNow;

                if (dailyLog.Id == 0) await _unitOfWork.DailyHealthLogs.AddAsync(dailyLog);
                else _unitOfWork.DailyHealthLogs.Update(dailyLog);

                await _unitOfWork.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi parse dữ liệu Garmin: {ex.Message}");
                if (ex is BadRequestException) throw;
                throw new BadRequestException($"Lỗi phân tích dữ liệu Garmin cho ngày {date.ToString("yyyy-MM-dd")}: {ex.Message}");
            }
        }

        public async Task DisconnectAsync(Guid userId)
        {
            var integration = await _unitOfWork.UserIntegrations.GetByUserAndProviderAsync(userId, (int)IntegrationProvider.Garmin);
            if (integration != null)
            {
                // 1. Xóa toàn bộ dữ liệu sức khỏe đã đồng bộ từ Garmin (theo cam kết Privacy Policy - Right to be Forgotten)
                var healthLogs = await _unitOfWork.DailyHealthLogs.GetAllByUserAsync(userId);
                if (healthLogs.Any())
                {
                    _unitOfWork.DailyHealthLogs.RemoveRange(healthLogs);
                }

                // 2. Xóa thông tin kết nối (Token) của Garmin
                _unitOfWork.UserIntegrations.Remove(integration);

                await _unitOfWork.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Kết nối Garmin qua Bridge mode (Python microservice).
        /// Đăng nhập động với tài khoản người dùng, mã hóa Base64 mật khẩu trước khi lưu.
        /// </summary>
        public async Task ConnectBridgeAsync(Guid userId, string email, string password)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                throw new BadRequestException("Email và mật khẩu Garmin không được để trống.");
            }

            // 1. Kiểm tra thông tin đăng nhập thực tế bằng cách gọi sang Python Bridge
            var isValid = await _garminClient.ValidateCredentialsAsync(email, password);
            if (!isValid)
            {
                throw new BadRequestException("Đăng nhập Garmin không thành công. Vui lòng kiểm tra lại Email và Mật khẩu.");
            }

            // 2. Mã hóa Base64 mật khẩu để lưu bảo mật đơn giản cho Capstone
            var passwordBytes = System.Text.Encoding.UTF8.GetBytes(password);
            var base64Password = Convert.ToBase64String(passwordBytes);

            var integration = await _unitOfWork.UserIntegrations.GetByUserAndProviderAsync(userId, (int)IntegrationProvider.Garmin);
            if (integration == null)
            {
                integration = new UserIntegration
                {
                    UserId = userId,
                    Provider = (int)IntegrationProvider.Garmin,
                    ExternalAccountId = email,
                    AccessToken = base64Password,
                    RefreshToken = "BRIDGE_MODE",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                await _unitOfWork.UserIntegrations.AddAsync(integration);
            }
            else
            {
                integration.ExternalAccountId = email;
                integration.AccessToken = base64Password;
                integration.RefreshToken = "BRIDGE_MODE";
                integration.IsActive = true;
                integration.UpdatedAt = DateTime.UtcNow;
                _unitOfWork.UserIntegrations.Update(integration);
            }

            await _unitOfWork.SaveChangesAsync();
        }

        /// <summary>
        /// Lấy thông tin trạng thái liên kết Garmin hiện tại của người dùng.
        /// </summary>
        public async Task<object> GetIntegrationStatusAsync(Guid userId)
        {
            var integration = await _unitOfWork.UserIntegrations.GetByUserAndProviderAsync(userId, (int)IntegrationProvider.Garmin);
            if (integration == null || integration.IsActive != true)
            {
                return new { connected = false };
            }

            return new
            {
                connected = true,
                email = integration.ExternalAccountId ?? "Đã kết nối"
            };
        }
    }
}