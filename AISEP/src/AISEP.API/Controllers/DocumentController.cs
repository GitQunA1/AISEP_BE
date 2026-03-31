using AISEP.BLL.Helpers;
using AISEP.BLL.DTOs.Requests;
using AISEP.BLL.Services.Documents;
using AISEP.BLL.Services.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sieve.Models;
using System.Security.Claims;

namespace AISEP.API.Controllers
{
    [ApiController]
    [Authorize]
    public class DocumentController : ControllerBase
    {
        private readonly IDocumentService _documentService;
        private readonly IUserService _currentUserService;

        public DocumentController(IDocumentService documentService, IUserService currentUserService)
        {
            _documentService = documentService;
            _currentUserService = currentUserService;
        }

        [HttpPost("api/projects/{projectId}/documents")]
        [Authorize(Roles = "Startup")]
        public async Task<IActionResult> Upload([FromRoute] int projectId, [FromForm] UploadDocumentRequest request)
        {
            if (request.File == null || request.File.Length == 0)
                return BadRequest(ApiResponse<object>.ErrorResponse("File is required.", "Validation failed"));

            try
            {
                var userId = _currentUserService.GetUserId();
                var result = await _documentService.UploadDocumentAsync(projectId, userId, request);
                return CreatedAtAction(nameof(GetById), new { documentId = result.DocumentId },
                    ApiResponse<object>.SuccessResponse(result, "Document uploaded successfully", 201));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<object>.ErrorResponse(ex.Message, "Not found", 404));
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, ApiResponse<object>.ErrorResponse(ex.Message, "Forbidden", 403));
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ApiResponse<object>.ErrorResponse(ex.Message, "Operation not allowed", 409));
            }
        }

        [HttpGet("api/projects/{projectId}/documents")]
        [Authorize(Roles = "Startup, Staff, Admin")]
        public async Task<IActionResult> GetByProjectId([FromRoute] int projectId, [FromQuery] SieveModel model)
        {
            try
            {
                var userId = _currentUserService.GetUserId();
                var role = User.FindFirstValue(ClaimTypes.Role)!;
                var result = await _documentService.GetByProjectIdAsync(projectId, userId, role, model);
                return Ok(ApiResponse<object>.SuccessResponse(result, "Success"));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<object>.ErrorResponse(ex.Message, "Not found", 404));
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, ApiResponse<object>.ErrorResponse(ex.Message, "Forbidden", 403));
            }
        }

        [HttpGet("api/documents/{documentId}")]
        [Authorize(Roles = "Startup, Staff, Admin")]
        public async Task<IActionResult> GetById([FromRoute] int documentId)
        {
            try
            {
                var userId = _currentUserService.GetUserId();
                var role = User.FindFirstValue(ClaimTypes.Role)!;
                var result = await _documentService.GetByIdAsync(documentId, userId, role);
                if (result is null)
                    return NotFound(ApiResponse<object>.ErrorResponse("Document not found.", "Not found", 404));

                return Ok(ApiResponse<object>.SuccessResponse(result, "Success"));
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, ApiResponse<object>.ErrorResponse(ex.Message, "Forbidden", 403));
            }
        }

        [HttpDelete("api/documents/{documentId}")]
        [Authorize(Roles = "Startup, Admin")]
        public async Task<IActionResult> Delete([FromRoute] int documentId)
        {
            try
            {
                var userId = _currentUserService.GetUserId();
                var role = User.FindFirstValue(ClaimTypes.Role)!;
                var deleted = await _documentService.DeleteAsync(documentId, userId, role);
                if (!deleted)
                    return NotFound(ApiResponse<object>.ErrorResponse("Document not found.", "Not found", 404));

                return Ok(ApiResponse<object>.SuccessResponse(null!, "Document deleted successfully"));
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, ApiResponse<object>.ErrorResponse(ex.Message, "Forbidden", 403));
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ApiResponse<object>.ErrorResponse(ex.Message, "Operation not allowed", 409));
            }
        }

        [HttpGet("api/documents/{documentId}/verify")]
        [AllowAnonymous]
        public async Task<IActionResult> VerifyDocument([FromRoute] int documentId)
        {
            try
            {
                var result = await _documentService.VerifyDocumentAsync(documentId);
                return Ok(ApiResponse<object>.SuccessResponse(result, "Verification completed"));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<object>.ErrorResponse(ex.Message, "Not found", 404));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message, "Invalid operation", 400));
            }
            catch (HttpRequestException ex)
            {
                return StatusCode(502, ApiResponse<object>.ErrorResponse(ex.Message, "Blockchain RPC error", 502));
            }
        }

        [HttpPut("api/projects/{projectId}/approve")]
        [Authorize(Roles = "Staff, Admin")]
        public async Task<IActionResult> ApproveProject([FromRoute] int projectId)
        {
            try
            {
               
                var result = await _documentService.ApproveProjectAsync(projectId);
                return Ok(ApiResponse<object>.SuccessResponse(result, "Project approved and document stored on blockchain successfully."));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<object>.ErrorResponse(ex.Message, "Not found", 404));
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ApiResponse<object>.ErrorResponse(ex.Message, "Operation not allowed", 409));
            }
        }

    }
}
