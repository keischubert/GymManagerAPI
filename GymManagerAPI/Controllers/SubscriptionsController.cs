using System.Security.Claims;
using GymManagerAPI.Data.DTOs;
using GymManagerAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GymManagerAPI.Controllers
{
    [Route("api/members/{memberId:int}/[controller]")]
    [ApiController]
    [Authorize(Policy = "UserPolicy")]
    public class SubscriptionsController : ControllerBase
    {
        private readonly SubscriptionService subscriptionService;

        public SubscriptionsController(SubscriptionService subscriptionService)
        {
            this.subscriptionService = subscriptionService;
        }

        [HttpPost]
        public async Task<ActionResult> CreateSubscription([FromRoute] int memberId, [FromBody] SubscriptionCreateDTO subscriptionCreateDTO)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(userIdClaim, out int userId))
            {
                return BadRequest("Invalid cast of user id");
            }

            var result = await subscriptionService.CreateSubscription(memberId, userId, subscriptionCreateDTO);

            if(!result.IsSuccess)
            {
                return StatusCode(result.StatusCode, result.Message);
            }

            var subscriptionDTO = result.Data;

            return CreatedAtAction("GetSubscriptionById", new { id = subscriptionDTO.Id }, subscriptionDTO);
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<SubscriptionListDTO>>> GetSubscriptionsByMemberId([FromRoute] int memberId)
        {
            var result = await subscriptionService.GetSubscriptionsByMember(memberId);

            if (!result.IsSuccess)
            {
                return StatusCode(result.StatusCode, result.Message);
            }

            var subscriptionList = result.Data;

            return Ok(subscriptionList);
        }

        [HttpGet("/api/[controller]")]
        public async Task<ActionResult<IEnumerable<SubscriptionDTO>>> GetFilteredSubscriptions([FromQuery] SubscriptionSearchDTO subscriptionSearchDTO)
        {
            var result = await subscriptionService.GetFilteredSubscriptions(subscriptionSearchDTO);

            var subscriptionList = result.Data;

            return Ok(subscriptionList);
        }

        [HttpGet("/api/[controller]/{id:int}")]
        public async Task<ActionResult<SubscriptionDetailsDTO>> GetSubscriptionById([FromRoute] int id)
        {
            var result = await subscriptionService.GetSubscriptionById(id);

            if (!result.IsSuccess)
            {
                return StatusCode(result.StatusCode, result.Message);
            }

            var subscriptionDetailsDTO = result.Data;

            return Ok(subscriptionDetailsDTO);
        }

        [HttpDelete("/api/[controller]/{id:int}")]
        public async Task<ActionResult> DeleteSubscription([FromRoute] int id)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(userIdClaim, out int userId))
            {
                return BadRequest($"An error with the user id");
            }

            //validation: verificar que el Member tenga la subscripcion
            var result = await subscriptionService.SoftDeleteSubscription(id, userId);

            if (!result.IsSuccess)
            {
                return StatusCode(result.StatusCode, result.Message);
            }

            return NoContent();
        }

    }
}