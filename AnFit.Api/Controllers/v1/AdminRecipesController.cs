using AnFit.Api.Data;
using AnFit.Api.DTOs.Requests;
using AnFit.Api.Models.Entities;
using AnFit.Api.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AnFit.Api.Controllers.v1
{
    [Authorize(Roles = "Admin")]
    [Route("api/v1/admin/recipes")]
    public class AdminRecipesController : BaseApiController
    {
        private readonly ApplicationDbContext _context;
        private readonly IUnitOfWork _unitOfWork;

        public AdminRecipesController(ApplicationDbContext context, IUnitOfWork unitOfWork)
        {
            _context = context;
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// Lấy tất cả Recipes kèm theo số lượng nguyên liệu (phục vụ danh sách Admin)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var recipes = await _context.Recipes
                .Include(r => r.Ingredients)
                .OrderByDescending(r => r.CreatedAt ?? DateTime.UtcNow)
                .ToListAsync();

            var data = recipes.Select(r => new
            {
                r.Id,
                r.Name,
                totalCalories = r.TotalCalories ?? 0,
                totalProtein = r.TotalProtein ?? 0,
                totalCarbs = r.TotalCarbs ?? 0,
                totalFat = r.TotalFat ?? 0,
                estimatedPrice = r.EstimatedPrice ?? 0,
                cookingTimeMinutes = r.CookingTimeMinutes ?? 0,
                r.MealSuitability,
                r.Category,
                r.ImageUrl,
                ingredientsCount = r.Ingredients.Count
            }).ToList();

            return Ok(new { success = true, data });
        }

        /// <summary>
        /// Chi tiết 1 recipe cùng toàn bộ danh sách nguyên liệu
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var recipe = await _context.Recipes
                .Include(r => r.Ingredients)
                    .ThenInclude(i => i.FoodItem)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (recipe == null)
                return NotFound(new { success = false, message = "Không tìm thấy món ăn." });

            var data = new
            {
                recipe.Id,
                recipe.Name,
                totalCalories = recipe.TotalCalories ?? 0,
                totalProtein = recipe.TotalProtein ?? 0,
                totalCarbs = recipe.TotalCarbs ?? 0,
                totalFat = recipe.TotalFat ?? 0,
                estimatedPrice = recipe.EstimatedPrice ?? 0,
                cookingTimeMinutes = recipe.CookingTimeMinutes ?? 0,
                recipe.MealSuitability,
                recipe.Category,
                recipe.ImageUrl,
                ingredients = recipe.Ingredients.Select(i => new
                {
                    foodItemId = i.FoodItemId,
                    name = i.FoodItem?.Name ?? "Nguyên liệu",
                    quantityGrams = i.QuantityGrams ?? 0,
                    role = i.Role ?? 1
                }).ToList()
            };

            return Ok(new { success = true, data });
        }

        /// <summary>
        /// Tạo mới món ăn kèm nguyên liệu (All-or-Nothing Transaction)
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateRecipeRequestDto dto)
        {
            // Bắt lỗi validation thủ công nếu mảng bị rỗng
            if (dto.Ingredients == null || dto.Ingredients.Count == 0)
            {
                return BadRequest(new { success = false, message = "Không thể tạo món ăn không có nguyên liệu." });
            }

            // Kiểm tra xem tất cả các FoodItemId có hợp lệ hay không
            var foodItemIds = dto.Ingredients.Select(i => i.FoodItemId).Distinct().ToList();
            var validFoodItemsCount = await _context.FoodItems
                .Where(f => foodItemIds.Contains(f.Id))
                .CountAsync();

            if (validFoodItemsCount != foodItemIds.Count)
            {
                return BadRequest(new { success = false, message = "Có nguyên liệu không tồn tại trong thư viện thực phẩm hệ thống." });
            }

            // Sử dụng Transaction bọc kín của EF Core để thực hiện lưu trữ ACID
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Tạo thực thể Recipe
                var recipe = new Recipe
                {
                    Name = dto.Name,
                    TotalCalories = dto.TotalCalories,
                    TotalProtein = dto.TotalProtein,
                    TotalCarbs = dto.TotalCarbs,
                    TotalFat = dto.TotalFat,
                    EstimatedPrice = dto.EstimatedPrice,
                    MealSuitability = dto.MealSuitability,
                    Category = dto.Category,
                    CookingTimeMinutes = dto.CookingTimeMinutes,
                    ImageUrl = dto.ImageUrl,
                    CreatedAt = DateTime.UtcNow
                };

                // Map và lưu danh sách nguyên liệu trực tiếp qua parent collection
                foreach (var ingDto in dto.Ingredients)
                {
                    recipe.Ingredients.Add(new RecipeIngredient
                    {
                        FoodItemId = ingDto.FoodItemId,
                        QuantityGrams = ingDto.QuantityGrams,
                        Role = ingDto.Role
                    });
                }

                // Thêm Recipe (EF Core tự động cascade insert tất cả Ingredients)
                _context.Recipes.Add(recipe);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync(); // Xác nhận giao dịch thành công

                return CreatedAtAction(nameof(GetById), new { id = recipe.Id }, new { success = true, message = "Tạo món ăn và danh sách nguyên liệu thành công.", data = new { id = recipe.Id, name = recipe.Name } });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(); // Hoàn tác lập tức nếu có lỗi xảy ra
                return StatusCode(500, new { success = false, message = "Đã xảy ra lỗi hệ thống trong quá trình lưu trữ.", error = ex.Message });
            }
        }

        /// <summary>
        /// Cập nhật món ăn và toàn bộ nguyên liệu đi kèm (Atomic Update Transaction)
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] CreateRecipeRequestDto dto)
        {
            var recipe = await _context.Recipes
                .Include(r => r.Ingredients)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (recipe == null)
                return NotFound(new { success = false, message = "Không tìm thấy món ăn để cập nhật." });

            if (dto.Ingredients == null || dto.Ingredients.Count == 0)
            {
                return BadRequest(new { success = false, message = "Không thể lưu món ăn không có nguyên liệu." });
            }

            var foodItemIds = dto.Ingredients.Select(i => i.FoodItemId).Distinct().ToList();
            var validFoodItemsCount = await _context.FoodItems
                .Where(f => foodItemIds.Contains(f.Id))
                .CountAsync();

            if (validFoodItemsCount != foodItemIds.Count)
            {
                return BadRequest(new { success = false, message = "Có nguyên liệu không tồn tại trong thư viện thực phẩm hệ thống." });
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 1. Cập nhật thông tin cơ bản món ăn
                recipe.Name = dto.Name;
                recipe.TotalCalories = dto.TotalCalories;
                recipe.TotalProtein = dto.TotalProtein;
                recipe.TotalCarbs = dto.TotalCarbs;
                recipe.TotalFat = dto.TotalFat;
                recipe.EstimatedPrice = dto.EstimatedPrice;
                recipe.MealSuitability = dto.MealSuitability;
                recipe.Category = dto.Category;
                recipe.CookingTimeMinutes = dto.CookingTimeMinutes;
                recipe.ImageUrl = dto.ImageUrl;

                // 2. Xóa toàn bộ nguyên liệu cũ khỏi Database thông qua DbContext để tránh lỗi FK nullability khi SaveChanges
                if (recipe.Ingredients != null && recipe.Ingredients.Any())
                {
                    _context.RecipeIngredients.RemoveRange(recipe.Ingredients);
                }
                recipe.Ingredients.Clear();

                // 3. Thêm mới nguyên liệu từ DTO trực tiếp vào parent collection (EF Core tự cascade insert)
                foreach (var ingDto in dto.Ingredients)
                {
                    recipe.Ingredients.Add(new RecipeIngredient
                    {
                        FoodItemId = ingDto.FoodItemId,
                        QuantityGrams = ingDto.QuantityGrams,
                        Role = ingDto.Role
                    });
                }

                _context.Recipes.Update(recipe);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new { success = true, message = "Cập nhật món ăn thành công.", data = new { id = recipe.Id, name = recipe.Name } });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new { success = false, message = "Lỗi hệ thống khi cập nhật.", error = ex.Message });
            }
        }

        /// <summary>
        /// Xóa món ăn và cascade delete các nguyên liệu của nó
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var recipe = await _context.Recipes
                .Include(r => r.Ingredients)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (recipe == null)
                return NotFound(new { success = false, message = "Không tìm thấy món ăn để xóa." });

            _context.RecipeIngredients.RemoveRange(recipe.Ingredients);
            _context.Recipes.Remove(recipe);
            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "Xóa món ăn thành công." });
        }
    }
}
