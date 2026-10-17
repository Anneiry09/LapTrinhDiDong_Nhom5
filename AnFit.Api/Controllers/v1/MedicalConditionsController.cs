using AnFit.Api.DTOs.Requests;
using AnFit.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace AnFit.Api.Controllers.v1
{
    [Authorize]
    [Route("api/v1/medical-conditions")]
    [Route("api/v1/MedicalConditions")]
    public class MedicalConditionsController : BaseApiController
    {
        private readonly IMedicalConditionService _medicalService;

        public MedicalConditionsController(IMedicalConditionService medicalService)
        {
            _medicalService = medicalService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var response = await _medicalService.GetAllAsync();
            return Ok(new { success = true, data = response });
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Create([FromQuery] string name, [FromQuery] int type)
        {
            var response = await _medicalService.CreateAsync(name, type);
            return Ok(new { success = true, message = "Thêm bệnh lý thành công.", data = response });
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _medicalService.DeleteAsync(id);
            return Ok(new { success = true, message = "Đã xóa bệnh lý." });
        }
    }
}