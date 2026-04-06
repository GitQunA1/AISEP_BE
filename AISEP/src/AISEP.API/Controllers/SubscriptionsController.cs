using AISEP.BLL.DTOs.Responses;
using AISEP.BLL.Helpers;
using AISEP.BLL.Services.Subscriptions;
using AISEP.BLL.Services.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sieve.Models;

namespace AISEP.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SubscriptionsController : ControllerBase
    {
        private readonly ISubscriptionService _subscriptionService;
        private readonly IUserService _userService;

        public SubscriptionsController(
            ISubscriptionService subscriptionService,
            IUserService userService)
        {
            _subscriptionService = subscriptionService;
            _userService = userService;
        }

        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> GetMySubscription()
        {
            var userId = _userService.GetUserId();
            var subscription = await _subscriptionService.GetMySubscriptionAsync(userId);

            if (subscription is null)
            {
                return Ok(ApiResponse<SubscriptionResponseDto?>.SuccessResponse(
                    null,
                    "You don't have any active subscription"));
            }

            return Ok(ApiResponse<SubscriptionResponseDto?>.SuccessResponse(subscription, "Success"));
        }

        [HttpGet]
        [Authorize(Roles = "Admin, Staff")]
        public async Task<IActionResult> GetAllSubscriptions([FromQuery] SieveModel sieveModel)
        {
            var subscriptions = await _subscriptionService.GetAllSubscriptionsAsync(sieveModel);
            return Ok(ApiResponse<PagedResult<SubscriptionResponseDto>>.SuccessResponse(subscriptions, "Success"));
        }
    }
}
