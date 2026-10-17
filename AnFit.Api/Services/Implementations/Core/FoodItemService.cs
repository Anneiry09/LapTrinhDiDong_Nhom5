using AnFit.Api.DTOs.Requests;
using AnFit.Api.DTOs.Responses;
using AnFit.Api.Exceptions;
using AnFit.Api.Extensions;
using AnFit.Api.Models.Entities;
using AnFit.Api.Repositories.Interfaces;
using AnFit.Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace AnFit.Api.Services.Implementations.Core
{
    public class FoodItemService : IFoodItemService
    {
        private readonly IUnitOfWork _unitOfWork;

        public FoodItemService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<PagedResponse<System.Collections.Generic.IEnumerable<FoodItemResponse>>> GetAllAsync(FoodItemFilterQuery query)
        {
            var allItems = await _unitOfWork.FoodItems.GetAllAsync();
            var baseQuery = allItems.AsQueryable();

            if (!string.IsNullOrWhiteSpace(query.SearchTerm))
                baseQuery = baseQuery.Where(f => f.Name != null && f.Name.Contains(query.SearchTerm, StringComparison.OrdinalIgnoreCase));

            if (query.MaxPrice.HasValue)
                baseQuery = baseQuery.Where(f => f.EstimatedPrice <= query.MaxPrice.Value);

            if (query.Category.HasValue)
                baseQuery = baseQuery.Where(f => f.Category == query.Category.Value);

            var totalRecords = baseQuery.Count();
            var totalPages = (int)Math.Ceiling(totalRecords / (double)query.PageSize);

            var pagedData = baseQuery.Skip((query.PageNumber - 1) * query.PageSize)
                                     .Take(query.PageSize)
                                     .Select(f => f.ToResponse())
                                     .ToList();

            return new PagedResponse<System.Collections.Generic.IEnumerable<FoodItemResponse>>(pagedData, query.PageNumber, query.PageSize, totalRecords, totalPages);
        }

        public async Task<FoodItemResponse?> GetByIdAsync(int id)
        {
            var food = await _unitOfWork.FoodItems.GetByIdAsync(id);
            if (food == null) throw new NotFoundException("Không tìm thấy thực phẩm.");
            return food.ToResponse();
        }

        public async Task<FoodItemResponse> CreateAsync(CreateFoodItemRequest request)
        {
            var food = new FoodItem
            {
                Name = request.Name,
                CaloriesPerUnit = request.CaloriesPerUnit,
                ProteinPerUnit = request.ProteinPerUnit,
                CarbsPerUnit = request.CarbsPerUnit,
                FatPerUnit = request.FatPerUnit,
                EstimatedPrice = request.EstimatedPrice,
                Unit = request.Unit,
                Category = request.Category,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            if (request.ConditionIds != null && request.ConditionIds.Any())
            {
                foreach (var cid in request.ConditionIds)
                {
                    var condition = await _unitOfWork.MedicalConditions.GetByIdAsync(cid);
                    if (condition != null) food.Conditions.Add(condition);
                }
            }

            await _unitOfWork.FoodItems.AddAsync(food);
            await _unitOfWork.SaveChangesAsync();

            return food.ToResponse();
        }

        public async Task<FoodItemResponse> UpdateAsync(int id, UpdateFoodItemRequest request)
        {
            var food = await _unitOfWork.FoodItems.GetByIdAsync(id);
            if (food == null) throw new NotFoundException("Không tìm thấy thực phẩm.");

            if (request.Name != null) food.Name = request.Name;
            if (request.CaloriesPerUnit.HasValue) food.CaloriesPerUnit = request.CaloriesPerUnit;
            if (request.ProteinPerUnit.HasValue) food.ProteinPerUnit = request.ProteinPerUnit;
            if (request.CarbsPerUnit.HasValue) food.CarbsPerUnit = request.CarbsPerUnit;
            if (request.FatPerUnit.HasValue) food.FatPerUnit = request.FatPerUnit;
            if (request.EstimatedPrice.HasValue) food.EstimatedPrice = request.EstimatedPrice;
            if (request.Unit != null) food.Unit = request.Unit;
            if (request.Category.HasValue) food.Category = request.Category.Value;

            food.UpdatedAt = DateTime.UtcNow;

            // Xử lý cập nhật danh sách bệnh lý
            if (request.ConditionIds != null)
            {
                food.Conditions.Clear();
                foreach (var cid in request.ConditionIds)
                {
                    var condition = await _unitOfWork.MedicalConditions.GetByIdAsync(cid);
                    if (condition != null) food.Conditions.Add(condition);
                }
            }

            _unitOfWork.FoodItems.Update(food);
            await _unitOfWork.SaveChangesAsync();

            return food.ToResponse();
        }

        public async Task DeleteAsync(int id)
        {
            var food = await _unitOfWork.FoodItems.GetByIdAsync(id);
            if (food == null) throw new NotFoundException("Không tìm thấy thực phẩm.");

            _unitOfWork.FoodItems.Remove(food);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}