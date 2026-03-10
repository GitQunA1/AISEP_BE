using AISEP.BLL.Common;
using AISEP.BLL.DTOs.Requests;
using AISEP.BLL.Services.Documents;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AISEP.API.Controllers
{
    [ApiController]
    [Authorize]
    public class DocumentController : ControllerBase
    {
        private readonly IDocumentService _documentService;

        public DocumentController(IDocumentService documentService)
        {
            _documentService = documentService;
        }

        // 1. UPLOAD (Nested under project)
        [HttpPost("api/projects/{projectId}/documents")]
        public async Task<IActionResult> Upload([FromRoute] int projectId, [FromForm] UploadDocumentRequest request)
        {
            if (request.File == null || request.File.Length == 0)
                return BadRequest(ApiResponse<object>.ErrorResponse("File is required.", "Validation failed"));

            var result = await _documentService.UploadDocumentAsync(projectId, request);
            return CreatedAtAction(nameof(GetById), new { documentId = result.DocumentId },
                ApiResponse<object>.SuccessResponse(result, "Document uploaded successfully", 201));
        }

        // 2. GET LIST (Nested under project)
        [HttpGet("api/projects/{projectId}/documents")]
        public async Task<IActionResult> GetByProjectId([FromRoute] int projectId)
        {
            var result = await _documentService.GetByProjectIdAsync(projectId);
            return Ok(ApiResponse<object>.SuccessResponse(result, "Success"));
        }

        // 3. GET DETAILS (Direct)
        [HttpGet("api/documents/{documentId}")]
        public async Task<IActionResult> GetById([FromRoute] int documentId)
        {
            var result = await _documentService.GetByIdAsync(documentId);
            if (result is null)
                return NotFound(ApiResponse<object>.ErrorResponse($"Document {documentId} not found.", "Not found", 404));

            return Ok(ApiResponse<object>.SuccessResponse(result, "Success"));
        }

        // 4. DELETE (Direct — service guards project đã Submit/Publish)
        [HttpDelete("api/documents/{documentId}")]
        public async Task<IActionResult> Delete([FromRoute] int documentId)
        {
            try
            {
                var deleted = await _documentService.DeleteAsync(documentId);
                if (!deleted)
                    return NotFound(ApiResponse<object>.ErrorResponse($"Document {documentId} not found.", "Not found", 404));

                return Ok(ApiResponse<object>.SuccessResponse(null!, "Deleted successfully"));
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ApiResponse<object>.ErrorResponse(ex.Message, "Operation not allowed", 409));
            }
        }

        // 5. VERIFY BLOCKCHAIN (Direct + Action)
        [HttpGet("api/documents/{documentId}/verify")]
        [AllowAnonymous]
        public async Task<IActionResult> VerifyDocument([FromRoute] int documentId)
        {
            var result = await _documentService.VerifyDocumentAsync(documentId);
            return Ok(ApiResponse<object>.SuccessResponse(result, "Verification completed"));
        }
    }
}
