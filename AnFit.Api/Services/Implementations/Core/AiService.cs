using AnFit.Api.Services.Interfaces;
using AnFit.Api.Extensions;
using AnFit.Api.Exceptions;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

namespace AnFit.Api.Services.Implementations.Core
{
    /// <summary>
    /// Service gọi HTTP sang Python AI Microservice (FastAPI).
    /// Hỗ trợ 2 endpoint: Custom Workout Session (Hybrid AI) và Meal Plan Generator.
    /// </summary>
    public class AiService : IAiService
    {
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonOptions;

        public AiService(HttpClient httpClient, IOptions<GarminSettings> garminSettings)
        {
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri(garminSettings.Value.BridgeBaseUrl);

            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                PropertyNameCaseInsensitive = true
            };
        }

        /// <summary>
        /// Gọi Python AI: Sinh buổi tập linh hoạt (Hybrid AI = ML Scorer + Knapsack).
        /// POST /api/ai/generate-custom-session
        /// </summary>
        public async Task<AiCustomSessionResult> GenerateCustomSessionAsync(AiCustomSessionRequest request)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("/api/ai/generate-custom-session", request, _jsonOptions);
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<AiCustomSessionResult>(_jsonOptions);
                return result ?? throw new ApiException("AI trả về kết quả rỗng.", 500);
            }
            catch (HttpRequestException ex)
            {
                throw new ApiException(
                    $"Không thể kết nối đến AI Microservice. Hãy đảm bảo Python server đang chạy (port 8001). Chi tiết: {ex.Message}", 503);
            }
            catch (JsonException ex)
            {
                throw new ApiException($"Lỗi xử lý dữ liệu từ AI: {ex.Message}", 500);
            }
        }

        /// <summary>
        /// Gọi Python AI: Sinh thực đơn thông minh (Greedy Algorithm).
        /// POST /api/ai/generate-meal-plan
        /// </summary>
        public async Task<AiMealPlanResult> GenerateMealPlanAsync(AiMealPlanRequest request)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("/api/ai/generate-meal-plan", request, _jsonOptions);
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<AiMealPlanResult>(_jsonOptions);
                return result ?? throw new ApiException("AI trả về kết quả rỗng.", 500);
            }
            catch (HttpRequestException ex)
            {
                throw new ApiException(
                    $"Không thể kết nối đến AI Microservice. Chi tiết: {ex.Message}", 503);
            }
            catch (JsonException ex)
            {
                throw new ApiException($"Lỗi xử lý dữ liệu từ AI: {ex.Message}", 500);
            }
        }

        /// <summary>
        /// Gọi Python AI: Sinh hướng dẫn nấu ăn (Gemini LLM + Fallback rule-based).
        /// POST /api/ai/generate-instructions
        /// </summary>
        public async Task<AiInstructionsResult> GenerateInstructionsAsync(AiInstructionsRequest request)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("/api/ai/generate-instructions", request, _jsonOptions);
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<AiInstructionsResult>(_jsonOptions);
                return result ?? throw new ApiException("AI trả về kết quả rỗng.", 500);
            }
            catch (HttpRequestException ex)
            {
                throw new ApiException(
                    $"Không thể kết nối đến AI Microservice. Chi tiết: {ex.Message}", 503);
            }
            catch (JsonException ex)
            {
                throw new ApiException($"Lỗi xử lý dữ liệu từ AI: {ex.Message}", 500);
            }
        }
    }
}
