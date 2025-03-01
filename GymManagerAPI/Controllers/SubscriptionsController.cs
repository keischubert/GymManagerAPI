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
        public async Task<ActionResult<Subscription>> Create(int memberId, [FromBody] SubscriptionCreateDTO subscriptionCreateDTO)
        {
            //validation: verificar si el memberId existe en Members
            var doesMemberExists = await dbContext.Members
                .AnyAsync(m => m.Id == memberId);

            if (!doesMemberExists)
            {
                return NotFound("No existe ningun miembro con el id proporcionado.");
            }

            //validation: si existe algun plan activo, la fecha de inicio sera la misma que la expiracion
            var expirationDateLastSubscription = await dbContext.Subscriptions
                .Where(s => s.MemberId == memberId)
                .OrderByDescending(s => s.ExpirationDate)
                .Select(x => x.ExpirationDate)
                .FirstOrDefaultAsync();

            if (expirationDateLastSubscription >= subscriptionCreateDTO.StartDate)
            {
                return BadRequest($"Ocurrio un error al asignar la fecha de inicio de la suscripcion debido a que ya hay una suscripcion activa.");
            }

            subscriptionCreateDTO.StartDate = expirationDateLastSubscription;

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
            subscription.ExpirationDate = subscription.StartDate.AddDays(planSelected.DurationInDays);

            //payment
            subscription.Payment.DateTime = DateTime.Now;
            subscription.Payment.TotalAmount = planSelected.Price;

            //registrando la subscripcion y el pago
            dbContext.Add(subscription);
            await dbContext.SaveChangesAsync();

            //mapping: Subscription a SubscriptionDTO para la respuesta
            var subscriptionDTO = mapper.Map<SubscriptionDTO>(subscription);

            return CreatedAtAction("GetById", new { memberId = subscriptionDTO.MemberId, id = subscriptionDTO.Id }, subscriptionDTO);
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Subscription>>> GetByMemberId([FromRoute] int memberId)
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

        [HttpGet("{id:int}")]
        public async Task<ActionResult<SubscriptionDTO>> GetById([FromRoute] int memberId, [FromRoute] int id)
        {
            var doesMemberExists = await dbContext.Members.AnyAsync(m => m.Id == memberId);

            if (!doesMemberExists)
            {
                return NotFound("No existe ningun miembro con el id proporcionado.");
            }

            var subscription = await dbContext.Subscriptions
                .Where(s => s.Id == id && s.MemberId == memberId)
                .FirstOrDefaultAsync();

            if (subscription == null)
            {
                return BadRequest("Ocurrio un error! El miembro no cuenta con ninguna suscripcion con el id proporcionado");
            }

            //mapping: subscription a subscriptionDTO para la respuesta
            var subscriptionDTO = mapper.Map<SubscriptionDTO>(subscription);

            return Ok(subscriptionDTO);
        }

        [HttpDelete("{id:int}")]
        public async Task<ActionResult> Delete([FromRoute] int memberId, [FromRoute] int id)
        {
            //validation: verificar existencia del MemberId
            var doesMemberExists = await dbContext.Members.AnyAsync(m => m.Id == memberId);

            if (!doesMemberExists)
            {
                return NotFound("No existe ningun miembro con el id proporcionado.");
            }

            //validation: verificar que el Member tenga la subscripcion
            var subscription = await dbContext.Subscriptions
                .Include(s => s.Payment)
                .Where(s => s.Id == id &&  s.MemberId == memberId)
                .FirstOrDefaultAsync();

            if(subscription == null)
            {
                return BadRequest("Ocurrio un error! El miembro no cuenta con ninguna suscripcion con el id proporcionado");
            }

            //validation: el registro solo podra ser eliminado hasta 10 minutos despues de su pago
            if(subscription.Payment.DateTime.AddMinutes(10) < DateTime.Now)
            {
                return BadRequest("No puedes eliminar esta suscripcion luego de los 10 minutos.");
            }

            //database: delete
            dbContext.Subscriptions.Remove(subscription);
            await dbContext.SaveChangesAsync();

            return NoContent();
        }

    }
}
