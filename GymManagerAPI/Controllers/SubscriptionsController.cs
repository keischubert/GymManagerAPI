using AutoMapper;
using GymManagerAPI.Data.Context;
using GymManagerAPI.Data.DTOs;
using GymManagerAPI.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace GymManagerAPI.Controllers
{
    [Route("api/members/{memberId:int}/[controller]")]
    [ApiController]
    public class SubscriptionsController : ControllerBase
    {
        private readonly ApplicationDbContext dbContext;
        private readonly IMapper mapper;

        public SubscriptionsController(ApplicationDbContext dbContext, IMapper mapper)
        {
            this.dbContext = dbContext;
            this.mapper = mapper;
        }

        [HttpPost]
        public async Task<ActionResult<Subscription>> Create(int memberId, [FromBody] CreateSubscriptionDTO subscriptionDTO)
        {
            //validation: memberId is valid
            var memberExists = await dbContext.Members.AnyAsync(m => m.Id.Equals(memberId));

            if(!memberExists)
            {
                return NotFound("There's a problem with the member");
            }

            //validation: deben haber paymentDetails
            var areTherePaymentDetails = subscriptionDTO.Payments.Any(p => !p.PaymentDetails.IsNullOrEmpty());

            if (!areTherePaymentDetails)
            {
                return BadRequest("There's a problem with the payment details");
            }

            //validation: PaymentDetails, no puede haber dos detalles con el mismo PlanId, se debe especificar la cantidad directamente
            var paymentDetailsData = subscriptionDTO.Payments
                .SelectMany(p => p.PaymentDetails)
                .Select(pd => new { planId = pd.PlanId, quantity = pd.Quantity })
                .ToList();

            var areTherePlanIdsRepeated = paymentDetailsData
                .GroupBy(pd => pd.planId)
                .Any(x => x.Count() > 1);

            if (areTherePlanIdsRepeated)
            {
                return BadRequest("There's a problem with the plans in the details");
            }

            //validation: verificar si los planIds no fueron alterados y si existen en Plans
            var countExistingPlanIds = await dbContext.Plans
                .Where(p => paymentDetailsData.Select(x => x.planId).Contains(p.Id))
                .CountAsync();

            if(countExistingPlanIds != paymentDetailsData.Count)
            {
                return BadRequest("There are problems with the plans records");
            }

            return Ok();
        }

    }
}
