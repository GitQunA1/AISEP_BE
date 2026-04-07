using AISEP.BLL.DTOs.Requests;
using AISEP.BLL.Helpers;
using AISEP.BLL.Services.PostPrs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sieve.Models;

namespace AISEP.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PostPrsController : ControllerBase
    {
        private readonly IPostPrService _postPrService;

        public PostPrsController(IPostPrService postPrService)
        {
            _postPrService = postPrService;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetList([FromQuery] SieveModel sieveModel)
        {
            var result = await _postPrService.GetListAsync(sieveModel);
            return Ok(ApiResponse<object>.SuccessResponse(result, "Success"));
        }

        [HttpGet("{id:int}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _postPrService.GetByIdAsync(id);
            return Ok(ApiResponse<object>.SuccessResponse(result, "Success"));
        }

        [HttpPost]
        [Authorize(Roles = "Admin, Staff")]
        public async Task<IActionResult> Create([FromBody] CreatePostPrRequest request)
        {
            var result = await _postPrService.CreateAsync(request);
            return CreatedAtAction(
                nameof(GetById),
                new { id = result.PostPrId },
                ApiResponse<object>.SuccessResponse(result, "Post PR created successfully.", 201));
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin, Staff")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdatePostPrRequest request)
        {
            var result = await _postPrService.UpdateAsync(id, request);
            return Ok(ApiResponse<object>.SuccessResponse(result, "Post PR updated successfully."));
        }

        [HttpPatch("{id:int}/publish")]
        [Authorize(Roles = "Admin, Staff")]
        public async Task<IActionResult> PatchPublish(int id)
        {
            var result = await _postPrService.PatchPublishAsync(id);
            return Ok(ApiResponse<object>.SuccessResponse(result, "Post PR published successfully."));
        }

        [HttpPatch("{id:int}/delete")]
        [Authorize(Roles = "Admin, Staff")]
        public async Task<IActionResult> PatchDelete(int id, [FromBody] PatchPostPrDeleteRequest request)
        {
            await _postPrService.PatchDeleteAsync(id, request.IsDelete);
            var message = request.IsDelete
                ? "Post PR soft-deleted successfully."
                : "Post PR restored successfully.";
            return Ok(ApiResponse<object>.SuccessResponse(null!, message));
        }
    }
}
