using System;

namespace AnFit.Api.Models.Entities;

/// <summary>
/// Lịch sử giá thực phẩm — cho phép cập nhật giá linh hoạt theo thời gian/thị trường
/// mà vẫn giữ dữ liệu cũ cho audit/analytics.
/// FoodItems.EstimatedPrice luôn là giá hiện tại (dùng cho engine).
/// Khi admin cập nhật giá → INSERT vào đây (lưu vết) + UPDATE FoodItems.EstimatedPrice.
/// </summary>
public class FoodPriceHistory
{
    public int Id { get; set; }

    /// <summary>FK → FoodItems.Id</summary>
    public int FoodItemId { get; set; }

    /// <summary>Giá tại thời điểm ghi nhận (VNĐ / đơn vị)</summary>
    public decimal Price { get; set; }

    /// <summary>Thời điểm ghi nhận giá</summary>
    public DateTime RecordedAt { get; set; }

    /// <summary>Nguồn cập nhật: 'market_update', 'admin_manual', 'auto_crawl', 'initial_seed'</summary>
    public string? Source { get; set; }

    // Navigation
    public virtual FoodItem? FoodItem { get; set; }
}
