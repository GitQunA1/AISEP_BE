using AISEP.BLL.Helpers;
using AISEP.BLL.DTOs.Requests;
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
            var userId = _currentUserService.GetUserId();
            var investor = await _investorService.GetMyProfileAsync(userId);
            if (investor is null)
                return NotFound(ApiResponse<object>.ErrorResponse("Investor profile not found.", "Not found", 404));

            return Ok(ApiResponse<object>.SuccessResponse(investor, "Success"));
        }

      
       
        [HttpPost]
        [Authorize(Roles = "Investor")]
        public async Task<IActionResult> Create([FromForm] CreateInvestorRequest dto)
        {
            var userId = _currentUserService.GetUserId();
            var data = await _investorService.CreateAsync(userId, dto);

            if (data is null)
                return Conflict(ApiResponse<object>.ErrorResponse("Investor profile already exists.", "Conflict", 409));

            return CreatedAtAction(nameof(GetById), new { id = data.InvestorId },
                ApiResponse<object>.SuccessResponse(data, "Investor created successfully", 201));
        }

       
        [HttpPut("{id:int}")]
        [Authorize(Roles = "Investor")]
        public async Task<IActionResult> Update(int id,[FromForm] UpdateInvestorRequest dto)
        {
           
            var data = await _investorService.UpdateAsync(id, dto);

            if (data is null)
                return NotFound(ApiResponse<object>.ErrorResponse("Investor profile not found.", "Not found", 404));

            return Ok(ApiResponse<object>.SuccessResponse(data, "Investor updated successfully"));
        }
    }
}
