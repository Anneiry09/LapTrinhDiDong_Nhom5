using AnFit.Api.Models.Entities;
using AnFit.Api.Models.Enums;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AnFit.Api.Data
{
    public class DataSeeder
    {
        private readonly ApplicationDbContext _context;

        public DataSeeder(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task SeedAsync()
        {
            // =========================================================
            // 1. Seed Medical Conditions (Bệnh lý / Dị ứng)
            // =========================================================
            if (!await _context.MedicalConditions.AnyAsync())
            {
                var conditions = new List<MedicalCondition>
                {
                    new MedicalCondition { ConditionName = "Dị ứng đậu phộng", Type = (int)MedicalConditionType.Allergy },
                    new MedicalCondition { ConditionName = "Dị ứng hải sản", Type = (int)MedicalConditionType.Allergy },
                    new MedicalCondition { ConditionName = "Dị ứng sữa / Lactose", Type = (int)MedicalConditionType.Allergy },
                    new MedicalCondition { ConditionName = "Dị ứng trứng", Type = (int)MedicalConditionType.Allergy },
                    new MedicalCondition { ConditionName = "Dị ứng đậu nành", Type = (int)MedicalConditionType.Allergy },
                    new MedicalCondition { ConditionName = "Dị ứng gluten (Celiac)", Type = (int)MedicalConditionType.Allergy },
                    new MedicalCondition { ConditionName = "Bệnh gout", Type = (int)MedicalConditionType.ChronicDisease },
                    new MedicalCondition { ConditionName = "Bệnh thận mãn tính", Type = (int)MedicalConditionType.ChronicDisease },
                    new MedicalCondition { ConditionName = "Tiểu đường Type 2", Type = (int)MedicalConditionType.ChronicDisease },
                    new MedicalCondition { ConditionName = "Huyết áp cao", Type = (int)MedicalConditionType.ChronicDisease },
                    new MedicalCondition { ConditionName = "Bệnh tim mạch", Type = (int)MedicalConditionType.ChronicDisease },
                    new MedicalCondition { ConditionName = "Cholesterol cao", Type = (int)MedicalConditionType.ChronicDisease },
                    new MedicalCondition { ConditionName = "Chấn thương đầu gối", Type = (int)MedicalConditionType.ChronicDisease },
                    new MedicalCondition { ConditionName = "Đau lưng dưới mãn tính", Type = (int)MedicalConditionType.ChronicDisease },
                    new MedicalCondition { ConditionName = "Người ăn chay (Vegan)", Type = (int)MedicalConditionType.DietaryRestriction }
                };
                await _context.MedicalConditions.AddRangeAsync(conditions);
                await _context.SaveChangesAsync();
            }

            // =========================================================
            // 2. Seed Food Items (Thực phẩm cơ bản)
            // =========================================================
            if (!await _context.FoodItems.AnyAsync())
            {
                var foods = new List<FoodItem>
                {
                    // =============================================
                    // TINH BỘT / CARBS (nguyên liệu)
                    // =============================================
                    new FoodItem { Name = "Gạo trắng", CaloriesPerUnit = 130, ProteinPerUnit = 2.7, CarbsPerUnit = 28, FatPerUnit = 0.3, EstimatedPrice = 5000, Unit = "100g (nấu chín)", Category = 1, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new FoodItem { Name = "Gạo lứt", CaloriesPerUnit = 123, ProteinPerUnit = 2.7, CarbsPerUnit = 26, FatPerUnit = 1.0, EstimatedPrice = 7000, Unit = "100g (nấu chín)", Category = 1, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new FoodItem { Name = "Bánh mì trắng", CaloriesPerUnit = 265, ProteinPerUnit = 9, CarbsPerUnit = 49, FatPerUnit = 3.2, EstimatedPrice = 5000, Unit = "100g", Category = 1, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new FoodItem { Name = "Bánh mì nguyên cám", CaloriesPerUnit = 247, ProteinPerUnit = 13, CarbsPerUnit = 41, FatPerUnit = 3.4, EstimatedPrice = 12000, Unit = "100g", Category = 1, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new FoodItem { Name = "Bún tươi", CaloriesPerUnit = 108, ProteinPerUnit = 3.5, CarbsPerUnit = 25, FatPerUnit = 0.2, EstimatedPrice = 5000, Unit = "100g", Category = 1, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new FoodItem { Name = "Phở tươi (bánh phở)", CaloriesPerUnit = 110, ProteinPerUnit = 3.0, CarbsPerUnit = 24, FatPerUnit = 0.5, EstimatedPrice = 6000, Unit = "100g", Category = 1, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new FoodItem { Name = "Miến dong", CaloriesPerUnit = 334, ProteinPerUnit = 0.1, CarbsPerUnit = 83, FatPerUnit = 0.1, EstimatedPrice = 8000, Unit = "100g", Category = 1, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new FoodItem { Name = "Khoai lang", CaloriesPerUnit = 86, ProteinPerUnit = 1.6, CarbsPerUnit = 20, FatPerUnit = 0.1, EstimatedPrice = 8000, Unit = "100g", Category = 1, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new FoodItem { Name = "Khoai tây", CaloriesPerUnit = 77, ProteinPerUnit = 2.0, CarbsPerUnit = 17, FatPerUnit = 0.1, EstimatedPrice = 7000, Unit = "100g", Category = 1, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new FoodItem { Name = "Yến mạch (Oat)", CaloriesPerUnit = 389, ProteinPerUnit = 17, CarbsPerUnit = 66, FatPerUnit = 7, EstimatedPrice = 12000, Unit = "100g", Category = 1, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new FoodItem { Name = "Ngô ngọt", CaloriesPerUnit = 96, ProteinPerUnit = 3.4, CarbsPerUnit = 21, FatPerUnit = 1.5, EstimatedPrice = 5000, Unit = "bắp", Category = 1, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new FoodItem { Name = "Khoai mỡ", CaloriesPerUnit = 118, ProteinPerUnit = 1.5, CarbsPerUnit = 28, FatPerUnit = 0.2, EstimatedPrice = 8000, Unit = "100g", Category = 1, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new FoodItem { Name = "Mì ý (Pasta)", CaloriesPerUnit = 131, ProteinPerUnit = 5.0, CarbsPerUnit = 25, FatPerUnit = 1.1, EstimatedPrice = 10000, Unit = "100g", Category = 1, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new FoodItem { Name = "Nếp", CaloriesPerUnit = 97, ProteinPerUnit = 2.0, CarbsPerUnit = 21, FatPerUnit = 0.2, EstimatedPrice = 6000, Unit = "100g (nấu chín)", Category = 1, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },

                    // =============================================
                    // PROTEIN ĐỘNG VẬT (nguyên liệu thô)
                    // =============================================
                    new FoodItem { Name = "Ức gà", CaloriesPerUnit = 165, ProteinPerUnit = 31, CarbsPerUnit = 0, FatPerUnit = 3.6, EstimatedPrice = 15000, Unit = "100g", Category = 2, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new FoodItem { Name = "Đùi gà (bỏ da)", CaloriesPerUnit = 191, ProteinPerUnit = 26, CarbsPerUnit = 0, FatPerUnit = 9.3, EstimatedPrice = 18000, Unit = "100g", Category = 2, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new FoodItem { Name = "Cánh gà", CaloriesPerUnit = 203, ProteinPerUnit = 18, CarbsPerUnit = 0, FatPerUnit = 14, EstimatedPrice = 16000, Unit = "100g", Category = 2, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new FoodItem { Name = "Thịt bò nạc", CaloriesPerUnit = 250, ProteinPerUnit = 26, CarbsPerUnit = 0, FatPerUnit = 15, EstimatedPrice = 40000, Unit = "100g", Category = 2, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new FoodItem { Name = "Thịt bò bắp", CaloriesPerUnit = 217, ProteinPerUnit = 28, CarbsPerUnit = 0, FatPerUnit = 11, EstimatedPrice = 35000, Unit = "100g", Category = 2, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new FoodItem { Name = "Thịt heo nạc vai", CaloriesPerUnit = 143, ProteinPerUnit = 26, CarbsPerUnit = 0, FatPerUnit = 3.5, EstimatedPrice = 20000, Unit = "100g", Category = 2, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new FoodItem { Name = "Thịt heo băm nạc", CaloriesPerUnit = 143, ProteinPerUnit = 26, CarbsPerUnit = 0, FatPerUnit = 3.5, EstimatedPrice = 22000, Unit = "100g", Category = 2, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new FoodItem { Name = "Sườn heo", CaloriesPerUnit = 210, ProteinPerUnit = 20, CarbsPerUnit = 0, FatPerUnit = 14, EstimatedPrice = 30000, Unit = "100g", Category = 2, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new FoodItem { Name = "Trứng gà", CaloriesPerUnit = 78, ProteinPerUnit = 6.3, CarbsPerUnit = 0.6, FatPerUnit = 5.3, EstimatedPrice = 4000, Unit = "quả", Category = 2, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new FoodItem { Name = "Trứng vịt", CaloriesPerUnit = 130, ProteinPerUnit = 9, CarbsPerUnit = 1, FatPerUnit = 10, EstimatedPrice = 5000, Unit = "quả", Category = 2, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new FoodItem { Name = "Trứng cút", CaloriesPerUnit = 14, ProteinPerUnit = 1.2, CarbsPerUnit = 0, FatPerUnit = 1, EstimatedPrice = 1000, Unit = "quả", Category = 2, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new FoodItem { Name = "Cá hồi (fillet)", CaloriesPerUnit = 208, ProteinPerUnit = 20, CarbsPerUnit = 0, FatPerUnit = 13, EstimatedPrice = 50000, Unit = "100g", Category = 2, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new FoodItem { Name = "Cá basa (fillet)", CaloriesPerUnit = 90, ProteinPerUnit = 18, CarbsPerUnit = 0, FatPerUnit = 2, EstimatedPrice = 15000, Unit = "100g", Category = 2, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new FoodItem { Name = "Cá thu", CaloriesPerUnit = 190, ProteinPerUnit = 19, CarbsPerUnit = 0, FatPerUnit = 12, EstimatedPrice = 25000, Unit = "100g", Category = 2, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new FoodItem { Name = "Cá ngừ đóng hộp", CaloriesPerUnit = 128, ProteinPerUnit = 26, CarbsPerUnit = 0, FatPerUnit = 2.5, EstimatedPrice = 20000, Unit = "100g", Category = 2, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new FoodItem { Name = "Cá diêu hồng", CaloriesPerUnit = 100, ProteinPerUnit = 20, CarbsPerUnit = 0, FatPerUnit = 2, EstimatedPrice = 18000, Unit = "100g", Category = 2, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new FoodItem { Name = "Cá lóc", CaloriesPerUnit = 95, ProteinPerUnit = 18, CarbsPerUnit = 0, FatPerUnit = 2, EstimatedPrice = 20000, Unit = "100g", Category = 2, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new FoodItem { Name = "Tôm sú", CaloriesPerUnit = 99, ProteinPerUnit = 24, CarbsPerUnit = 0.2, FatPerUnit = 0.3, EstimatedPrice = 25000, Unit = "100g", Category = 2, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new FoodItem { Name = "Mực ống", CaloriesPerUnit = 92, ProteinPerUnit = 18, CarbsPerUnit = 3, FatPerUnit = 1.4, EstimatedPrice = 28000, Unit = "100g", Category = 2, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new FoodItem { Name = "Nghêu", CaloriesPerUnit = 73, ProteinPerUnit = 13, CarbsPerUnit = 2.5, FatPerUnit = 1, EstimatedPrice = 20000, Unit = "100g", Category = 2, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },

                    // =============================================
                    // PROTEIN THỰC VẬT / SỮA
                    // =============================================
                    new FoodItem { Name = "Đậu hũ trắng", CaloriesPerUnit = 76, ProteinPerUnit = 8, CarbsPerUnit = 1.9, FatPerUnit = 4.8, EstimatedPrice = 8000, Unit = "100g", Category = 2, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new FoodItem { Name = "Đậu hũ non", CaloriesPerUnit = 55, ProteinPerUnit = 5, CarbsPerUnit = 2.0, FatPerUnit = 3.5, EstimatedPrice = 6000, Unit = "100g", Category = 2, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new FoodItem { Name = "Tempeh", CaloriesPerUnit = 192, ProteinPerUnit = 20, CarbsPerUnit = 8, FatPerUnit = 11, EstimatedPrice = 18000, Unit = "100g", Category = 2, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new FoodItem { Name = "Đậu đen", CaloriesPerUnit = 132, ProteinPerUnit = 8.9, CarbsPerUnit = 24, FatPerUnit = 0.5, EstimatedPrice = 6000, Unit = "100g", Category = 2, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new FoodItem { Name = "Đậu xanh", CaloriesPerUnit = 105, ProteinPerUnit = 7.0, CarbsPerUnit = 19, FatPerUnit = 0.4, EstimatedPrice = 5000, Unit = "100g", Category = 2, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new FoodItem { Name = "Đậu lăng", CaloriesPerUnit = 116, ProteinPerUnit = 9.0, CarbsPerUnit = 20, FatPerUnit = 0.4, EstimatedPrice = 10000, Unit = "100g", Category = 2, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new FoodItem { Name = "Đậu chickpea", CaloriesPerUnit = 164, ProteinPerUnit = 9.0, CarbsPerUnit = 27, FatPerUnit = 2.6, EstimatedPrice = 12000, Unit = "100g", Category = 2, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new FoodItem { Name = "Đậu nành (Edamame)", CaloriesPerUnit = 122, ProteinPerUnit = 11, CarbsPerUnit = 10, FatPerUnit = 5, EstimatedPrice = 12000, Unit = "100g", Category = 2, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new FoodItem { Name = "Sữa chua Hy Lạp", CaloriesPerUnit = 59, ProteinPerUnit = 10, CarbsPerUnit = 3.6, FatPerUnit = 0.7, EstimatedPrice = 15000, Unit = "100g", Category = 2, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new FoodItem { Name = "Sữa chua không đường", CaloriesPerUnit = 61, ProteinPerUnit = 3.5, CarbsPerUnit = 4.7, FatPerUnit = 3.3, EstimatedPrice = 7000, Unit = "hộp 100g", Category = 2, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new FoodItem { Name = "Phô mai Cottage", CaloriesPerUnit = 98, ProteinPerUnit = 11, CarbsPerUnit = 3.4, FatPerUnit = 4.3, EstimatedPrice = 20000, Unit = "100g", Category = 2, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },

                    // =============================================
                    // RAU CỦ (nguyên liệu tươi)
                    // =============================================
                    new FoodItem { Name = "Bông cải xanh", CaloriesPerUnit = 34, ProteinPerUnit = 2.8, CarbsPerUnit = 7, FatPerUnit = 0.4, EstimatedPrice = 10000, Unit = "100g", Category = 3, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new FoodItem { Name = "Bông cải trắng", CaloriesPerUnit = 25, ProteinPerUnit = 1.9, CarbsPerUnit = 5, FatPerUnit = 0.3, EstimatedPrice = 10000, Unit = "100g", Category = 3, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new FoodItem { Name = "Rau chân vịt (Spinach)", CaloriesPerUnit = 23, ProteinPerUnit = 2.9, CarbsPerUnit = 3.6, FatPerUnit = 0.4, EstimatedPrice = 12000, Unit = "100g", Category = 3, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new FoodItem { Name = "Cà chua", CaloriesPerUnit = 18, ProteinPerUnit = 0.9, CarbsPerUnit = 3.9, FatPerUnit = 0.2, EstimatedPrice = 6000, Unit = "100g", Category = 3, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new FoodItem { Name = "Dưa chuột", CaloriesPerUnit = 16, ProteinPerUnit = 0.7, CarbsPerUnit = 3.6, FatPerUnit = 0.1, EstimatedPrice = 5000, Unit = "100g", Category = 3, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new FoodItem { Name = "Cà rốt", CaloriesPerUnit = 41, ProteinPerUnit = 0.9, CarbsPerUnit = 10, FatPerUnit = 0.2, EstimatedPrice = 5000, Unit = "100g", Category = 3, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new FoodItem { Name = "Rau muống", CaloriesPerUnit = 19, ProteinPerUnit = 2.6, CarbsPerUnit = 3.1, FatPerUnit = 0.2, EstimatedPrice = 4000, Unit = "100g", Category = 3, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new FoodItem { Name = "Rau cải ngọt", CaloriesPerUnit = 12, ProteinPerUnit = 1.5, CarbsPerUnit = 1.2, FatPerUnit = 0.2, EstimatedPrice = 4000, Unit = "100g", Category = 3, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new FoodItem { Name = "Rau mồng tơi", CaloriesPerUnit = 19, ProteinPerUnit = 1.8, CarbsPerUnit = 3.4, FatPerUnit = 0.3, EstimatedPrice = 4000, Unit = "100g", Category = 3, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new FoodItem { Name = "Bắp cải", CaloriesPerUnit = 25, ProteinPerUnit = 1.3, CarbsPerUnit = 5.8, FatPerUnit = 0.1, EstimatedPrice = 5000, Unit = "100g", Category = 3, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new FoodItem { Name = "Cải bó xôi (Kale)", CaloriesPerUnit = 35, ProteinPerUnit = 2.9, CarbsPerUnit = 4.4, FatPerUnit = 1.5, EstimatedPrice = 15000, Unit = "100g", Category = 3, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new FoodItem { Name = "Đậu bắp", CaloriesPerUnit = 33, ProteinPerUnit = 2.0, CarbsPerUnit = 7, FatPerUnit = 0.2, EstimatedPrice = 6000, Unit = "100g", Category = 3, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new FoodItem { Name = "Bí đỏ", CaloriesPerUnit = 26, ProteinPerUnit = 1.0, CarbsPerUnit = 6.5, FatPerUnit = 0.1, EstimatedPrice = 5000, Unit = "100g", Category = 3, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new FoodItem { Name = "Bí xanh", CaloriesPerUnit = 12, ProteinPerUnit = 0.6, CarbsPerUnit = 2.7, FatPerUnit = 0.1, EstimatedPrice = 4000, Unit = "100g", Category = 3, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new FoodItem { Name = "Mướp", CaloriesPerUnit = 20, ProteinPerUnit = 0.6, CarbsPerUnit = 4.4, FatPerUnit = 0.2, EstimatedPrice = 4000, Unit = "100g", Category = 3, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new FoodItem { Name = "Khổ qua (mướp đắng)", CaloriesPerUnit = 17, ProteinPerUnit = 1.0, CarbsPerUnit = 3.7, FatPerUnit = 0.2, EstimatedPrice = 6000, Unit = "100g", Category = 3, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new FoodItem { Name = "Giá đỗ", CaloriesPerUnit = 31, ProteinPerUnit = 3.0, CarbsPerUnit = 6, FatPerUnit = 0.2, EstimatedPrice = 3000, Unit = "100g", Category = 3, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new FoodItem { Name = "Nấm rơm", CaloriesPerUnit = 22, ProteinPerUnit = 2.5, CarbsPerUnit = 3.3, FatPerUnit = 0.3, EstimatedPrice = 12000, Unit = "100g", Category = 3, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new FoodItem { Name = "Nấm đùi gà", CaloriesPerUnit = 33, ProteinPerUnit = 3.3, CarbsPerUnit = 5.0, FatPerUnit = 0.4, EstimatedPrice = 15000, Unit = "100g", Category = 3, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new FoodItem { Name = "Ớt chuông", CaloriesPerUnit = 31, ProteinPerUnit = 1.0, CarbsPerUnit = 6.0, FatPerUnit = 0.3, EstimatedPrice = 12000, Unit = "100g", Category = 3, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new FoodItem { Name = "Hành tây", CaloriesPerUnit = 40, ProteinPerUnit = 1.1, CarbsPerUnit = 9.3, FatPerUnit = 0.1, EstimatedPrice = 5000, Unit = "100g", Category = 3, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new FoodItem { Name = "Rau thơm các loại", CaloriesPerUnit = 25, ProteinPerUnit = 2.0, CarbsPerUnit = 4.0, FatPerUnit = 0.5, EstimatedPrice = 3000, Unit = "100g", Category = 3, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },

                    // =============================================
                    // TRÁI CÂY
                    // =============================================
                    new FoodItem { Name = "Chuối", CaloriesPerUnit = 89, ProteinPerUnit = 1.1, CarbsPerUnit = 23, FatPerUnit = 0.3, EstimatedPrice = 5000, Unit = "quả", Category = 4, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new FoodItem { Name = "Táo", CaloriesPerUnit = 52, ProteinPerUnit = 0.3, CarbsPerUnit = 14, FatPerUnit = 0.2, EstimatedPrice = 10000, Unit = "quả", Category = 4, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new FoodItem { Name = "Bơ (Avocado)", CaloriesPerUnit = 160, ProteinPerUnit = 2, CarbsPerUnit = 9, FatPerUnit = 15, EstimatedPrice = 15000, Unit = "100g", Category = 4, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new FoodItem { Name = "Cam", CaloriesPerUnit = 47, ProteinPerUnit = 0.9, CarbsPerUnit = 12, FatPerUnit = 0.1, EstimatedPrice = 6000, Unit = "quả", Category = 4, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new FoodItem { Name = "Thanh long", CaloriesPerUnit = 50, ProteinPerUnit = 1.1, CarbsPerUnit = 11, FatPerUnit = 0.4, EstimatedPrice = 8000, Unit = "quả", Category = 4, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new FoodItem { Name = "Ổi", CaloriesPerUnit = 68, ProteinPerUnit = 2.6, CarbsPerUnit = 14, FatPerUnit = 1.0, EstimatedPrice = 5000, Unit = "quả", Category = 4, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new FoodItem { Name = "Đu đủ", CaloriesPerUnit = 43, ProteinPerUnit = 0.5, CarbsPerUnit = 11, FatPerUnit = 0.3, EstimatedPrice = 5000, Unit = "100g", Category = 4, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new FoodItem { Name = "Dưa hấu", CaloriesPerUnit = 30, ProteinPerUnit = 0.6, CarbsPerUnit = 8, FatPerUnit = 0.2, EstimatedPrice = 3000, Unit = "100g", Category = 4, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new FoodItem { Name = "Kiwi", CaloriesPerUnit = 61, ProteinPerUnit = 1.1, CarbsPerUnit = 15, FatPerUnit = 0.5, EstimatedPrice = 12000, Unit = "quả", Category = 4, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new FoodItem { Name = "Dâu tây", CaloriesPerUnit = 32, ProteinPerUnit = 0.7, CarbsPerUnit = 7.7, FatPerUnit = 0.3, EstimatedPrice = 15000, Unit = "100g", Category = 4, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new FoodItem { Name = "Việt quất (Blueberry)", CaloriesPerUnit = 57, ProteinPerUnit = 0.7, CarbsPerUnit = 14, FatPerUnit = 0.3, EstimatedPrice = 25000, Unit = "100g", Category = 4, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new FoodItem { Name = "Xoài cát", CaloriesPerUnit = 60, ProteinPerUnit = 0.8, CarbsPerUnit = 15, FatPerUnit = 0.4, EstimatedPrice = 8000, Unit = "100g", Category = 4, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },

                    // =============================================
                    // CHẤT BÉO LÀNH MẠNH / HẠT
                    // =============================================
                    new FoodItem { Name = "Bơ đậu phộng", CaloriesPerUnit = 588, ProteinPerUnit = 25, CarbsPerUnit = 20, FatPerUnit = 50, EstimatedPrice = 20000, Unit = "100g", Category = 5, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new FoodItem { Name = "Dầu olive", CaloriesPerUnit = 884, ProteinPerUnit = 0, CarbsPerUnit = 0, FatPerUnit = 100, EstimatedPrice = 30000, Unit = "100ml", Category = 5, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new FoodItem { Name = "Hạt chia", CaloriesPerUnit = 486, ProteinPerUnit = 17, CarbsPerUnit = 42, FatPerUnit = 31, EstimatedPrice = 25000, Unit = "100g", Category = 5, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new FoodItem { Name = "Hạt hạnh nhân", CaloriesPerUnit = 579, ProteinPerUnit = 21, CarbsPerUnit = 22, FatPerUnit = 50, EstimatedPrice = 35000, Unit = "100g", Category = 5, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new FoodItem { Name = "Hạt óc chó", CaloriesPerUnit = 654, ProteinPerUnit = 15, CarbsPerUnit = 14, FatPerUnit = 65, EstimatedPrice = 40000, Unit = "100g", Category = 5, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new FoodItem { Name = "Hạt điều", CaloriesPerUnit = 553, ProteinPerUnit = 18, CarbsPerUnit = 30, FatPerUnit = 44, EstimatedPrice = 30000, Unit = "100g", Category = 5, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new FoodItem { Name = "Hạt bí", CaloriesPerUnit = 559, ProteinPerUnit = 30, CarbsPerUnit = 11, FatPerUnit = 49, EstimatedPrice = 20000, Unit = "100g", Category = 5, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new FoodItem { Name = "Hạt lanh", CaloriesPerUnit = 534, ProteinPerUnit = 18, CarbsPerUnit = 29, FatPerUnit = 42, EstimatedPrice = 18000, Unit = "100g", Category = 5, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new FoodItem { Name = "Hạt mắc ca", CaloriesPerUnit = 718, ProteinPerUnit = 8, CarbsPerUnit = 14, FatPerUnit = 76, EstimatedPrice = 50000, Unit = "100g", Category = 5, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new FoodItem { Name = "Đậu phộng", CaloriesPerUnit = 567, ProteinPerUnit = 26, CarbsPerUnit = 16, FatPerUnit = 49, EstimatedPrice = 10000, Unit = "100g", Category = 5, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },

                    // =============================================
                    // ĐỒ UỐNG
                    // =============================================
                    new FoodItem { Name = "Sữa tươi không đường", CaloriesPerUnit = 42, ProteinPerUnit = 3.4, CarbsPerUnit = 5, FatPerUnit = 1, EstimatedPrice = 8000, Unit = "100ml", Category = 6, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new FoodItem { Name = "Sữa đậu nành không đường", CaloriesPerUnit = 33, ProteinPerUnit = 2.9, CarbsPerUnit = 1.7, FatPerUnit = 1.8, EstimatedPrice = 6000, Unit = "100ml", Category = 6, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new FoodItem { Name = "Sữa hạnh nhân", CaloriesPerUnit = 15, ProteinPerUnit = 0.6, CarbsPerUnit = 0.3, FatPerUnit = 1.2, EstimatedPrice = 12000, Unit = "100ml", Category = 6, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new FoodItem { Name = "Whey Protein (scoop)", CaloriesPerUnit = 120, ProteinPerUnit = 24, CarbsPerUnit = 3, FatPerUnit = 1.5, EstimatedPrice = 15000, Unit = "scoop", Category = 6, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new FoodItem { Name = "Nước dừa tươi", CaloriesPerUnit = 19, ProteinPerUnit = 0.7, CarbsPerUnit = 3.7, FatPerUnit = 0.2, EstimatedPrice = 10000, Unit = "100ml", Category = 6, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new FoodItem { Name = "Trà xanh không đường", CaloriesPerUnit = 1, ProteinPerUnit = 0, CarbsPerUnit = 0, FatPerUnit = 0, EstimatedPrice = 5000, Unit = "250ml", Category = 6, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },

                    // =============================================
                    // GIA VỊ / DẦU NẤU ĂN
                    // =============================================
                    new FoodItem { Name = "Nước mắm", CaloriesPerUnit = 9, ProteinPerUnit = 1.4, CarbsPerUnit = 0.8, FatPerUnit = 0, EstimatedPrice = 1000, Unit = "15ml", Category = 7, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new FoodItem { Name = "Dầu ăn (dầu mè/hướng dương)", CaloriesPerUnit = 120, ProteinPerUnit = 0, CarbsPerUnit = 0, FatPerUnit = 14, EstimatedPrice = 2000, Unit = "15ml", Category = 7, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new FoodItem { Name = "Mật ong", CaloriesPerUnit = 304, ProteinPerUnit = 0.3, CarbsPerUnit = 82, FatPerUnit = 0, EstimatedPrice = 15000, Unit = "100g", Category = 7, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new FoodItem { Name = "Tỏi", CaloriesPerUnit = 149, ProteinPerUnit = 6.4, CarbsPerUnit = 33, FatPerUnit = 0.5, EstimatedPrice = 5000, Unit = "100g", Category = 7, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new FoodItem { Name = "Gừng", CaloriesPerUnit = 80, ProteinPerUnit = 1.8, CarbsPerUnit = 18, FatPerUnit = 0.8, EstimatedPrice = 5000, Unit = "100g", Category = 7, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new FoodItem { Name = "Sả", CaloriesPerUnit = 99, ProteinPerUnit = 1.8, CarbsPerUnit = 25, FatPerUnit = 0.5, EstimatedPrice = 3000, Unit = "100g", Category = 7, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new FoodItem { Name = "Hành tím", CaloriesPerUnit = 40, ProteinPerUnit = 1.1, CarbsPerUnit = 9.3, FatPerUnit = 0.1, EstimatedPrice = 5000, Unit = "100g", Category = 7, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new FoodItem { Name = "Giấm táo", CaloriesPerUnit = 21, ProteinPerUnit = 0, CarbsPerUnit = 0.9, FatPerUnit = 0, EstimatedPrice = 5000, Unit = "100ml", Category = 7, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                };
                await _context.FoodItems.AddRangeAsync(foods);
                await _context.SaveChangesAsync();

                // Tạo cảnh báo sức khỏe (FoodHealthWarnings)
                var medicalConditions = await _context.MedicalConditions.ToListAsync();
                var foodItems = await _context.FoodItems.ToListAsync();

                Action<string, string> LinkWarning = (foodName, conditionName) =>
                {
                    var food = foodItems.FirstOrDefault(f => f.Name == foodName);
                    var condition = medicalConditions.FirstOrDefault(c => c.ConditionName == conditionName);
                    if (food != null && condition != null)
                    {
                        food.Conditions.Add(condition);
                    }
                };

                // Dị ứng đậu phộng
                LinkWarning("Bơ đậu phộng", "Dị ứng đậu phộng");
                LinkWarning("Đậu phộng", "Dị ứng đậu phộng");

                // Dị ứng hải sản
                LinkWarning("Tôm sú", "Dị ứng hải sản");
                LinkWarning("Cá hồi (fillet)", "Dị ứng hải sản");
                LinkWarning("Cá ngừ đóng hộp", "Dị ứng hải sản");
                LinkWarning("Cá basa (fillet)", "Dị ứng hải sản");
                LinkWarning("Cá thu", "Dị ứng hải sản");
                LinkWarning("Cá diêu hồng", "Dị ứng hải sản");
                LinkWarning("Cá lóc", "Dị ứng hải sản");
                LinkWarning("Mực ống", "Dị ứng hải sản");
                LinkWarning("Nghêu", "Dị ứng hải sản");

                // Dị ứng sữa / Lactose
                LinkWarning("Sữa tươi không đường", "Dị ứng sữa / Lactose");
                LinkWarning("Sữa chua Hy Lạp", "Dị ứng sữa / Lactose");
                LinkWarning("Sữa chua không đường", "Dị ứng sữa / Lactose");
                LinkWarning("Whey Protein (scoop)", "Dị ứng sữa / Lactose");
                LinkWarning("Phô mai Cottage", "Dị ứng sữa / Lactose");

                // Dị ứng trứng
                LinkWarning("Trứng gà", "Dị ứng trứng");
                LinkWarning("Trứng vịt", "Dị ứng trứng");
                LinkWarning("Trứng cút", "Dị ứng trứng");

                // Dị ứng đậu nành
                LinkWarning("Đậu hũ trắng", "Dị ứng đậu nành");
                LinkWarning("Đậu hũ non", "Dị ứng đậu nành");
                LinkWarning("Đậu nành (Edamame)", "Dị ứng đậu nành");
                LinkWarning("Tempeh", "Dị ứng đậu nành");
                LinkWarning("Sữa đậu nành không đường", "Dị ứng đậu nành");

                // Dị ứng gluten (Celiac)
                LinkWarning("Bánh mì trắng", "Dị ứng gluten (Celiac)");
                LinkWarning("Bánh mì nguyên cám", "Dị ứng gluten (Celiac)");
                LinkWarning("Yến mạch (Oat)", "Dị ứng gluten (Celiac)");
                LinkWarning("Phở tươi (bánh phở)", "Dị ứng gluten (Celiac)");
                LinkWarning("Mì ý (Pasta)", "Dị ứng gluten (Celiac)");

                // Bệnh gout (tránh purine cao)
                LinkWarning("Thịt bò nạc", "Bệnh gout");
                LinkWarning("Thịt bò bắp", "Bệnh gout");
                LinkWarning("Tôm sú", "Bệnh gout");
                LinkWarning("Mực ống", "Bệnh gout");
                LinkWarning("Nghêu", "Bệnh gout");
                LinkWarning("Sườn heo", "Bệnh gout");

                // Bệnh thận mãn tính (tránh protein/kali cao)
                LinkWarning("Whey Protein (scoop)", "Bệnh thận mãn tính");
                LinkWarning("Chuối", "Bệnh thận mãn tính");
                LinkWarning("Bơ (Avocado)", "Bệnh thận mãn tính");

                // Người ăn chay (Vegan) — loại bỏ nguyên liệu động vật
                LinkWarning("Ức gà", "Người ăn chay (Vegan)");
                LinkWarning("Đùi gà (bỏ da)", "Người ăn chay (Vegan)");
                LinkWarning("Cánh gà", "Người ăn chay (Vegan)");
                LinkWarning("Thịt bò nạc", "Người ăn chay (Vegan)");
                LinkWarning("Thịt bò bắp", "Người ăn chay (Vegan)");
                LinkWarning("Thịt heo nạc vai", "Người ăn chay (Vegan)");
                LinkWarning("Thịt heo băm nạc", "Người ăn chay (Vegan)");
                LinkWarning("Sườn heo", "Người ăn chay (Vegan)");
                LinkWarning("Trứng gà", "Người ăn chay (Vegan)");
                LinkWarning("Trứng vịt", "Người ăn chay (Vegan)");
                LinkWarning("Trứng cút", "Người ăn chay (Vegan)");
                LinkWarning("Cá hồi (fillet)", "Người ăn chay (Vegan)");
                LinkWarning("Cá basa (fillet)", "Người ăn chay (Vegan)");
                LinkWarning("Cá thu", "Người ăn chay (Vegan)");
                LinkWarning("Cá ngừ đóng hộp", "Người ăn chay (Vegan)");
                LinkWarning("Cá diêu hồng", "Người ăn chay (Vegan)");
                LinkWarning("Cá lóc", "Người ăn chay (Vegan)");
                LinkWarning("Tôm sú", "Người ăn chay (Vegan)");
                LinkWarning("Mực ống", "Người ăn chay (Vegan)");
                LinkWarning("Nghêu", "Người ăn chay (Vegan)");
                LinkWarning("Sữa tươi không đường", "Người ăn chay (Vegan)");
                LinkWarning("Sữa chua Hy Lạp", "Người ăn chay (Vegan)");
                LinkWarning("Sữa chua không đường", "Người ăn chay (Vegan)");
                LinkWarning("Whey Protein (scoop)", "Người ăn chay (Vegan)");
                LinkWarning("Phô mai Cottage", "Người ăn chay (Vegan)");

                await _context.SaveChangesAsync();
            }

            // =========================================================
            // 3. Seed Exercises (Bài tập)
            // =========================================================
            var existingExercises = await _context.Exercises.Select(e => e.Name.ToLower()).ToListAsync();
            var exercisesToSeed = new List<Exercise>
            {
                // Bodyweight & Core
                new Exercise { Name = "Plank", MuscleGroup = "Core", BaseCaloriesBurned = 25, InstructionUrl = "https://youtube.com/...", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Lunge cơ bản", MuscleGroup = "Chân, Mông", BaseCaloriesBurned = 55, InstructionUrl = "https://youtube.com/...", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Jump Squat", MuscleGroup = "Chân, Mông, Cardio", BaseCaloriesBurned = 85, InstructionUrl = "https://youtube.com/...", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Mountain Climbers", MuscleGroup = "Core, Cardio", BaseCaloriesBurned = 65, InstructionUrl = "https://youtube.com/...", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Russian Twist", MuscleGroup = "Core", BaseCaloriesBurned = 35, InstructionUrl = "https://youtube.com/...", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Glute Bridge", MuscleGroup = "Mông, Lưng dưới", BaseCaloriesBurned = 40, InstructionUrl = "https://youtube.com/...", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Tricep Dips", MuscleGroup = "Triceps, Ngực", BaseCaloriesBurned = 40, InstructionUrl = "https://youtube.com/...", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Pull Up (Xà đơn)", MuscleGroup = "Lưng, Biceps", BaseCaloriesBurned = 70, InstructionUrl = "https://youtube.com/...", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Chin Up", MuscleGroup = "Biceps, Lưng", BaseCaloriesBurned = 65, InstructionUrl = "https://youtube.com/...", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Push Up", MuscleGroup = "Ngực, Vai", BaseCaloriesBurned = 50, InstructionUrl = "https://youtube.com/...", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Gập bụng (Crunch)", MuscleGroup = "Core", BaseCaloriesBurned = 30, InstructionUrl = "https://youtube.com/...", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Squat cơ bản", MuscleGroup = "Chân, Mông", BaseCaloriesBurned = 70, InstructionUrl = "https://youtube.com/...", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },

                // Bodyweight nâng cao (34-41)
                new Exercise { Name = "Superman Hold", MuscleGroup = "Lưng dưới, Mông", BaseCaloriesBurned = 25, InstructionUrl = "https://youtube.com/...", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Bicycle Crunch", MuscleGroup = "Core, Bụng bên", BaseCaloriesBurned = 40, InstructionUrl = "https://youtube.com/...", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Diamond Push Up", MuscleGroup = "Tay sau, Ngực", BaseCaloriesBurned = 55, InstructionUrl = "https://youtube.com/...", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Inchworm", MuscleGroup = "Toàn thân, Core", BaseCaloriesBurned = 45, InstructionUrl = "https://youtube.com/...", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Bear Crawl", MuscleGroup = "Toàn thân, Core", BaseCaloriesBurned = 70, InstructionUrl = "https://youtube.com/...", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Flutter Kicks", MuscleGroup = "Bụng dưới, Core", BaseCaloriesBurned = 35, InstructionUrl = "https://youtube.com/...", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Heel Touches", MuscleGroup = "Bụng bên, Core", BaseCaloriesBurned = 30, InstructionUrl = "https://youtube.com/...", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Plank sườn", MuscleGroup = "Bụng bên, Core", BaseCaloriesBurned = 30, InstructionUrl = "https://youtube.com/...", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },

                // Cardio & HIIT (13-18)
                new Exercise { Name = "Burpee", MuscleGroup = "Toàn thân, Cardio", BaseCaloriesBurned = 100, InstructionUrl = "https://youtube.com/...", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Jumping Jacks", MuscleGroup = "Cardio, Toàn thân", BaseCaloriesBurned = 60, InstructionUrl = "https://youtube.com/...", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Chạy bộ nhẹ (30 phút)", MuscleGroup = "Cardio, Chân", BaseCaloriesBurned = 250, InstructionUrl = "https://youtube.com/...", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Đạp xe (30 phút)", MuscleGroup = "Chân, Cardio", BaseCaloriesBurned = 200, InstructionUrl = "https://youtube.com/...", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Nhảy dây (15 phút)", MuscleGroup = "Cardio, Bắp chân", BaseCaloriesBurned = 180, InstructionUrl = "https://youtube.com/...", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Leo cầu thang", MuscleGroup = "Chân, Cardio", BaseCaloriesBurned = 90, InstructionUrl = "https://youtube.com/...", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },

                // Cardio nâng cao (42-48)
                new Exercise { Name = "High Knees", MuscleGroup = "Cardio, Chân", BaseCaloriesBurned = 75, InstructionUrl = "https://youtube.com/...", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Butt Kicks", MuscleGroup = "Cardio, Chân", BaseCaloriesBurned = 55, InstructionUrl = "https://youtube.com/...", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Skaters", MuscleGroup = "Cardio, Chân, Mông", BaseCaloriesBurned = 80, InstructionUrl = "https://youtube.com/...", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Plank Jacks", MuscleGroup = "Cardio, Core", BaseCaloriesBurned = 50, InstructionUrl = "https://youtube.com/...", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Shadow Boxing", MuscleGroup = "Cardio, Vai, Tay", BaseCaloriesBurned = 60, InstructionUrl = "https://youtube.com/...", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Kettlebell Swing", MuscleGroup = "Cardio, Toàn thân, Mông", BaseCaloriesBurned = 90, InstructionUrl = "https://youtube.com/...", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Chạy nước rút", MuscleGroup = "Cardio, Chân", BaseCaloriesBurned = 120, InstructionUrl = "https://youtube.com/...", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },

                // Tạ đơn (Dumbbell) (19-24)
                new Exercise { Name = "Dumbbell Goblet Squat", MuscleGroup = "Chân, Mông", BaseCaloriesBurned = 75, InstructionUrl = "https://youtube.com/...", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Dumbbell Romanian Deadlift", MuscleGroup = "Đùi sau, Lưng dưới", BaseCaloriesBurned = 80, InstructionUrl = "https://youtube.com/...", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Dumbbell Shoulder Press", MuscleGroup = "Vai, Triceps", BaseCaloriesBurned = 45, InstructionUrl = "https://youtube.com/...", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Dumbbell Bicep Curls", MuscleGroup = "Bắp tay trước", BaseCaloriesBurned = 30, InstructionUrl = "https://youtube.com/...", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Dumbbell Chest Press", MuscleGroup = "Ngực, Triceps", BaseCaloriesBurned = 50, InstructionUrl = "https://youtube.com/...", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Dumbbell Bent-over Row", MuscleGroup = "Lưng giữa, Biceps", BaseCaloriesBurned = 55, InstructionUrl = "https://youtube.com/...", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },

                // Dumbbell nâng cao (49-59)
                new Exercise { Name = "Dumbbell Hammer Curls", MuscleGroup = "Bắp tay trước", BaseCaloriesBurned = 30, InstructionUrl = "https://youtube.com/...", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Dumbbell Tricep Kickback", MuscleGroup = "Tay sau", BaseCaloriesBurned = 35, InstructionUrl = "https://youtube.com/...", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Dang tạ bên", MuscleGroup = "Vai", BaseCaloriesBurned = 30, InstructionUrl = "https://youtube.com/...", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Nâng tạ trước", MuscleGroup = "Vai", BaseCaloriesBurned = 30, InstructionUrl = "https://youtube.com/...", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Dumbbell Rear Delt Fly", MuscleGroup = "Vai, Lưng", BaseCaloriesBurned = 35, InstructionUrl = "https://youtube.com/...", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Dumbbell Overhead Extension", MuscleGroup = "Tay sau", BaseCaloriesBurned = 40, InstructionUrl = "https://youtube.com/...", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Bulgarian Split Squat tạ đơn", MuscleGroup = "Chân, Mông", BaseCaloriesBurned = 80, InstructionUrl = "https://youtube.com/...", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Dumbbell Incline Bench Press", MuscleGroup = "Ngực, Vai", BaseCaloriesBurned = 55, InstructionUrl = "https://youtube.com/...", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Nhón chân tạ đơn", MuscleGroup = "Bắp chân", BaseCaloriesBurned = 25, InstructionUrl = "https://youtube.com/...", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Dumbbell Floor Press", MuscleGroup = "Ngực, Tay sau", BaseCaloriesBurned = 45, InstructionUrl = "https://youtube.com/...", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Nhún vai tạ đơn", MuscleGroup = "Vai, Cầu vai", BaseCaloriesBurned = 30, InstructionUrl = "https://youtube.com/...", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },

                // Máy phòng Gym (25-29)
                new Exercise { Name = "Leg Press", MuscleGroup = "Chân, Mông", BaseCaloriesBurned = 90, InstructionUrl = "https://youtube.com/...", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Leg Extension", MuscleGroup = "Đùi trước", BaseCaloriesBurned = 45, InstructionUrl = "https://youtube.com/...", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Cable Chest Fly", MuscleGroup = "Ngực", BaseCaloriesBurned = 35, InstructionUrl = "https://youtube.com/...", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Lat Pulldown", MuscleGroup = "Lưng, Biceps", BaseCaloriesBurned = 50, InstructionUrl = "https://youtube.com/...", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Smith Machine Squat", MuscleGroup = "Chân, Mông", BaseCaloriesBurned = 80, InstructionUrl = "https://youtube.com/...", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },

                // Máy phòng Gym nâng cao (60-68)
                new Exercise { Name = "Seated Cable Row", MuscleGroup = "Lưng, Biceps", BaseCaloriesBurned = 55, InstructionUrl = "https://youtube.com/...", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Ép ngực bằng máy", MuscleGroup = "Ngực", BaseCaloriesBurned = 40, InstructionUrl = "https://youtube.com/...", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Leg Curl Machine", MuscleGroup = "Đùi sau", BaseCaloriesBurned = 45, InstructionUrl = "https://youtube.com/...", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Kéo cáp tay sau", MuscleGroup = "Tay sau", BaseCaloriesBurned = 35, InstructionUrl = "https://youtube.com/...", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Bicep Cable Curl", MuscleGroup = "Bắp tay trước", BaseCaloriesBurned = 30, InstructionUrl = "https://youtube.com/...", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Cable Lateral Raise", MuscleGroup = "Vai", BaseCaloriesBurned = 30, InstructionUrl = "https://youtube.com/...", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Calf Press on Leg Press", MuscleGroup = "Bắp chân", BaseCaloriesBurned = 35, InstructionUrl = "https://youtube.com/...", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Assisted Pull-up Machine", MuscleGroup = "Lưng, Biceps", BaseCaloriesBurned = 60, InstructionUrl = "https://youtube.com/...", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Kéo cáp cơ vai sau", MuscleGroup = "Vai, Lưng", BaseCaloriesBurned = 35, InstructionUrl = "https://youtube.com/...", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },

                // Yoga & Giãn cơ (30-33)
                new Exercise { Name = "Chào mặt trời (Sun Salutation)", MuscleGroup = "Toàn thân, Linh hoạt", BaseCaloriesBurned = 20, InstructionUrl = "https://youtube.com/...", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Tư thế chó úp mặt (Downward Dog)", MuscleGroup = "Lưng, Chân", BaseCaloriesBurned = 15, InstructionUrl = "https://youtube.com/...", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Cat-Cow Stretch", MuscleGroup = "Lưng, Cột sống", BaseCaloriesBurned = 10, InstructionUrl = "https://youtube.com/...", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Child's Pose", MuscleGroup = "Lưng dưới, Vai", BaseCaloriesBurned = 10, InstructionUrl = "https://youtube.com/...", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },

                // Yoga nâng cao (69-76)
                new Exercise { Name = "Tư thế rắn hổ mang", MuscleGroup = "Lưng, Cột sống", BaseCaloriesBurned = 12, InstructionUrl = "https://youtube.com/...", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Tư thế chiến binh II", MuscleGroup = "Chân, Linh hoạt", BaseCaloriesBurned = 18, InstructionUrl = "https://youtube.com/...", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Tư thế bồ câu", MuscleGroup = "Mông, Linh hoạt", BaseCaloriesBurned = 15, InstructionUrl = "https://youtube.com/...", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Tư thế cái cây", MuscleGroup = "Chân, Linh hoạt", BaseCaloriesBurned = 10, InstructionUrl = "https://youtube.com/...", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Tư thế cây cầu", MuscleGroup = "Mông, Lưng dưới", BaseCaloriesBurned = 20, InstructionUrl = "https://youtube.com/...", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Tư thế trăng lưỡi liềm", MuscleGroup = "Chân, Linh hoạt", BaseCaloriesBurned = 16, InstructionUrl = "https://youtube.com/...", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Tư thế em bé vui vẻ", MuscleGroup = "Lưng dưới, Linh hoạt", BaseCaloriesBurned = 10, InstructionUrl = "https://youtube.com/...", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Tư thế xỏ chỉ", MuscleGroup = "Vai, Lưng", BaseCaloriesBurned = 10, InstructionUrl = "https://youtube.com/...", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Gập bụng ngược (Reverse Crunch)", MuscleGroup = "Core", BaseCaloriesBurned = 30, InstructionUrl = "https://youtube.com/.../test", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Hít đất vỗ tay (Clap Push Up)", MuscleGroup = "Ngực, Vai", BaseCaloriesBurned = 65, InstructionUrl = "https://youtube.com/.../test", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Plank đi ngang (Moving Plank)", MuscleGroup = "Core, Vai", BaseCaloriesBurned = 35, InstructionUrl = "https://youtube.com/.../test", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Tư thế Hollow Body Hold", MuscleGroup = "Bụng, Core", BaseCaloriesBurned = 25, InstructionUrl = "https://youtube.com/.../test", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Hít đất dốc xuống (Decline Push Up)", MuscleGroup = "Ngực trên, Vai", BaseCaloriesBurned = 55, InstructionUrl = "https://youtube.com/.../test", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Hít đất dốc lên (Incline Push Up)", MuscleGroup = "Ngực dưới", BaseCaloriesBurned = 45, InstructionUrl = "https://youtube.com/.../test", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Tư thế Dead Bug", MuscleGroup = "Core, Bụng dưới", BaseCaloriesBurned = 25, InstructionUrl = "https://youtube.com/.../test", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Lunge nhảy đổi bên (Jumping Lunges)", MuscleGroup = "Chân, Mông", BaseCaloriesBurned = 75, InstructionUrl = "https://youtube.com/.../test", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Nằm nhấc chân kép (Double Leg Raise)", MuscleGroup = "Bụng dưới, Core", BaseCaloriesBurned = 35, InstructionUrl = "https://youtube.com/.../test", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Hít đất Archer (Archer Push Up)", MuscleGroup = "Ngực, Tay sau", BaseCaloriesBurned = 60, InstructionUrl = "https://youtube.com/.../test", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Superman kiêm bơi sải (Superman Swim)", MuscleGroup = "Lưng dưới, Vai", BaseCaloriesBurned = 30, InstructionUrl = "https://youtube.com/.../test", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Co gối chạm cùi chỏ (Plank Knee to Elbow)", MuscleGroup = "Bụng bên, Core", BaseCaloriesBurned = 35, InstructionUrl = "https://youtube.com/.../test", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Squat khuỵu chéo chân (Curtsy Squat)", MuscleGroup = "Mông, Đùi ngoài", BaseCaloriesBurned = 60, InstructionUrl = "https://youtube.com/.../test", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Plank leo núi chéo chân (Cross Mountain Climber)", MuscleGroup = "Core, Bụng bên", BaseCaloriesBurned = 45, InstructionUrl = "https://youtube.com/.../test", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Hít đất Pike (Pike Push Up)", MuscleGroup = "Vai, Tay sau", BaseCaloriesBurned = 50, InstructionUrl = "https://youtube.com/.../test", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Tư thế Bird Dog", MuscleGroup = "Lưng, Mông, Core", BaseCaloriesBurned = 20, InstructionUrl = "https://youtube.com/.../test", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Squat nhún sâu (Pulse Squat)", MuscleGroup = "Chân, Mông", BaseCaloriesBurned = 65, InstructionUrl = "https://youtube.com/.../test", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Giữ tư thế ngồi xổm tựa tường (Wall Sit Hold)", MuscleGroup = "Đùi trước, Chân", BaseCaloriesBurned = 30, InstructionUrl = "https://youtube.com/.../test", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Plank chạm vai ngược (Reverse Plank Shoulder Taps)", MuscleGroup = "Core, Vai", BaseCaloriesBurned = 35, InstructionUrl = "https://youtube.com/.../test", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Hít đất giải phóng tay (Hand Release Push Up)", MuscleGroup = "Ngực, Vai", BaseCaloriesBurned = 55, InstructionUrl = "https://youtube.com/.../test", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Tư thế V-Up", MuscleGroup = "Core, Bụng dưới", BaseCaloriesBurned = 40, InstructionUrl = "https://youtube.com/.../test", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Nằm xoay chân vòng tròn (Leg Circles)", MuscleGroup = "Core, Bụng dưới", BaseCaloriesBurned = 30, InstructionUrl = "https://youtube.com/.../test", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Hít đất Hindú (Hindu Push Up)", MuscleGroup = "Vai, Ngực, Tay sau", BaseCaloriesBurned = 60, InstructionUrl = "https://youtube.com/.../test", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Đá chân nhún mông (Donkey Kicks)", MuscleGroup = "Mông", BaseCaloriesBurned = 40, InstructionUrl = "https://youtube.com/.../test", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Nằm nghiêng đá chân (Side Leg Raises)", MuscleGroup = "Đùi ngoài, Mông", BaseCaloriesBurned = 30, InstructionUrl = "https://youtu.be/Z_0p0I8B4EU?si=Ib9n3otOefMf1voB", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Cào cào cất cánh (Locust Pose)", MuscleGroup = "Lưng dưới", BaseCaloriesBurned = 20, InstructionUrl = "https://youtu.be/4I5EFtmDAiY?si=PmF7OGeHN8Wcfcz8", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Nhún mông sườn (Bridge Pulses)", MuscleGroup = "Mông, Đùi sau", BaseCaloriesBurned = 45, InstructionUrl = "https://youtu.be/9qo48CYN06w?si=_eHpOxKAg50f_vxt", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Hít đất sườn (Side Push Up)", MuscleGroup = "Tay sau, Ngực", BaseCaloriesBurned = 45, InstructionUrl = "https://youtu.be/TGQi4VsPIhU?si=uzJn54lMalqOiOq8", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Plank nhảy rút chân (Plank Tuck Jump)", MuscleGroup = "Core, Cardio", BaseCaloriesBurned = 60, InstructionUrl = "https://youtu.be/ERGzP1Wavq4?si=8OiGG1X_XUBT1mzM", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Gập bụng chữ X (X-Crunch)", MuscleGroup = "Core, Bụng bên", BaseCaloriesBurned = 35, InstructionUrl = "https://youtu.be/f_ZsJgaqFNE?si=tEq94NNt1jbxfl-f", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Nhảy cao co gối (Tuck Jump)", MuscleGroup = "Toàn thân, Cardio", BaseCaloriesBurned = 95, InstructionUrl = "https://youtu.be/3-5Vmrp3yGE?si=ns0c_eP3xppEp06o", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Chạy nâng cao đùi tại chỗ (High Knees Sprint)", MuscleGroup = "Cardio, Chân", BaseCaloriesBurned = 85, InstructionUrl = "https://youtu.be/1EELa6eDwrI?si=LDuGEHKwtK8guCqG", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Cardio Nhảy Jumping Jacks chéo tay (Seal Jacks)", MuscleGroup = "Cardio, Vai", BaseCaloriesBurned = 60, InstructionUrl = "https://youtu.be/tYt2TH72cbA?si=bpTNAnQeMz843iLN", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Burpee không hít đất (Half Burpee)", MuscleGroup = "Cardio, Toàn thân", BaseCaloriesBurned = 80, InstructionUrl = "https://youtu.be/JhG3ewDF0vA?si=MUyOiGlsROKTkP7S", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Nhảy ngang qua lại (Lateral Hops)", MuscleGroup = "Chân, Cardio", BaseCaloriesBurned = 65, InstructionUrl = "https://youtu.be/bqbZqxqs8tY?si=39JZp_jCdkysyV7z", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Leo núi đùi chạm ngực (Power Mountain Climbers)", MuscleGroup = "Cardio, Core", BaseCaloriesBurned = 70, InstructionUrl = "https://youtu.be/wQq3ybaLZeA?si=jTsTO54TPx8XH9e7", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "HIIT Nhảy dây tốc độ cao (Speed Rope)", MuscleGroup = "Cardio", BaseCaloriesBurned = 90, InstructionUrl = "https://youtu.be/CYGeazlNbU4?si=dI2MPC-c8mO7SmhK", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Chạy ziczac nhanh (Shuttle Run)", MuscleGroup = "Chân, Cardio", BaseCaloriesBurned = 85, InstructionUrl = "https://youtu.be/9pb6JX2ulDU?si=2-Yb2mI2BEfO-7Mu", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Tấm ván nhảy ngang (Lateral Plank Walks)", MuscleGroup = "Core, Cardio", BaseCaloriesBurned = 45, InstructionUrl = "https://youtu.be/yCVyaX-RjLM?si=ONbp0vmJ9HM7C6bN", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "HIIT Đấm móc nhanh (Shadow Boxing Uppercuts)", MuscleGroup = "Vai, Tay, Cardio", BaseCaloriesBurned = 65, InstructionUrl = "https://youtu.be/J4j3AOVWuHE?si=vz759hHXU6ZczvVo", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Nhảy bục gỗ (Box Jump)", MuscleGroup = "Chân, Mông, Cardio", BaseCaloriesBurned = 90, InstructionUrl = "https://youtu.be/k7dmYdknbac?si=scFvcbbdbcULz3db", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "HIIT Đẩy tạ Kettlebell ngang ngực", MuscleGroup = "Cardio, Toàn thân", BaseCaloriesBurned = 85, InstructionUrl = "https://youtu.be/0pYiTShXDfk?si=ypYA1QBa-ZgZXjys", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "HIIT Chạy đổi hướng (Agility Ladder drills)", MuscleGroup = "Cardio, Chân", BaseCaloriesBurned = 80, InstructionUrl = "https://youtu.be/l9zPxGy0fzI?si=_-nFXLKdWAbYm_4n", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Bật nhảy ếch (Frog Jumps)", MuscleGroup = "Chân, Mông, Cardio", BaseCaloriesBurned = 85, InstructionUrl = "https://youtu.be/C54LoE13DUo?si=-TC8FKZiLvnvX3K4", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Nhảy chạm ngón chân (Toe Taps)", MuscleGroup = "Cardio, Chân", BaseCaloriesBurned = 55, InstructionUrl = "https://youtu.be/tPCYZu7Seug?si=g1DYftSrzGWg7Lke", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Bật nhảy xoay người (180 Jump Squat)", MuscleGroup = "Chân, Mông, Cardio", BaseCaloriesBurned = 80, InstructionUrl = "https://youtu.be/ZPwfsf7vByE?si=AajsAPAKRd4P4vzM", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Cardio Leo cầu thang tốc độ cao", MuscleGroup = "Chân, Cardio", BaseCaloriesBurned = 95, InstructionUrl = "https://youtube.com/shorts/ofhGpzOYIac?si=NoX2Q6Debl-9pmae", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Chạy nâng chân sau chạm tay (Butt Kick Sprint)", MuscleGroup = "Cardio", BaseCaloriesBurned = 70, InstructionUrl = "https://youtu.be/vXVPvY1UbJI?si=Z5GNmCX_ZmfThkxf", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Cardio Đạp xe tốc độ cao (Spinning)", MuscleGroup = "Cardio, Chân", BaseCaloriesBurned = 110, InstructionUrl = "https://youtube.com/shorts/H246MZj5fCY?si=BevfSlbUrCBtaTxf", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "HIIT Nhảy Jacks liên tục (Power Jacks)", MuscleGroup = "Cardio, Toàn thân", BaseCaloriesBurned = 75, InstructionUrl = "https://youtu.be/SSy9BxywZgQ?si=5GjoPY8WwlG3jjo6", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Burpee kết hợp nhảy cao (Tuck Jump Burpee)", MuscleGroup = "Toàn thân, Cardio", BaseCaloriesBurned = 110, InstructionUrl = "https://youtu.be/kB917sFCaIo?si=ZebtkYI2-55lhKIJ", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Nhảy Skaters sâu chạm tay xuống sàn", MuscleGroup = "Cardio, Chân, Mông", BaseCaloriesBurned = 85, InstructionUrl = "https://youtube.com/.../test", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Nhảy dây lò cò 1 chân (Single Leg Jump Rope)", MuscleGroup = "Bắp chân, Cardio", BaseCaloriesBurned = 75, InstructionUrl = "https://youtu.be/4iPWOfmXIvA?si=Ojj1CYs2m8W3Ib5R", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Cardio Chạy việt dã (45 phút)", MuscleGroup = "Cardio, Chân", BaseCaloriesBurned = 380, InstructionUrl = "https://youtube.com/.../test", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Cardio Đạp xe leo dốc (30 phút)", MuscleGroup = "Chân, Cardio", BaseCaloriesBurned = 280, InstructionUrl = "https://youtube.com/.../test", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "HIIT Chạy nước rút đổi hướng", MuscleGroup = "Cardio, Chân", BaseCaloriesBurned = 105, InstructionUrl = "https://youtube.com/.../test", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Leo núi xoay hông (Spiderman Climbers)", MuscleGroup = "Core, Bụng bên, Cardio", BaseCaloriesBurned = 70, InstructionUrl = "https://youtu.be/wQq3ybaLZeA?si=rFSs2yPRdQ36DeL4", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Đấm gió Jab-Cross liên tục", MuscleGroup = "Cardio, Vai, Tay", BaseCaloriesBurned = 60, InstructionUrl = "https://youtu.be/VxQWbxaV_9w?si=RKug7u2tqqgDVLld", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Bật nhảy thu chân (Star Jumps)", MuscleGroup = "Cardio, Toàn thân", BaseCaloriesBurned = 80, InstructionUrl = "https://youtu.be/4uaqCn1p1Xk?si=XlNwAUanQus02ukY", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Cardio Nhảy dây chéo tay (Criss-Cross Rope)", MuscleGroup = "Cardio", BaseCaloriesBurned = 85, InstructionUrl = "https://youtube.com/shorts/Z9-2QMifwFE?si=Netm5Va4HEpzFyJ2", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "HIIT Đi lùi bước chân gấu (Reverse Bear Crawl)", MuscleGroup = "Toàn thân, Cardio", BaseCaloriesBurned = 75, InstructionUrl = "https://youtu.be/b4-dcSacnco?si=rxvdmFcj4Uu_GtaX", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Nhảy Lunge chạm gót chân", MuscleGroup = "Chân, Mông, Cardio", BaseCaloriesBurned = 80, InstructionUrl = "https://youtu.be/1J8mVmtyYpk?si=9HSfE9L0qi2Zc3EZ", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Cardio Chạy nâng cao đùi nghiêng người", MuscleGroup = "Cardio", BaseCaloriesBurned = 70, InstructionUrl = "https://youtu.be/RR3r1-Hi6fM?si=GNmyLa6KB9SPqlXY", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Leo cầu thang một bước hai bậc", MuscleGroup = "Đùi trước, Mông, Cardio", BaseCaloriesBurned = 90, InstructionUrl = "https://youtube.com/.../test", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Cardio Chạy bền tốc độ chậm", MuscleGroup = "Cardio, Sức bền", BaseCaloriesBurned = 220, InstructionUrl = "https://youtube.com/.../test", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "HIIT Bật nhảy nhấp chân liên hoàn", MuscleGroup = "Cardio", BaseCaloriesBurned = 75, InstructionUrl = "https://youtube.com/.../test", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Bật nhảy ngồi xổm thu tay (Tuck Jump Squat)", MuscleGroup = "Cardio, Chân", BaseCaloriesBurned = 85, InstructionUrl = "https://youtu.be/M63FcCHUol4?si=VF9G-nwI18REh_Yy", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Dumbbell Incline Bicep Curl", MuscleGroup = "Bắp tay trước", BaseCaloriesBurned = 30, InstructionUrl = "https://youtu.be/9C5EtvkK6k4?si=zIVTZQhGfKOWGUVf", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Dumbbell Concentration Curl", MuscleGroup = "Bắp tay trước", BaseCaloriesBurned = 30, InstructionUrl = "https://youtu.be/a6VgtO2ZOwM?si=IuIs4Tp40joVBkKj", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Kéo tạ đơn chèo xuồng (Dumbbell Single-arm Row)", MuscleGroup = "Lưng giữa, Biceps", BaseCaloriesBurned = 55, InstructionUrl = "https://youtu.be/6YfpfLvUa5w?si=bdSpm2RLwG8Hv6BD", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Dumbbell Goblet Lunge", MuscleGroup = "Chân, Mông", BaseCaloriesBurned = 70, InstructionUrl = "https://youtu.be/yIc1YbVLMZ8?si=q99GDzaYUYOBlwIL", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Dang tạ vai sau nghiêng người (Dumbbell Bent-over Rear Delt Raise)", MuscleGroup = "Vai sau, Lưng", BaseCaloriesBurned = 35, InstructionUrl = "https://youtu.be/65gMtswVB1c?si=Siar-2G9vlpEmqZy", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Nằm đẩy tạ đơn liên hợp (Dumbbell Squeeze Press)", MuscleGroup = "Ngực, Vai", BaseCaloriesBurned = 50, InstructionUrl = "https://youtu.be/1iQt5BwSWYg?si=LLAenvsXEBJN_dq0", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Dumbbell Pullover", MuscleGroup = "Ngực, Xô lưng", BaseCaloriesBurned = 45, InstructionUrl = "https://youtu.be/KQOBt5gjkSI?si=oLTyPzmP54bSAHtq", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Dumbbell Sumo Squat", MuscleGroup = "Đùi trong, Mông", BaseCaloriesBurned = 75, InstructionUrl = "https://youtu.be/Z2F0bArQH5s?si=ndC9vsmPivQL4L16", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Dumbbell Step-ups", MuscleGroup = "Đùi trước, Mông", BaseCaloriesBurned = 65, InstructionUrl = "https://youtu.be/LBE_p-h_XhA?si=GETft_CfuBAnHSX7", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Dumbbell Arnold Press", MuscleGroup = "Vai, Tay sau", BaseCaloriesBurned = 50, InstructionUrl = "https://youtu.be/at9pnhR2cFM?si=Nf_dalgzwsZ8sOUr", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Dumbbell Zottman Curl", MuscleGroup = "Bắp tay trước, Cẳng tay", BaseCaloriesBurned = 30, InstructionUrl = "https://youtu.be/d69IO-dO2pc?si=BnIcjIdm0o0yqFmz", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Dumbbell Incline Chest Fly", MuscleGroup = "Ngực trên", BaseCaloriesBurned = 40, InstructionUrl = "https://youtu.be/7uvFOUA4gsY?si=SBsgSnKBQ_N5AumS", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Dumbbell Decline Chest Press", MuscleGroup = "Ngực dưới, Tay sau", BaseCaloriesBurned = 50, InstructionUrl = "https://youtu.be/GrmETJwvBgY?si=vBmxRJ9HdCRbwKrx", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Dumbbell Deadlift cơ bản", MuscleGroup = "Đùi sau, Lưng dưới, Mông", BaseCaloriesBurned = 85, InstructionUrl = "https://youtu.be/plb5jEO4Unw?si=b5aK-k40ka9d1hEd", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Dumbbell Thrusters", MuscleGroup = "Toàn thân, Vai, Chân", BaseCaloriesBurned = 90, InstructionUrl = "https://youtu.be/sLIswEpOHng?si=o4NPph9z5K2ic-CN", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Dumbbell Renegade Row", MuscleGroup = "Lưng giữa, Core, Vai", BaseCaloriesBurned = 65, InstructionUrl = "https://youtu.be/KvoHxslZun0?si=g9tHS8JGatiyZjwJ", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Dumbbell Shrugs xoay khớp", MuscleGroup = "Cầu vai, Cổ", BaseCaloriesBurned = 30, InstructionUrl = "https://youtu.be/IuMXYFCVIdA?si=gHUP_ASLLhKX0eDl", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Dumbbell Calf Raise nhón một chân", MuscleGroup = "Bắp chân", BaseCaloriesBurned = 25, InstructionUrl = "https://youtu.be/jlDvVeOcP5M?si=-rcTCDJRPIOlLR13", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Dumbbell Clean and Press", MuscleGroup = "Toàn thân, Vai", BaseCaloriesBurned = 95, InstructionUrl = "https://youtu.be/vIt0SeDcAug?si=Xj8FsBbHPbPFrIpS", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Dumbbell Snatch một tay", MuscleGroup = "Toàn thân, Vai, Mông", BaseCaloriesBurned = 90, InstructionUrl = "https://youtu.be/xA_DpewzL7c?si=5LwOgNn300XiWsXS", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Kéo tạ đơn tay ngửa (Dumbbell Underhand Row)", MuscleGroup = "Lưng dưới, Biceps", BaseCaloriesBurned = 55, InstructionUrl = "https://youtu.be/3vTTr_u7ZYM?si=dZeyVeV_18QgT5i6", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Dumbbell Farmers Walk", MuscleGroup = "Toàn thân, Cơ liên sườn", BaseCaloriesBurned = 50, InstructionUrl = "https://youtu.be/8OtwXwrJizk?si=-k4N-9TyZK_znskO", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Dumbbell Lying Tricep Extension", MuscleGroup = "Tay sau", BaseCaloriesBurned = 35, InstructionUrl = "https://youtu.be/FyBXas1QUWo?si=caZmLsLUe2v0peZn", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Dumbbell Push Press", MuscleGroup = "Vai, Chân, Triceps", BaseCaloriesBurned = 65, InstructionUrl = "https://youtu.be/4tCaD42ghlc?si=sMd-I3a1nBuFHPfc", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Dumbbell Around the World", MuscleGroup = "Vai, Ngực", BaseCaloriesBurned = 30, InstructionUrl = "https://youtu.be/_91Uo7ev_f8?si=4vQICtmTGWn0471u", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Dumbbell Goblet Cossack Squat", MuscleGroup = "Đùi trong, Chân", BaseCaloriesBurned = 70, InstructionUrl = "https://youtu.be/rSaWYv37zzE?si=FeXeJw80Nx9MqBDR", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Dumbbell Bench Fly", MuscleGroup = "Ngực", BaseCaloriesBurned = 40, InstructionUrl = "https://youtu.be/hJo-BOz_rfw?si=iwVvnP81ugbq36Td", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Dumbbell Floor Fly", MuscleGroup = "Ngực", BaseCaloriesBurned = 35, InstructionUrl = "https://youtu.be/YcVRQr5xQLk?si=j0EF1LAmeWDSxfmz", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Dumbbell Cross Body Hammer Curl", MuscleGroup = "Bắp tay trước", BaseCaloriesBurned = 30, InstructionUrl = "https://youtu.be/uBSgMtuxnv0?si=1O3F69ZXRpI0-lHE", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Dumbbell Tate Press", MuscleGroup = "Tay sau", BaseCaloriesBurned = 35, InstructionUrl = "https://youtu.be/Ih5J5cAd-Ww?si=1r286JV93GKrIPjT", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Dumbbell Skull Crusher", MuscleGroup = "Tay sau", BaseCaloriesBurned = 40, InstructionUrl = "https://youtu.be/jPjhQ2hsAds?si=xZvObeGgaOh6Whim", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Dumbbell Lying Chest Fly", MuscleGroup = "Ngực", BaseCaloriesBurned = 40, InstructionUrl = "https://youtube.com/shorts/I41j6y7J-ew?si=wS55vgls_Yqz7Pl1", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Dumbbell Deadlift chân thẳng", MuscleGroup = "Đùi sau, Mông", BaseCaloriesBurned = 80, InstructionUrl = "https://youtu.be/plb5jEO4Unw?si=b6CAySGKf4XHhhY7", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Kéo xô hẹp tay (Close-grip Lat Pulldown)", MuscleGroup = "Lưng, Biceps", BaseCaloriesBurned = 50, InstructionUrl = "https://youtu.be/IjoFCmLX7z0?si=cxDGVGmcbhBPFDsQ", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Ép vai bằng máy (Machine Shoulder Press)", MuscleGroup = "Vai, Tay sau", BaseCaloriesBurned = 45, InstructionUrl = "https://youtu.be/WvLMauqrnK8?si=rd0l7VxUZAEY430L", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Đạp đùi sau bằng máy (Seated Leg Press)", MuscleGroup = "Chân, Mông", BaseCaloriesBurned = 85, InstructionUrl = "https://youtu.be/8EMbB0tCn7Q?si=iJcSK7vQ-oSboelB", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Kéo cáp chèo thuyền một tay (Single-arm Cable Row)", MuscleGroup = "Lưng, Biceps", BaseCaloriesBurned = 50, InstructionUrl = "https://youtu.be/Adzm8LYrZOo?si=T5LqysMGVJ9BJJvI", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Nhón gót chân ngồi bằng máy (Seated Calf Raise)", MuscleGroup = "Bắp chân", BaseCaloriesBurned = 30, InstructionUrl = "https://youtu.be/3ZRe_QpvRPg?si=t9fD5lNg_JLt3rcB", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Kéo cáp bắp tay trước nằm nghiêng (Incline Cable Curl)", MuscleGroup = "Bắp tay trước", BaseCaloriesBurned = 30, InstructionUrl = "https://youtu.be/BhGsQWugKt8?si=trBI4tDNMgy9ZeUp", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Kéo cáp đùi trong (Cable Adduction)", MuscleGroup = "Đùi trong", BaseCaloriesBurned = 30, InstructionUrl = "https://youtu.be/P1pLE1jmnI0?si=Q_JU5AsNhPidzzTF", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Kéo cáp đùi ngoài (Cable Abduction)", MuscleGroup = "Đùi ngoài, Mông", BaseCaloriesBurned = 30, InstructionUrl = "https://youtu.be/pvnR8CDb4BU?si=kyaoIikTbK_82xD1", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Đá cáp mông sau (Cable Glute Kickback)", MuscleGroup = "Mông", BaseCaloriesBurned = 40, InstructionUrl = "https://youtu.be/qJxwJ1e1HxI?si=451B5xwf7XcJTNK7", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Ép ngực dốc lên bằng máy (Leverage Incline Chest Press)", MuscleGroup = "Ngực trên", BaseCaloriesBurned = 55, InstructionUrl = "https://youtu.be/D45sO7jSebI?si=BEY5VvWRt1d1yCF4", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Gập bụng kéo cáp (Cable Crunch)", MuscleGroup = "Bụng, Core", BaseCaloriesBurned = 35, InstructionUrl = "https://youtu.be/ToJeyhydUxU?si=Fn5pd7FweqSEHVKb", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Xoay eo bằng máy (Rotary Torso Machine)", MuscleGroup = "Bụng bên, Core", BaseCaloriesBurned = 30, InstructionUrl = "https://youtu.be/dXPHZuutX84?si=Xd6_it0P3tzH2y97", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Đá đùi trước nằm nghiêng bằng máy", MuscleGroup = "Đùi trước", BaseCaloriesBurned = 45, InstructionUrl = "https://youtu.be/6DRYAHzQgM4?si=hFju_vadqxOjLQZ9", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Kéo cáp tay sau tay ngửa (Underhand Tricep Pushdown)", MuscleGroup = "Tay sau", BaseCaloriesBurned = 35, InstructionUrl = "https://youtu.be/De2x_auxCYw?si=joo3b4i1Kqwiclh0", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Đẩy máy ép ngực ngang (Leverage Chest Press)", MuscleGroup = "Ngực", BaseCaloriesBurned = 50, InstructionUrl = "https://youtu.be/bB6VmPVSxQ4?si=C1-mcwhUkN_9ojTd", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Kéo cáp chéo tay cơ vai sau (Cable Rear Delt Fly)", MuscleGroup = "Vai sau, Lưng", BaseCaloriesBurned = 35, InstructionUrl = "https://www.youtube.com/watch?v=kDhyOrL_1io", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Kéo xô tay thẳng (Straight-arm Pulldown)", MuscleGroup = "Xô lưng, Ngực", BaseCaloriesBurned = 45, InstructionUrl = "https://youtu.be/G9uNaXGTJ4w?si=iivo5gQDCHsm9BNy", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Smith Machine Bench Press", MuscleGroup = "Ngực, Tay sau", BaseCaloriesBurned = 55, InstructionUrl = "https://youtu.be/O5viuEPDXKY?si=mhjXw-RvCnbtCmZh", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Smith Machine Incline Press", MuscleGroup = "Ngực trên, Vai", BaseCaloriesBurned = 55, InstructionUrl = "https://youtu.be/8urE8Z8AMQ4?si=ZDnKgVR7D57B4-gz", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Smith Machine Lunges", MuscleGroup = "Chân, Mông", BaseCaloriesBurned = 70, InstructionUrl = "https://youtu.be/qY7Yo0x5mhE?si=HvnoiUTbmBftomvT", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Kéo cáp bắp tay trước tay úp (Cable Reverse Curl)", MuscleGroup = "Cẳng tay, Bắp tay trước", BaseCaloriesBurned = 30, InstructionUrl = "https://youtu.be/vM40o2TiJfM?si=vqOXa9FObPv6A2nJ", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Machine Rear Delt Fly", MuscleGroup = "Vai sau, Lưng", BaseCaloriesBurned = 40, InstructionUrl = "https://youtu.be/rd13vNldjzo?si=DGUA-mZ7rz9BhSOU", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Machine Bicep Curl", MuscleGroup = "Bắp tay trước", BaseCaloriesBurned = 35, InstructionUrl = "https://youtu.be/UKutOIr6ROg?si=UNOS4eYjIpmNnbkm", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Machine Tricep Extension", MuscleGroup = "Tay sau", BaseCaloriesBurned = 35, InstructionUrl = "https://youtu.be/Bx8ga1BLHLE?si=veUWW0zKie3_0nmb", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Cable Overhead Tricep Extension", MuscleGroup = "Tay sau", BaseCaloriesBurned = 40, InstructionUrl = "https://youtube.com/.../test", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Cable Rope Hammer Curl", MuscleGroup = "Bắp tay trước", BaseCaloriesBurned = 30, InstructionUrl = "https://youtube.com/.../test", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Smith Machine Romanian Deadlift", MuscleGroup = "Đùi sau, Lưng dưới", BaseCaloriesBurned = 75, InstructionUrl = "https://youtube.com/.../test", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Smith Machine Calf Raise", MuscleGroup = "Bắp chân", BaseCaloriesBurned = 30, InstructionUrl = "https://youtube.com/.../test", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Leverage Row Machine", MuscleGroup = "Lưng giữa, Biceps", BaseCaloriesBurned = 55, InstructionUrl = "https://youtube.com/.../test", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Cable Standing One-arm Shoulder Press", MuscleGroup = "Vai", BaseCaloriesBurned = 45, InstructionUrl = "https://youtube.com/.../test", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Cable Shrugs", MuscleGroup = "Cầu vai", BaseCaloriesBurned = 30, InstructionUrl = "https://youtube.com/.../test", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Cable Woodchopper", MuscleGroup = "Bụng bên, Core", BaseCaloriesBurned = 40, InstructionUrl = "https://youtube.com/.../test", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Cable Standing Pallof Press", MuscleGroup = "Core, Bụng", BaseCaloriesBurned = 25, InstructionUrl = "https://youtube.com/.../test", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Machine Leg Abduction (Ép đùi ngoài)", MuscleGroup = "Mông, Đùi ngoài", BaseCaloriesBurned = 30, InstructionUrl = "https://youtube.com/.../test", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Machine Leg Adduction (Ép đùi trong)", MuscleGroup = "Đùi trong", BaseCaloriesBurned = 30, InstructionUrl = "https://youtube.com/.../test", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Cable Front Raise", MuscleGroup = "Vai trước", BaseCaloriesBurned = 30, InstructionUrl = "https://youtube.com/.../test", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Tư thế đứa trẻ (Child's Pose)", MuscleGroup = "Lưng dưới, Vai", BaseCaloriesBurned = 10, InstructionUrl = "https://youtube.com/.../test", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Tư thế lạc đà (Camel Pose)", MuscleGroup = "Lưng, Ngực, Linh hoạt", BaseCaloriesBurned = 22, InstructionUrl = "https://youtube.com/.../test", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Tư thế nhân sư (Sphinx Pose)", MuscleGroup = "Lưng dưới", BaseCaloriesBurned = 12, InstructionUrl = "https://youtube.com/.../test", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Tư thế vũ công (Dancer's Pose)", MuscleGroup = "Chân, Thăng bằng", BaseCaloriesBurned = 24, InstructionUrl = "https://youtube.com/.../test", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Tư thế bánh xe (Wheel Pose)", MuscleGroup = "Toàn thân, Cột sống", BaseCaloriesBurned = 30, InstructionUrl = "https://youtube.com/.../test", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Tư thế cái cày (Plow Pose)", MuscleGroup = "Vai, Cổ, Cột sống", BaseCaloriesBurned = 18, InstructionUrl = "https://youtube.com/.../test", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Tư thế con cá (Fish Pose)", MuscleGroup = "Cổ, Ngực", BaseCaloriesBurned = 14, InstructionUrl = "https://youtube.com/.../test", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Tư thế đứng gập người forward fold", MuscleGroup = "Chân, Đùi sau", BaseCaloriesBurned = 15, InstructionUrl = "https://youtube.com/.../test", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Tư thế em bé hạnh phúc (Happy Baby)", MuscleGroup = "Hông, Lưng dưới", BaseCaloriesBurned = 10, InstructionUrl = "https://youtube.com/.../test", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Tư thế vặn người ngồi (Half Lord of the Fishes)", MuscleGroup = "Cột sống, Lưng", BaseCaloriesBurned = 16, InstructionUrl = "https://youtube.com/.../test", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Tư thế chiến binh I (Warrior I)", MuscleGroup = "Chân, Lưng", BaseCaloriesBurned = 20, InstructionUrl = "https://youtube.com/.../test", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Tư thế chiến binh III (Warrior III)", MuscleGroup = "Chân, Thăng bằng", BaseCaloriesBurned = 22, InstructionUrl = "https://youtube.com/.../test", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Tư thế tam giác (Triangle Pose)", MuscleGroup = "Chân, Linh hoạt", BaseCaloriesBurned = 18, InstructionUrl = "https://youtube.com/.../test", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Tư thế góc nghiêng mở rộng (Extended Side Angle)", MuscleGroup = "Chân, Linh hoạt", BaseCaloriesBurned = 20, InstructionUrl = "https://youtube.com/.../test", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Tư thế tấm ván ngược (Reverse Plank Pose)", MuscleGroup = "Lưng, Vai", BaseCaloriesBurned = 25, InstructionUrl = "https://youtube.com/.../test", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Tư thế nửa vầng trăng (Half Moon Pose)", MuscleGroup = "Chân, Thăng bằng", BaseCaloriesBurned = 20, InstructionUrl = "https://youtube.com/.../test", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Tư thế con mèo (Cat Pose)", MuscleGroup = "Cột sống", BaseCaloriesBurned = 10, InstructionUrl = "https://youtube.com/.../test", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Tư thế con bò (Cow Pose)", MuscleGroup = "Cột sống", BaseCaloriesBurned = 10, InstructionUrl = "https://youtube.com/.../test", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Tư thế đại bàng (Eagle Pose)", MuscleGroup = "Toàn thân, Thăng bằng", BaseCaloriesBurned = 18, InstructionUrl = "https://youtube.com/.../test", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Tư thế chiếc ghế (Chair Pose)", MuscleGroup = "Chân, Mông", BaseCaloriesBurned = 25, InstructionUrl = "https://youtube.com/.../test", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Tư thế cánh cung (Bow Pose)", MuscleGroup = "Lưng, Linh hoạt", BaseCaloriesBurned = 24, InstructionUrl = "https://youtube.com/.../test", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Tư thế tấm ván nghiêng (Side Plank Pose)", MuscleGroup = "Core", BaseCaloriesBurned = 20, InstructionUrl = "https://youtube.com/.../test", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Tư thế con quạ (Crow Pose)", MuscleGroup = "Cổ tay, Vai, Core", BaseCaloriesBurned = 35, InstructionUrl = "https://youtube.com/.../test", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Tư thế đứng bằng vai (Shoulder Stand)", MuscleGroup = "Toàn thân", BaseCaloriesBurned = 22, InstructionUrl = "https://youtube.com/.../test", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Tư thế xác chết (Savasana)", MuscleGroup = "Thư giãn", BaseCaloriesBurned = 5, InstructionUrl = "https://youtube.com/.../test", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Tư thế anh hùng (Hero Pose)", MuscleGroup = "Chân, Linh hoạt", BaseCaloriesBurned = 12, InstructionUrl = "https://youtube.com/.../test", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Tư thế vặn mình nằm ngửa (Supine Spinal Twist)", MuscleGroup = "Lưng, Cột sống", BaseCaloriesBurned = 10, InstructionUrl = "https://youtube.com/.../test", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Tư thế đầu chạm gối ngồi (Head to Knee Pose)", MuscleGroup = "Chân, Đùi sau", BaseCaloriesBurned = 14, InstructionUrl = "https://youtube.com/.../test", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Tư thế con cào cào (Locust Pose)", MuscleGroup = "Lưng dưới, Mông", BaseCaloriesBurned = 20, InstructionUrl = "https://youtube.com/.../test", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Tư thế ếch giang chân (Frog Pose)", MuscleGroup = "Hông, Đùi trong", BaseCaloriesBurned = 15, InstructionUrl = "https://youtube.com/.../test", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Tư thế cổng sắt (Gate Pose)", MuscleGroup = "Core, Lưng bên", BaseCaloriesBurned = 16, InstructionUrl = "https://youtube.com/.../test", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Tư thế ngồi xếp cánh bướm (Butterfly Stretch)", MuscleGroup = "Đùi trong", BaseCaloriesBurned = 10, InstructionUrl = "https://youtube.com/.../test", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Giãn cơ xô với tường (Wall Lat Stretch)", MuscleGroup = "Lưng, Vai", BaseCaloriesBurned = 10, InstructionUrl = "https://youtube.com/.../test", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Giãn cơ vai chéo tay (Cross-body Shoulder Stretch)", MuscleGroup = "Vai", BaseCaloriesBurned = 8, InstructionUrl = "https://youtube.com/.../test", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Giãn cơ ngực đứng tựa khung cửa", MuscleGroup = "Ngực", BaseCaloriesBurned = 8, InstructionUrl = "https://youtube.com/.../test", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Giãn cơ đùi trước đứng 1 chân", MuscleGroup = "Đùi trước", BaseCaloriesBurned = 10, InstructionUrl = "https://youtube.com/.../test", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Giãn cơ bắp chân với bục", MuscleGroup = "Bắp chân", BaseCaloriesBurned = 10, InstructionUrl = "https://youtube.com/.../test", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Tư thế chó úp mặt co 1 chân hướng lên", MuscleGroup = "Chân, Linh hoạt", BaseCaloriesBurned = 18, InstructionUrl = "https://youtube.com/.../test", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },

                // Bodyweight & Core (Type 1) - 10 exercises
                new Exercise { Name = "Hít đất kim cương nâng cao", MuscleGroup = "Tay sau, Ngực", BaseCaloriesBurned = 55, InstructionUrl = "https://youtube.com/.../test", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Hít đất một chân trên bóng", MuscleGroup = "Core, Ngực, Vai", BaseCaloriesBurned = 60, InstructionUrl = "https://youtube.com/.../test", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Plank đi bộ ngang hông nâng cao", MuscleGroup = "Core, Vai", BaseCaloriesBurned = 40, InstructionUrl = "https://youtube.com/.../test", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Gập bụng xoay hông ngang", MuscleGroup = "Core, Bụng bên", BaseCaloriesBurned = 35, InstructionUrl = "https://youtube.com/.../test", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Nằm nhấc chân chữ V co gối", MuscleGroup = "Bụng dưới, Core", BaseCaloriesBurned = 35, InstructionUrl = "https://youtube.com/.../test", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Nhún mông tư thế bàn ngửa", MuscleGroup = "Mông, Lưng dưới", BaseCaloriesBurned = 45, InstructionUrl = "https://youtube.com/.../test", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Hít đất cùi chỏ sát liên sườn", MuscleGroup = "Tay sau, Ngực", BaseCaloriesBurned = 55, InstructionUrl = "https://youtube.com/.../test", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Ngồi xổm tấn ngang luân phiên cơ bản", MuscleGroup = "Chân, Đùi trong", BaseCaloriesBurned = 50, InstructionUrl = "https://youtube.com/.../test", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Plank liên sườn nghiêng người giữ tĩnh", MuscleGroup = "Bụng bên, Core", BaseCaloriesBurned = 35, InstructionUrl = "https://youtube.com/.../test", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Nhấc hông co gối một chân nâng cao", MuscleGroup = "Mông, Core", BaseCaloriesBurned = 45, InstructionUrl = "https://youtube.com/.../test", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },

                // Cardio & HIIT (Type 2) - 10 exercises
                new Exercise { Name = "Cardio Bật nhảy cao tại chỗ", MuscleGroup = "Cardio, Sức mạnh", BaseCaloriesBurned = 90, InstructionUrl = "https://youtube.com/.../test", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "HIIT Chạy nâng cao đùi nhanh", MuscleGroup = "Cardio, Chân", BaseCaloriesBurned = 85, InstructionUrl = "https://youtube.com/.../test", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Cardio Bật nhảy ngang hai bên", MuscleGroup = "Cardio, Chân", BaseCaloriesBurned = 75, InstructionUrl = "https://youtube.com/.../test", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "HIIT Nhảy lò cò đổi chân liên tục", MuscleGroup = "Cardio, Bắp chân", BaseCaloriesBurned = 70, InstructionUrl = "https://youtube.com/.../test", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Cardio Nhảy vỗ tay cao", MuscleGroup = "Cardio, Toàn thân", BaseCaloriesBurned = 65, InstructionUrl = "https://youtube.com/.../test", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Cardio Đấm bốc tiến lùi tự do", MuscleGroup = "Cardio, Vai, Tay", BaseCaloriesBurned = 60, InstructionUrl = "https://youtube.com/.../test", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "HIIT Chạy nước rút tại chỗ", MuscleGroup = "Cardio, Chân", BaseCaloriesBurned = 95, InstructionUrl = "https://youtube.com/.../test", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Cardio Đạp xe giả lập leo dốc", MuscleGroup = "Cardio, Chân", BaseCaloriesBurned = 100, InstructionUrl = "https://youtube.com/.../test", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "HIIT Bật nhảy co gối chạm ngực", MuscleGroup = "Cardio, Toàn thân", BaseCaloriesBurned = 95, InstructionUrl = "https://youtube.com/.../test", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Cardio Chạy ziczac đổi hướng nhanh", MuscleGroup = "Cardio, Chân", BaseCaloriesBurned = 80, InstructionUrl = "https://youtube.com/.../test", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },

                // Dumbbell (Type 3) - 10 exercises
                new Exercise { Name = "Dumbbell Squat Clean nâng cao", MuscleGroup = "Đùi trước, Mông", BaseCaloriesBurned = 85, InstructionUrl = "https://youtube.com/.../test", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Dumbbell Thruster liên hoàn", MuscleGroup = "Vai, Chân", BaseCaloriesBurned = 90, InstructionUrl = "https://youtube.com/.../test", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Dumbbell Push Press lực đẩy", MuscleGroup = "Vai, Tay sau", BaseCaloriesBurned = 70, InstructionUrl = "https://youtube.com/.../test", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Dumbbell Sumo Deadlift đùi trong", MuscleGroup = "Đùi trong, Mông", BaseCaloriesBurned = 80, InstructionUrl = "https://youtube.com/.../test", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Dumbbell French Press ngồi ghế", MuscleGroup = "Tay sau", BaseCaloriesBurned = 35, InstructionUrl = "https://youtube.com/.../test", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Dumbbell Renegade Row xoay người", MuscleGroup = "Lưng giữa, Core", BaseCaloriesBurned = 70, InstructionUrl = "https://youtube.com/.../test", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Cuốn bắp tay trước tạ đơn nghiêng", MuscleGroup = "Bắp tay trước", BaseCaloriesBurned = 30, InstructionUrl = "https://youtube.com/.../test", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Dang tạ vai ngang luân phiên", MuscleGroup = "Vai", BaseCaloriesBurned = 30, InstructionUrl = "https://youtube.com/.../test", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Nhún vai tạ đơn đứng thẳng", MuscleGroup = "Cầu vai", BaseCaloriesBurned = 30, InstructionUrl = "https://youtube.com/.../test", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Deadlift tạ đơn một chân thăng bằng", MuscleGroup = "Đùi sau, Mông", BaseCaloriesBurned = 75, InstructionUrl = "https://youtube.com/.../test", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },

                // Machine (Type 4) - 10 exercises
                new Exercise { Name = "Cable Tricep Pushdown nâng cao", MuscleGroup = "Tay sau", BaseCaloriesBurned = 35, InstructionUrl = "https://youtube.com/.../test", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Cable Hammer Curl cuốn tay", MuscleGroup = "Bắp tay trước", BaseCaloriesBurned = 30, InstructionUrl = "https://youtube.com/.../test", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Leg Press Machine đẩy chân sâu", MuscleGroup = "Chân, Mông", BaseCaloriesBurned = 85, InstructionUrl = "https://youtube.com/.../test", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Smith Machine Hack Squat", MuscleGroup = "Chân, Mông", BaseCaloriesBurned = 80, InstructionUrl = "https://youtube.com/.../test", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Pec Deck Machine ép ngực ngang", MuscleGroup = "Ngực", BaseCaloriesBurned = 45, InstructionUrl = "https://youtube.com/.../test", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "T-Bar Row Machine chèo lưng", MuscleGroup = "Lưng giữa, Biceps", BaseCaloriesBurned = 55, InstructionUrl = "https://youtube.com/.../test", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Cable Standing Face Pull", MuscleGroup = "Vai sau, Lưng trên", BaseCaloriesBurned = 35, InstructionUrl = "https://youtube.com/.../test", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Smith Machine Romanian Deadlift đùi sau", MuscleGroup = "Đùi sau, Lưng dưới", BaseCaloriesBurned = 75, InstructionUrl = "https://youtube.com/.../test", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Cable Woodchopper chéo từ trên xuống", MuscleGroup = "Bụng bên, Core", BaseCaloriesBurned = 40, InstructionUrl = "https://youtube.com/.../test", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Machine Chest Press đẩy ngực ngang", MuscleGroup = "Ngực", BaseCaloriesBurned = 50, InstructionUrl = "https://youtube.com/.../test", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },

                // Yoga / Recovery (Type 5) - 10 exercises
                new Exercise { Name = "Tư thế em bé thư giãn sâu", MuscleGroup = "Lưng dưới, Vai", BaseCaloriesBurned = 10, InstructionUrl = "https://youtube.com/.../test", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Tư thế rắn hổ mang thấp phục hồi", MuscleGroup = "Lưng dưới, Cột sống", BaseCaloriesBurned = 12, InstructionUrl = "https://youtube.com/.../test", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Tư thế bồ câu một chân mở hông", MuscleGroup = "Mông, Hông", BaseCaloriesBurned = 15, InstructionUrl = "https://youtube.com/.../test", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Tư thế chiến binh bay thăng bằng", MuscleGroup = "Chân, Linh hoạt", BaseCaloriesBurned = 20, InstructionUrl = "https://youtube.com/.../test", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Tư thế cái cây thăng bằng tĩnh tâm", MuscleGroup = "Chân, Thăng bằng", BaseCaloriesBurned = 10, InstructionUrl = "https://youtube.com/.../test", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Tư thế con mèo vặn mình phục hồi", MuscleGroup = "Cột sống, Lưng", BaseCaloriesBurned = 10, InstructionUrl = "https://youtube.com/.../test", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Giãn cơ đùi sau đứng gập sâu", MuscleGroup = "Đùi sau, Linh hoạt", BaseCaloriesBurned = 15, InstructionUrl = "https://youtube.com/.../test", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Giãn cơ vai chéo tay ép sát tường", MuscleGroup = "Vai", BaseCaloriesBurned = 8, InstructionUrl = "https://youtube.com/.../test", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Tư thế góc nghiêng kéo giãn sườn", MuscleGroup = "Lồng ngực, Linh hoạt", BaseCaloriesBurned = 18, InstructionUrl = "https://youtube.com/.../test", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Tư thế cây cầu một chân nâng cao", MuscleGroup = "Mông, Lưng dưới", BaseCaloriesBurned = 20, InstructionUrl = "https://youtube.com/.../test", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },

                // Additional Bodyweight & Core - 36 exercises to reach 60 total
                new Exercise { Name = "Hít đất rộng tay", MuscleGroup = "Ngực, Vai", BaseCaloriesBurned = 50, InstructionUrl = "https://youtube.com/.../test", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Hít đất chạm vai", MuscleGroup = "Ngực, Core", BaseCaloriesBurned = 55, InstructionUrl = "https://youtube.com/.../test", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Hít đất giải phóng tay nâng cao", MuscleGroup = "Ngực, Lưng", BaseCaloriesBurned = 55, InstructionUrl = "https://youtube.com/.../test", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Hít đất nhấp nháy", MuscleGroup = "Ngực, Tay sau", BaseCaloriesBurned = 50, InstructionUrl = "https://youtube.com/.../test", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Hít đất kim cương cơ bản", MuscleGroup = "Ngực, Tay sau", BaseCaloriesBurned = 55, InstructionUrl = "https://youtube.com/.../test", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Hít đất dốc lên tay hẹp", MuscleGroup = "Ngực dưới, Tay sau", BaseCaloriesBurned = 45, InstructionUrl = "https://youtube.com/.../test", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Hít đất dốc xuống tay rộng", MuscleGroup = "Ngực trên, Vai", BaseCaloriesBurned = 55, InstructionUrl = "https://youtube.com/.../test", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Hít đất một tay tựa tường", MuscleGroup = "Ngực, Vai", BaseCaloriesBurned = 35, InstructionUrl = "https://youtube.com/.../test", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Plank chạm đầu gối", MuscleGroup = "Bụng, Đùi trước", BaseCaloriesBurned = 35, InstructionUrl = "https://youtube.com/.../test", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Plank xoay hông liên sườn", MuscleGroup = "Bụng bên, Core", BaseCaloriesBurned = 35, InstructionUrl = "https://youtube.com/.../test", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Plank tĩnh tay thẳng", MuscleGroup = "Core, Vai", BaseCaloriesBurned = 30, InstructionUrl = "https://youtube.com/.../test", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Plank co gối chạm cùi chỏ chéo", MuscleGroup = "Bụng bên, Core", BaseCaloriesBurned = 35, InstructionUrl = "https://youtube.com/.../test", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Plank nhấc chân luân phiên", MuscleGroup = "Core, Mông", BaseCaloriesBurned = 35, InstructionUrl = "https://youtube.com/.../test", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Plank đi bộ ngang", MuscleGroup = "Core, Vai", BaseCaloriesBurned = 35, InstructionUrl = "https://youtube.com/.../test", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Gập bụng đạp xe cơ bản", MuscleGroup = "Core, Đùi trước", BaseCaloriesBurned = 30, InstructionUrl = "https://youtube.com/.../test", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Gập bụng ngược nâng cao", MuscleGroup = "Bụng dưới, Core", BaseCaloriesBurned = 35, InstructionUrl = "https://youtube.com/.../test", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Gập bụng chữ V một chân", MuscleGroup = "Core, Bụng dưới", BaseCaloriesBurned = 40, InstructionUrl = "https://youtube.com/.../test", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Gập bụng chạm gót chân liên tục", MuscleGroup = "Bụng bên, Core", BaseCaloriesBurned = 30, InstructionUrl = "https://youtube.com/.../test", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Gập bụng nhấp nhô liên hoàn", MuscleGroup = "Bụng trên, Core", BaseCaloriesBurned = 35, InstructionUrl = "https://youtube.com/.../test", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Nằm nhấc chân luân phiên từng chân", MuscleGroup = "Bụng dưới, Core", BaseCaloriesBurned = 30, InstructionUrl = "https://youtube.com/.../test", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Nằm nâng chân vuông góc giữ tĩnh", MuscleGroup = "Bụng dưới, Core", BaseCaloriesBurned = 30, InstructionUrl = "https://youtube.com/.../test", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Co gối chạm ngực nằm ngửa", MuscleGroup = "Core, Bụng dưới", BaseCaloriesBurned = 30, InstructionUrl = "https://youtube.com/.../test", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Bàn ngược giữ tĩnh", MuscleGroup = "Mông, Lưng dưới, Vai", BaseCaloriesBurned = 30, InstructionUrl = "https://youtube.com/.../test", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Nằm sấp bơi sải cơ bản", MuscleGroup = "Lưng dưới, Mông", BaseCaloriesBurned = 25, InstructionUrl = "https://youtube.com/.../test", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Chiếc lá rơi giữ tĩnh", MuscleGroup = "Lưng dưới, Đùi sau", BaseCaloriesBurned = 25, InstructionUrl = "https://youtube.com/.../test", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Nằm sấp nâng thân trên", MuscleGroup = "Lưng dưới, Vai", BaseCaloriesBurned = 25, InstructionUrl = "https://youtube.com/.../test", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Nằm nhấc hông cơ bản", MuscleGroup = "Mông, Đùi sau", BaseCaloriesBurned = 40, InstructionUrl = "https://youtube.com/.../test", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Nằm nhấc hông một chân", MuscleGroup = "Mông, Lưng dưới", BaseCaloriesBurned = 45, InstructionUrl = "https://youtube.com/.../test", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Ngồi xổm nhún sâu giữ tĩnh", MuscleGroup = "Chân, Mông", BaseCaloriesBurned = 55, InstructionUrl = "https://youtube.com/.../test", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Ngồi xổm nhấc gót chân liên tục", MuscleGroup = "Chân, Bắp chân", BaseCaloriesBurned = 50, InstructionUrl = "https://youtube.com/.../test", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Lunge bước lùi cơ bản", MuscleGroup = "Chân, Mông", BaseCaloriesBurned = 50, InstructionUrl = "https://youtube.com/.../test", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Lunge tấn ngang cơ bản", MuscleGroup = "Chân, Đùi trong", BaseCaloriesBurned = 50, InstructionUrl = "https://youtube.com/.../test", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Lunge chéo chân sau cơ bản", MuscleGroup = "Mông, Đùi ngoài", BaseCaloriesBurned = 50, InstructionUrl = "https://youtube.com/.../test", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Tấn đứng tựa tường giữ tĩnh", MuscleGroup = "Đùi trước, Chân", BaseCaloriesBurned = 30, InstructionUrl = "https://youtube.com/.../test", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Bò kiểu gấu bốn chi", MuscleGroup = "Toàn thân, Core", BaseCaloriesBurned = 65, InstructionUrl = "https://youtube.com/.../test", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Bò sâu đo toàn thân", MuscleGroup = "Toàn thân, Vai, Core", BaseCaloriesBurned = 45, InstructionUrl = "https://youtube.com/.../test", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },

                // Additional Machine - 14 exercises to reach 60 total
                new Exercise { Name = "Kéo cáp cơ vai trước bằng máy", MuscleGroup = "Vai trước", BaseCaloriesBurned = 40, InstructionUrl = "https://youtube.com/.../test", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Kéo cáp cơ vai sau bằng máy", MuscleGroup = "Vai sau", BaseCaloriesBurned = 40, InstructionUrl = "https://youtube.com/.../test", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Ép ngực dốc xuống bằng máy", MuscleGroup = "Ngực dưới", BaseCaloriesBurned = 55, InstructionUrl = "https://youtube.com/.../test", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Smith Machine Close-grip Bench Press", MuscleGroup = "Tay sau, Ngực", BaseCaloriesBurned = 55, InstructionUrl = "https://youtube.com/.../test", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Smith Machine Shoulder Press", MuscleGroup = "Vai, Tay sau", BaseCaloriesBurned = 50, InstructionUrl = "https://youtube.com/.../test", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Smith Machine Bulgarian Split Squat", MuscleGroup = "Chân, Mông", BaseCaloriesBurned = 80, InstructionUrl = "https://youtube.com/.../test", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Cable Pull Through", MuscleGroup = "Mông, Đùi sau", BaseCaloriesBurned = 45, InstructionUrl = "https://youtube.com/.../test", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Cable Bicep Concentration Curl", MuscleGroup = "Bắp tay trước", BaseCaloriesBurned = 30, InstructionUrl = "https://youtube.com/.../test", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Cable Overhead Tricep Extension với dây thừng", MuscleGroup = "Tay sau", BaseCaloriesBurned = 40, InstructionUrl = "https://youtube.com/.../test", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Cable Single-arm Lateral Raise", MuscleGroup = "Vai", BaseCaloriesBurned = 30, InstructionUrl = "https://youtube.com/.../test", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Leg Curl Machine nằm sấp", MuscleGroup = "Đùi sau", BaseCaloriesBurned = 45, InstructionUrl = "https://youtube.com/.../test", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Leg Extension Machine đá đùi", MuscleGroup = "Đùi trước", BaseCaloriesBurned = 45, InstructionUrl = "https://youtube.com/.../test", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Machine Seated Calf Raise nhón chân", MuscleGroup = "Bắp chân", BaseCaloriesBurned = 30, InstructionUrl = "https://youtube.com/.../test", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Cable Lying Leg Curl", MuscleGroup = "Đùi sau", BaseCaloriesBurned = 45, InstructionUrl = "https://youtube.com/.../test", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },

                // Additional Yoga / Recovery - 7 exercises to reach 60 total
                new Exercise { Name = "Tư thế ngọn núi vững chãi", MuscleGroup = "Toàn thân, Thăng bằng", BaseCaloriesBurned = 8, InstructionUrl = "https://youtube.com/.../test", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Tư thế chó úp mặt duỗi mình phục hồi", MuscleGroup = "Lưng, Chân, Linh hoạt", BaseCaloriesBurned = 15, InstructionUrl = "https://youtube.com/.../test", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Tư thế em bé nằm thư giãn vai", MuscleGroup = "Vai, Lưng dưới", BaseCaloriesBurned = 10, InstructionUrl = "https://youtube.com/.../test", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Tư thế nhân sư kéo giãn cột sống", MuscleGroup = "Lưng dưới, Cổ", BaseCaloriesBurned = 12, InstructionUrl = "https://youtube.com/.../test", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Tư thế trăng lưỡi liềm thấp phục hồi", MuscleGroup = "Đùi trước, Hông", BaseCaloriesBurned = 16, InstructionUrl = "https://youtube.com/.../test", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Tư thế cây cầu nhấc mông nhẹ nhàng", MuscleGroup = "Mông, Đùi sau", BaseCaloriesBurned = 20, InstructionUrl = "https://youtube.com/.../test", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Exercise { Name = "Giãn cơ xô tư thế quỳ phục hồi", MuscleGroup = "Lưng, Vai", BaseCaloriesBurned = 10, InstructionUrl = "https://youtube.com/.../test", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
            };

            var newExercises = exercisesToSeed.Where(e => !existingExercises.Contains(e.Name.ToLower())).ToList();
            if (newExercises.Any())
            {
                await _context.Exercises.AddRangeAsync(newExercises);
                await _context.SaveChangesAsync();
            }

            // =========================================================
            // 4. Seed Activity Categories (Danh mục hoạt động cho Onboarding)
            // =========================================================
            if (!await _context.ActivityCategories.AnyAsync())
            {
                var categories = new List<ActivityCategory>
                {
                    new ActivityCategory { Name = "Cardio" },
                    new ActivityCategory { Name = "Chạy bộ" },
                    new ActivityCategory { Name = "HIIT" },
                    new ActivityCategory { Name = "Tập tại nhà" },
                    new ActivityCategory { Name = "Tạ đơn (Dumbbell)" },
                    new ActivityCategory { Name = "Máy phòng gym" },
                    new ActivityCategory { Name = "Yoga" },
                    new ActivityCategory { Name = "Pilates" },
                    new ActivityCategory { Name = "Giãn cơ phục hồi" }
                };
                await _context.ActivityCategories.AddRangeAsync(categories);
                await _context.SaveChangesAsync();
            }

            // =========================================================
            // 5. Seed Users & Data (Admin + Test Users)
            // =========================================================
            if (!await _context.Users.AnyAsync(u => u.Role == (int)UserRole.Admin))
            {
                var adminUser = new User
                {
                    Id = Guid.NewGuid(),
                    FullName = "System Admin",
                    Email = "admin@anfit.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"), // Mật khẩu mặc định
                    Role = (int)UserRole.Admin,
                    IsEmailVerified = true,
                    Gender = (int)Gender.Unknown,
                    TargetGoal = (int)TargetGoal.MaintainWeight,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                await _context.Users.AddAsync(adminUser);
            }

            if (!await _context.Users.AnyAsync(u => u.Email == "test.nam@anfit.com"))
            {
                var userNam = new User
                {
                    Id = Guid.NewGuid(),
                    FullName = "Nguyễn Văn An",
                    Email = "test.nam@anfit.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Test@123"),
                    Role = (int)UserRole.User,
                    IsEmailVerified = true,
                    Gender = (int)Gender.Male,
                    HeightCm = 175,
                    DateOfBirth = new DateTime(2000, 5, 15, 0, 0, 0, DateTimeKind.Utc),
                    TargetGoal = (int)TargetGoal.LoseWeight,
                    DailyBudget = 80000,
                    Occupation = (int)Occupation.OfficeWorker,
                    CurrentBodyShape = 3,
                    TargetBodyShape = 7,
                    FitnessLevel = (int)FitnessLevel.Beginner,
                    PreferredIntensity = (int)PreferredIntensity.Medium,
                    PsychologicalBarrier = (int)PsychologicalBarrier.BoringExercise,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                var userNu = new User
                {
                    Id = Guid.NewGuid(),
                    FullName = "Trần Thị Bình",
                    Email = "test.nu@anfit.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Test@123"),
                    Role = (int)UserRole.User,
                    IsEmailVerified = true,
                    Gender = (int)Gender.Female,
                    HeightCm = 160,
                    DateOfBirth = new DateTime(1998, 10, 22, 0, 0, 0, DateTimeKind.Utc),
                    TargetGoal = (int)TargetGoal.GainMuscle,
                    DailyBudget = 120000,
                    Occupation = (int)Occupation.Student,
                    CurrentBodyShape = 5,
                    TargetBodyShape = 7,
                    FitnessLevel = (int)FitnessLevel.Intermediate,
                    PreferredIntensity = (int)PreferredIntensity.High,
                    PsychologicalBarrier = (int)PsychologicalBarrier.NoMotivation,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await _context.Users.AddRangeAsync(userNam, userNu);
                await _context.SaveChangesAsync();

                // Assign Medical Conditions & Preferred Activities
                var conditions = await _context.MedicalConditions.ToListAsync();
                var activities = await _context.ActivityCategories.ToListAsync();

                // User Nam: Có bệnh tim mạch, béo phì nhẹ (Gout), thích tập nhà, Cardio
                userNam.Conditions.Add(conditions.First(c => c.ConditionName == "Bệnh tim mạch"));
                userNam.Conditions.Add(conditions.First(c => c.ConditionName == "Bệnh gout"));
                userNam.PreferredActivities.Add(activities.First(a => a.Name == "Tập tại nhà"));
                userNam.PreferredActivities.Add(activities.First(a => a.Name == "Cardio"));

                // User Nữ: Dị ứng sữa, muốn tập gym, yoga
                userNu.Conditions.Add(conditions.First(c => c.ConditionName == "Dị ứng sữa / Lactose"));
                userNu.PreferredActivities.Add(activities.First(a => a.Name == "Máy phòng gym"));
                userNu.PreferredActivities.Add(activities.First(a => a.Name == "Yoga"));

                await _context.SaveChangesAsync();

                // Seed Body Logs
                var bodyLogs = new List<UserBodyLog>
                {
                    new UserBodyLog { UserId = userNam.Id, RecordedAt = DateTime.UtcNow.AddDays(-35), Weight = 75.0, Bmi = 24.5, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new UserBodyLog { UserId = userNam.Id, RecordedAt = DateTime.UtcNow.AddDays(-28), Weight = 74.2, Bmi = 24.2, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new UserBodyLog { UserId = userNam.Id, RecordedAt = DateTime.UtcNow.AddDays(-21), Weight = 73.5, Bmi = 24.0, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new UserBodyLog { UserId = userNam.Id, RecordedAt = DateTime.UtcNow.AddDays(-14), Weight = 72.8, Bmi = 23.8, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new UserBodyLog { UserId = userNam.Id, RecordedAt = DateTime.UtcNow.AddDays(-7), Weight = 72.0, Bmi = 23.5, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },

                    new UserBodyLog { UserId = userNu.Id, RecordedAt = DateTime.UtcNow.AddDays(-35), Weight = 55.0, Bmi = 21.5, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new UserBodyLog { UserId = userNu.Id, RecordedAt = DateTime.UtcNow.AddDays(-28), Weight = 55.5, Bmi = 21.7, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new UserBodyLog { UserId = userNu.Id, RecordedAt = DateTime.UtcNow.AddDays(-21), Weight = 56.2, Bmi = 22.0, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new UserBodyLog { UserId = userNu.Id, RecordedAt = DateTime.UtcNow.AddDays(-14), Weight = 56.8, Bmi = 22.2, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new UserBodyLog { UserId = userNu.Id, RecordedAt = DateTime.UtcNow.AddDays(-7), Weight = 57.5, Bmi = 22.5, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
                };
                await _context.UserBodyLogs.AddRangeAsync(bodyLogs);
                await _context.SaveChangesAsync();

                // Seed Daily Health Logs (7 days)
                var healthLogs = new List<DailyHealthLog>();
                var random = new Random();
                for (int i = 0; i < 7; i++)
                {
                    healthLogs.Add(new DailyHealthLog
                    {
                        UserId = userNam.Id,
                        RecordDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-i)),
                        TotalSteps = random.Next(6000, 12000),
                        ActiveCalories = random.Next(200, 500),
                        RestingHeartRate = random.Next(68, 75),
                        MaxHeartRate = random.Next(145, 165),
                        SleepDurationMinutes = random.Next(360, 480),
                        StressLevel = random.Next(20, 60),
                        CalculatedTdee = random.Next(2100, 2300),
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    });

                    healthLogs.Add(new DailyHealthLog
                    {
                        UserId = userNu.Id,
                        RecordDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-i)),
                        TotalSteps = random.Next(4000, 9000),
                        ActiveCalories = random.Next(150, 400),
                        RestingHeartRate = random.Next(60, 70),
                        MaxHeartRate = random.Next(155, 175),
                        SleepDurationMinutes = random.Next(420, 540),
                        StressLevel = random.Next(15, 50),
                        CalculatedTdee = random.Next(1800, 2000),
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    });
                }
                await _context.DailyHealthLogs.AddRangeAsync(healthLogs);
                await _context.SaveChangesAsync();

                // Seed Meal Plans & Details
                var allFoods = await _context.FoodItems.ToListAsync();
                var rice = allFoods.First(f => f.Name == "Cơm trắng");
                var chicken = allFoods.First(f => f.Name == "Ức gà luộc");
                var broccoli = allFoods.First(f => f.Name == "Bông cải xanh");
                var oats = allFoods.First(f => f.Name == "Yến mạch (Oat)");
                var banana = allFoods.First(f => f.Name == "Chuối");
                var sweetPotato = allFoods.First(f => f.Name == "Khoai lang luộc");
                var salmon = allFoods.First(f => f.Name == "Cá hồi nướng");

                for (int i = 0; i < 3; i++)
                {
                    var planDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-i));
                    
                    var namMealPlan = new MealPlan
                    {
                        UserId = userNam.Id,
                        PlanDate = planDate,
                        Status = (int)PlanStatus.Completed,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                        MealPlanDetails = new List<MealPlanDetail>
                        {
                            new MealPlanDetail { FoodItemId = oats.Id, MealType = (int)MealType.Breakfast, Quantity = 1.5, CalculatedCalories = oats.CaloriesPerUnit * 1.5, CalculatedPrice = oats.EstimatedPrice * 1.5M },
                            new MealPlanDetail { FoodItemId = banana.Id, MealType = (int)MealType.Breakfast, Quantity = 1, CalculatedCalories = banana.CaloriesPerUnit, CalculatedPrice = banana.EstimatedPrice },
                            new MealPlanDetail { FoodItemId = rice.Id, MealType = (int)MealType.Lunch, Quantity = 2.0, CalculatedCalories = rice.CaloriesPerUnit * 2.0, CalculatedPrice = rice.EstimatedPrice * 2.0M },
                            new MealPlanDetail { FoodItemId = chicken.Id, MealType = (int)MealType.Lunch, Quantity = 1.5, CalculatedCalories = chicken.CaloriesPerUnit * 1.5, CalculatedPrice = chicken.EstimatedPrice * 1.5M },
                            new MealPlanDetail { FoodItemId = broccoli.Id, MealType = (int)MealType.Lunch, Quantity = 1.0, CalculatedCalories = broccoli.CaloriesPerUnit, CalculatedPrice = broccoli.EstimatedPrice }
                        }
                    };
                    
                    namMealPlan.TotalCalories = namMealPlan.MealPlanDetails.Sum(d => d.CalculatedCalories);
                    namMealPlan.TotalPrice = namMealPlan.MealPlanDetails.Sum(d => d.CalculatedPrice);

                    var nuMealPlan = new MealPlan
                    {
                        UserId = userNu.Id,
                        PlanDate = planDate,
                        Status = (int)PlanStatus.Completed,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                        MealPlanDetails = new List<MealPlanDetail>
                        {
                            new MealPlanDetail { FoodItemId = sweetPotato.Id, MealType = (int)MealType.Dinner, Quantity = 1.5, CalculatedCalories = sweetPotato.CaloriesPerUnit * 1.5, CalculatedPrice = sweetPotato.EstimatedPrice * 1.5M },
                            new MealPlanDetail { FoodItemId = salmon.Id, MealType = (int)MealType.Dinner, Quantity = 1.2, CalculatedCalories = salmon.CaloriesPerUnit * 1.2, CalculatedPrice = salmon.EstimatedPrice * 1.2M }
                        }
                    };

                    nuMealPlan.TotalCalories = nuMealPlan.MealPlanDetails.Sum(d => d.CalculatedCalories);
                    nuMealPlan.TotalPrice = nuMealPlan.MealPlanDetails.Sum(d => d.CalculatedPrice);

                    await _context.MealPlans.AddRangeAsync(namMealPlan, nuMealPlan);
                }
                await _context.SaveChangesAsync();

                // Seed Workout Plans & Activities
                var allExercises = await _context.Exercises.ToListAsync();
                var jumpingJacks = allExercises.First(e => e.Name == "Jumping Jacks");
                var pushUp = allExercises.First(e => e.Name == "Push Up");
                var squat = allExercises.First(e => e.Name == "Squat cơ bản");
                var legPress = allExercises.First(e => e.Name == "Leg Press");
                var latPulldown = allExercises.First(e => e.Name == "Lat Pulldown");

                for (int i = 0; i < 3; i++)
                {
                    var planDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-i));

                    var namWorkout = new WorkoutPlan
                    {
                        UserId = userNam.Id,
                        PlanDate = planDate,
                        Status = (int)PlanStatus.Completed,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                        WorkoutActivities = new List<WorkoutActivity>
                        {
                            new WorkoutActivity { ExerciseId = jumpingJacks.Id, SetsCount = 3, RepsPerSet = 20, RestTimeSeconds = 30 },
                            new WorkoutActivity { ExerciseId = pushUp.Id, SetsCount = 3, RepsPerSet = 10, RestTimeSeconds = 60 },
                            new WorkoutActivity { ExerciseId = squat.Id, SetsCount = 3, RepsPerSet = 15, RestTimeSeconds = 45 }
                        }
                    };
                    namWorkout.EstimatedCaloriesBurn = namWorkout.WorkoutActivities.Sum(a => a.SetsCount * 10);

                    var nuWorkout = new WorkoutPlan
                    {
                        UserId = userNu.Id,
                        PlanDate = planDate,
                        Status = (int)PlanStatus.Completed,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                        WorkoutActivities = new List<WorkoutActivity>
                        {
                            new WorkoutActivity { ExerciseId = legPress.Id, SetsCount = 3, RepsPerSet = 12, RestTimeSeconds = 60 },
                            new WorkoutActivity { ExerciseId = latPulldown.Id, SetsCount = 3, RepsPerSet = 12, RestTimeSeconds = 60 }
                        }
                    };
                    nuWorkout.EstimatedCaloriesBurn = nuWorkout.WorkoutActivities.Sum(a => a.SetsCount * 15);

                    await _context.WorkoutPlans.AddRangeAsync(namWorkout, nuWorkout);
                }
                await _context.SaveChangesAsync();
            }
        }
    }
}