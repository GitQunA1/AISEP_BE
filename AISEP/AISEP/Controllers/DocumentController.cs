using AISEP.Common;
using AISEP.Models.DTOs;
using AISEP.Services.Documents;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AISEP.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DocumentController : ControllerBase
    {
        private readonly IDocumentService _documentService;

        public DocumentController(IDocumentService documentService)
        {
            _documentService = documentService;
        }

        [HttpPost("upload")]
        public async Task<IActionResult> Upload([FromForm] UploadDocumentDto dto)
        {
            if (dto.File == null || dto.File.Length == 0)
                return BadRequest(ApiResponse.Fail("File is required."));

            var result = await _documentService.UploadDocumentAsync(dto);
            return Ok(ApiResponse.Success(result));
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _documentService.GetByIdAsync(id);
            if (result is null)
                return NotFound(ApiResponse.Fail($"Document with Id {id} not found."));

            return Ok(ApiResponse.Success(result));
        }

        [HttpGet("startup/{startupId:int}")]
        public async Task<IActionResult> GetByStartupId(int startupId)
        {
            var result = await _documentService.GetByStartupIdAsync(startupId);
            return Ok(ApiResponse.Success(result));
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _documentService.DeleteAsync(id);
            if (!deleted)
                return NotFound(ApiResponse.Fail($"Document with Id {id} not found."));

            return Ok(ApiResponse.Success());
        }
    }
}
