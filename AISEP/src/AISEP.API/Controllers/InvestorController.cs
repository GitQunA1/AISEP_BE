using AISEP.BLL.DTOs.Requests;
using AISEP.BLL.Helpers;
using AISEP.BLL.Services.Investors;
using AISEP.BLL.Services.Users;
using AISEP.DAL.Entities;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Nethereum.Contracts.QueryHandlers.MultiCall;
using Newtonsoft.Json.Linq;
using Org.BouncyCastle.Asn1.Ocsp;
using Sieve.Models;
using Swashbuckle.AspNetCore.Swagger;
using System.Collections;
using static System.Net.Mime.MediaTypeNames;

namespace AISEP.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    
    public class InvestorController : ControllerBase
    {
        private readonly IInvestorService _investorService;
        private readonly IUserService _currentUserService;

        public InvestorController(IInvestorService investorService, IUserService currentUserService)
        {
            _investorService = investorService;
            _currentUserService = currentUserService;
        }

       
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] SieveModel model)
        {
            var result = await _investorService.GetAllAsync(model);
            return Ok(ApiResponse<object>.SuccessResponse(result, "Success"));
        }

        
        [HttpGet("{id}")]
        [Authorize]
        //check dưới service sửa lại log lỗi
        public async Task<IActionResult> GetById(int id)
        {
            var investor = await _investorService.GetByIdAsync(id);
            if (investor is null)
                return NotFound(ApiResponse<object>.ErrorResponse("Investor not found.", "Not found", 404));

            return Ok(ApiResponse<object>.SuccessResponse(investor, "Success"));
        }

       
     
        [HttpGet("me")]
        [Authorize(Roles ="Investor")]
        public async Task<IActionResult> GetMyProfile()
        {
            try
            {
                var investor = await _investorService.GetMyProfileAsync();
                return Ok(ApiResponse<object>.SuccessResponse(investor, "Success"));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<object>.ErrorResponse("Investor profile not found.", "Not found", 404));
            }
        }

      
       
        [HttpPost]
        [Authorize(Roles = "Investor")]
        public async Task<IActionResult> Create([FromForm] CreateInvestorRequest dto)
        {
            try
            {
                var data = await _investorService.CreateAsync(dto);
                return CreatedAtAction(nameof(GetById), new { id = data.InvestorId },
                    ApiResponse<object>.SuccessResponse(data, "Investor created successfully", 201));
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ApiResponse<object>.ErrorResponse(ex.Message, "Conflict", 409));
            }
        }

       
        [HttpPut("{id:int}")]
        [Authorize(Roles = "Investor")]
        public async Task<IActionResult> Update(int id,[FromForm] UpdateInvestorRequest dto)
        {
            try
            {
                var data = await _investorService.UpdateAsync(id, dto);
                return Ok(ApiResponse<object>.SuccessResponse(data, "Investor updated successfully"));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<object>.ErrorResponse("Investor profile not found.", "Not found", 404));
            }
        }

        [HttpPatch("{investorId:int}/approve")]
        [Authorize(Roles = "Staff")]
        public async Task<IActionResult> ApproveInvestor(int investorId)
        {
            try
            {
                await _investorService.ApproveInvestorAsync(investorId);
                return Ok(ApiResponse<object>.SuccessResponse(null, "Investor approved successfully."));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<object>.ErrorResponse(ex.Message, "Not Found", 404));
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ApiResponse<object>.ErrorResponse(ex.Message, "Conflict", 409));
            }
        }

        [HttpPatch("{investorId:int}/reject")]
        [Authorize(Roles = "Staff")]
        public async Task<IActionResult> RejectInvestor(int investorId, [FromBody] RejectInvestorRequest dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse("Invalid input data.", "Bad Request", 400));
            }
            try
            {
                await _investorService.RejectInvestorAsync(investorId, dto.Reason);
                return Ok(ApiResponse<object>.SuccessResponse(null, "Investor rejected successfully."));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<object>.ErrorResponse(ex.Message, "Not Found", 404));
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ApiResponse<object>.ErrorResponse(ex.Message, "Conflict", 409));
            }
        }
    }
}
