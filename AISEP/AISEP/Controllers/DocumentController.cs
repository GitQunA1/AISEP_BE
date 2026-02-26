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

        /// <summary>
        /// Upload document mới (Cloudinary + tuỳ chọn Blockchain).
        /// </summary>
        [HttpPost("upload")]
        public async Task<IActionResult> Upload([FromForm] UploadDocumentDto dto)
        {
            if (dto.File == null || dto.File.Length == 0)
                return BadRequest(new { message = "File is required." });

            var result = await _documentService.UploadDocumentAsync(dto);
            return Ok(result);
        }

        /// <summary>
        /// Lấy document theo Id.
        /// </summary>
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var result = await _documentService.GetByIdAsync(id);
                return Ok(result);
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { message = $"Document with Id {id} not found." });
            }
        }

        /// <summary>
        /// Lấy danh sách document theo ProjectId.
        /// </summary>
        [HttpGet("project/{projectId:int}")]
        public async Task<IActionResult> GetByProjectId(int projectId)
        {
            var result = await _documentService.GetByProjectIdAsync(projectId);
            return Ok(result);
        }

        /// <summary>
        /// Xoá document theo Id.
        /// </summary>
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _documentService.DeleteAsync(id);

            if (!deleted)
                return NotFound(new { message = $"Document with Id {id} not found." });

            return Ok(new { message = "Document deleted successfully." });
        }
    }
}
