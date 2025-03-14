using GymManagerAPI.Data.DTOs;
using GymManagerAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace GymManagerAPI.Controllers
{
    [Route("api/members/{memberId:int}/[controller]")]
    [ApiController]
    public class SubscriptionsController : ControllerBase
    {
        private readonly SubscriptionService subscriptionService;

        public SubscriptionsController(SubscriptionService subscriptionService)
        {
            this.subscriptionService = subscriptionService;
        }

        [HttpPost]
        public async Task<ActionResult> Create(int memberId, [FromBody] SubscriptionCreateDTO subscriptionCreateDTO)
        {
            var createdSubscription = await subscriptionService.CreateSubscription(memberId, subscriptionCreateDTO);

            if(!createdSubscription.Success)
            {
                return StatusCode(createdSubscription.ErrorStatusCode, createdSubscription.ErrorMessage);
            }

            var subscriptionDTO = createdSubscription.Data;

            return CreatedAtAction("GetById", new { memberId = subscriptionDTO.MemberId, id = subscriptionDTO.Id }, subscriptionDTO);
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<SubscriptionListDTO>>> GetByMemberId([FromRoute] int memberId)
        {
            var result = await subscriptionService.GetSubscriptionsByMember(memberId);

            if (!result.Success)
            {
                return StatusCode(result.ErrorStatusCode, result.ErrorMessage);
            }

            var subscriptionList = result.Data;

            return Ok(subscriptionList);
        }

        [HttpGet("/api/[controller]")]
        public async Task<ActionResult<IEnumerable<SubscriptionDTO>>> GetSubscriptions([FromQuery] SubscriptionSearchDTO subscriptionSearchDTO)
        {
            var result = await subscriptionService.GetFilteredSubscriptions(subscriptionSearchDTO);

            var subscriptionList = result.Data;

            return Ok(subscriptionList);
        }

        [HttpGet("/api/[controller]/{id:int}")]
        public async Task<ActionResult<SubscriptionDetailsDTO>> GetById([FromRoute] int id)
        {
            var result = await subscriptionService.GetSubscriptionById(id);

            if (!result.Success)
            {
                return StatusCode(result.ErrorStatusCode, result.ErrorMessage);
            }

            var subscriptionDetailsDTO = result.Data;

            return Ok(subscriptionDetailsDTO);
        }

        [HttpDelete("/api/[controller]/{id:int}")]
        public async Task<ActionResult> Delete([FromRoute] int id)
        {
            //validation: verificar que el Member tenga la subscripcion
            var result = await subscriptionService.SoftDeleteSubscription(id);

            if (!result.Success)
            {
                return StatusCode(result.ErrorStatusCode, result.ErrorMessage);
            }

            return NoContent();
        }

    }
}
