using System.ComponentModel.DataAnnotations;
using AutoMapper;
using AutoMapper.Execution;
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
        public async Task<ActionResult> Create(int memberId, [FromBody] SubscriptionCreateDTO subscriptionCreateDTO)
        {
            //validation: verificar si el memberId existe en Members
            var doesMemberExists = await dbContext.Members
                .AnyAsync(m => m.Id == memberId);

            if (!doesMemberExists)
            {
                return NotFound("No existe ningun miembro con el id proporcionado.");
            }

            //validation: verificar si el plan seleccionado existe
            var planSelected = await dbContext.Plans
                .FindAsync(subscriptionCreateDTO.Payment.PlanId);

            if(planSelected == null)
            {
                return NotFound("El plan proporcionado no existe");
            }

            //mapping: SubscriptionCreateDTO a Subscription
            var subscription = mapper.Map<Subscription>(subscriptionCreateDTO);

            subscription.MemberId = memberId;

            //validation: si existe algun plan activo, la fecha de inicio sera la misma que la expiracion
            var expirationDateLastSubscription = await dbContext.Subscriptions
                .Where(s => s.MemberId == memberId)
                .OrderByDescending(s => s.ExpirationDate)
                .Select(x => x.ExpirationDate)
                .FirstOrDefaultAsync();

            subscription.StartDate = expirationDateLastSubscription >= DateTime.Now ? expirationDateLastSubscription : DateTime.Now;

            subscription.ExpirationDate = subscription.StartDate.AddDays(planSelected.DurationInDays);

            //payment
            subscription.Payment.DateTime = DateTime.Now;

            var totalAmount = subscription.Payment.PaymentDetails.Sum(x => x.Amount);

            if (totalAmount != planSelected.Price)
            {
                return BadRequest("Hay un problema con el pago, este no coincide con el precio del plan.");
            }

            subscription.Payment.TotalAmount = subscription.Payment.PaymentDetails.Sum(x => x.Amount);

            //registrando la subscripcion y el pago
            dbContext.Add(subscription);
            await dbContext.SaveChangesAsync();

            //mapping: Subscription a SubscriptionDTO para la respuesta
            var subscriptionDTO = mapper.Map<SubscriptionDTO>(subscription);

            return CreatedAtAction("GetById", new { memberId = subscriptionDTO.MemberId, id = subscriptionDTO.Id }, subscriptionDTO);
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<SubscriptionListDTO>>> GetByMemberId([FromRoute] int memberId)
        {
            //validation: verificar existencia del MemberId
            var doesMemberExists = await dbContext.Members.AnyAsync(m => m.Id == memberId);

            if (!doesMemberExists)
            {
                return NotFound("No existe ningun miembro con el id proporcionado.");
            }

            var subscriptionList = await dbContext.Subscriptions
                .Where(s => s.MemberId == memberId)
                .OrderByDescending(s => s.ExpirationDate)
                .Include(s => s.Payment)
                .ThenInclude(p => p.Plan)
                .ToListAsync();

            var subscriptionListDTO = mapper.Map<List<SubscriptionListDTO>>(subscriptionList);

            return Ok(subscriptionListDTO);
        }

        [HttpGet("/api/[controller]")]
        public async Task<ActionResult<IEnumerable<SubscriptionDTO>>> GetSubscriptions(
            [FromQuery] DateTime? paymentDate,
            [FromQuery] DateTime? paymentDateEnd,
            [FromQuery] DateTime? expirationDate)
        {
            var query = dbContext.Subscriptions
                .Include(x => x.Member)
                .Include(x => x.Payment)
                .ThenInclude(x => x.Plan)
                .AsQueryable();

            if(paymentDate.HasValue && !paymentDateEnd.HasValue)
            {
                query = query
                    .Where(x => x.Payment.DateTime.Date == paymentDate);
            }

            if(paymentDate.HasValue && paymentDateEnd.HasValue)
            {
                query = query
                    .Where(x => x.Payment.DateTime.Date >= paymentDate && x.Payment.DateTime.Date <= paymentDateEnd);
            }

            if (expirationDate.HasValue)
            {
                query = query
                    .Where(x => x.ExpirationDate == expirationDate);
            }

            var subscriptionList = await query.ToListAsync();

            var subscriptionDetailsDTOList = mapper.Map<List<SubscriptionDetailsDTO>>(subscriptionList);

            return Ok(subscriptionDetailsDTOList);
        }

        [HttpGet("/api/[controller]/{id:int}")]
        public async Task<ActionResult<SubscriptionDetailsDTO>> GetById([FromRoute] int id)
        {
            var subscription = await dbContext.Subscriptions
                .Include(x => x.Member)
                .Include(x => x.Payment)
                .ThenInclude(x => x.Plan)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (subscription == null)
            {
                return BadRequest("Ocurrio un error! El miembro no cuenta con ninguna suscripcion con el id proporcionado");
            }

            //mapping: subscription a subscriptionDTO para la respuesta
            var subscriptionDetailsDTO = mapper.Map<SubscriptionDetailsDTO>(subscription);

            return Ok(subscriptionDetailsDTO);
        }

        [HttpDelete("/api/[controller]/{id:int}")]
        public async Task<ActionResult> Delete([FromRoute] int id)
        {
            //validation: verificar que el Member tenga la subscripcion
            var subscription = await dbContext.Subscriptions
                .FirstOrDefaultAsync(x => x.Id == id);

            if(subscription == null)
            {
                return BadRequest("Ocurrio un error! El miembro no cuenta con ninguna suscripcion con el id proporcionado");
            }

            //apply: aplicamos el softdelete
            subscription.IsDeleted = true;
            dbContext.Subscriptions.Update(subscription);

            //registramos el softdelete en DeletedSubscriptions
            var deletedSubscription = new DeletedSubscription()
            {
                SubscriptionId = subscription.Id,
                DeletedBy = "Juan",
                DeletedAt = DateTime.Now
            };

            dbContext.DeletedSubscriptions.Add(deletedSubscription);
            
            await dbContext.SaveChangesAsync();
            

            return NoContent();
        }

    }
}
