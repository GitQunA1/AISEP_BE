using AISEP.BLL.DTOs.Requests;
using AISEP.BLL.Helpers;
using AISEP.BLL.Services.FormValidationRules;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AISEP.API.Controllers
{
    [ApiController]
    [Route("api/form-validation-rules")]
    public class FormValidationRulesController : ControllerBase
    {
        private readonly IFormValidationRuleService _formValidationRuleService;

        public FormValidationRulesController(IFormValidationRuleService formValidationRuleService)
        {
            _formValidationRuleService = formValidationRuleService;
        }

        // Tạo mới rule validate cho một field trong form. Nếu đã tồn tại thì trả conflict.
        [HttpPost]
        [Authorize(Roles = "Staff,Admin")]
        public async Task<IActionResult> Create([FromBody] CreateFormValidationRuleRequest request)
        {
            try
            {
                var result = await _formValidationRuleService.CreateAsync(request);
                return StatusCode(201, ApiResponse<object>.SuccessResponse(result, "Form validation rule created successfully.", 201));
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ApiResponse<object>.ErrorResponse(ex.Message, "Conflict", 409));
            }
        }

        // Trả bộ rule validate của một form để FE render form và validate phía client.
        [HttpGet("{formKey}")]
        [Authorize]
        public async Task<IActionResult> GetByFormKey(string formKey)
        {
            try
            {
                // FE gọi endpoint này khi mở form create/update để lấy rule theo từng field.
                var result = await _formValidationRuleService.GetByFormKeyAsync(formKey);
                return Ok(ApiResponse<object>.SuccessResponse(result, "Form validation rules retrieved successfully."));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message, "Bad Request", 400));
            }
        }

        // Cập nhật một rule đã tồn tại theo id.
        [HttpPut("{id:int}")]
        [Authorize(Roles = "Staff,Admin")]
        public async Task<IActionResult> Update(int id, [FromBody] UpsertFormValidationRuleRequest request)
        {
            try
            {
                // Staff/Admin chỉ update rule đã tồn tại; lúc submit hệ thống sẽ đọc rule mới nhất từ DB.
                var result = await _formValidationRuleService.UpdateAsync(id, request);
                return Ok(ApiResponse<object>.SuccessResponse(result, "Form validation rule updated successfully."));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<object>.ErrorResponse(ex.Message, "Not Found", 404));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message, "Bad Request", 400));
            }
        }
    }
}
