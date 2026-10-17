using AnFit.Api.DTOs.Requests;
using AnFit.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace AnFit.Api.Controllers.v1
{
    [Authorize]
    [Route("api/v1/food-items")]
    [Route("api/v1/FoodItems")]
    public class FoodItemsController : BaseApiController
    {
        private readonly IFoodItemService _foodItemService;

        public FoodItemsController(IFoodItemService foodItemService)
        {
            _foodItemService = foodItemService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] FoodItemFilterQuery query)
        {
            var response = await _foodItemService.GetAllAsync(query);
            return Ok(response);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var response = await _foodItemService.GetByIdAsync(id);
            return Ok(new { success = true, data = response });
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateFoodItemRequest request)
        {
            var response = await _foodItemService.CreateAsync(request);
            return Ok(new { success = true, message = "Thêm thực phẩm thành công.", data = response });
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateFoodItemRequest request)
        {
            var response = await _foodItemService.UpdateAsync(id, request);
            return Ok(new { success = true, message = "Cập nhật thực phẩm thành công.", data = response });
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _foodItemService.DeleteAsync(id);
            return Ok(new { success = true, message = "Đã xóa thực phẩm." });
        }
    }
}