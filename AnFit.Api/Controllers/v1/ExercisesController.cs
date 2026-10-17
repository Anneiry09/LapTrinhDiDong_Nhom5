using AnFit.Api.DTOs.Requests;
using AnFit.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace AnFit.Api.Controllers.v1
{
    [Authorize] // Ai đăng nhập cũng xem được
    public class ExercisesController : BaseApiController
    {
        private readonly IExerciseService _exerciseService;

        public ExercisesController(IExerciseService exerciseService)
        {
            _exerciseService = exerciseService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] ExerciseFilterQuery query)
        {
            var response = await _exerciseService.GetAllAsync(query);
            return Ok(response); // Trả về PagedResponse có phân trang
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var response = await _exerciseService.GetByIdAsync(id);
            return Ok(new { success = true, data = response });
        }

        [Authorize(Roles = "Admin")] // Chỉ Admin mới được tạo
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateExerciseRequest request)
        {
            var response = await _exerciseService.CreateAsync(request);
            return Ok(new { success = true, message = "Thêm bài tập thành công.", data = response });
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateExerciseRequest request)
        {
            var response = await _exerciseService.UpdateAsync(id, request);
            return Ok(new { success = true, message = "Cập nhật bài tập thành công.", data = response });
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _exerciseService.DeleteAsync(id);
            return Ok(new { success = true, message = "Đã xóa bài tập." });
        }
    }
}