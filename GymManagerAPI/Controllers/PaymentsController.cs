//using AutoMapper;
//using GymManagerAPI.Data.Context;
//using GymManagerAPI.Data.DTOs;
//using GymManagerAPI.Models;
//using Microsoft.AspNetCore.Http;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;

//namespace GymManagerAPI.Controllers
//{
//    [Route("api/[controller]")]
//    [ApiController]
//    public class PaymentsController : ControllerBase
//    {
//        private readonly ApplicationDbContext context;
//        private readonly IMapper mapper;

//        public PaymentsController(ApplicationDbContext context, IMapper mapper) 
//        {
//            this.context = context;
//            this.mapper = mapper;
//        }

//        [HttpPost]
//        public async Task<ActionResult> Create(CreatePaymentDTO paymentDTO)
//        {
//            //payment validation
//            //validation: validate if the memberId exists in Members 
//            var member = await context.Members.Include(m => m.MembershipCard).FirstOrDefaultAsync(x => x.Id == paymentDTO.MemberId);

//            if (member == null)
//            {
//                return NotFound("There's a problem with the member");
//            }

//            //payment detail validation
//            //validation: un payment necesita al menos un detalle
//            if (paymentDTO.PaymentDetails == null)
//            {
//                return BadRequest("there's no payment details");
//            }

//            //data ingresada por el usuario
//            var listInputMembershipPlanIds = paymentDTO.PaymentDetails.Select(x => x.MembershipPlanId).ToList();

//            var listInputQuantities = paymentDTO.PaymentDetails.Select(x => x.Quantity).ToList();

//            //validation: verificar si no hay items de payment details repetidos (en lugar de crear varios detalles repetidos, se puede especificar la cantidad directamente)
//            var thereAreRepeats = listInputMembershipPlanIds.GroupBy(id => id).Any(x => x.Count() > 1);
//            if (thereAreRepeats)
//            {
//                return BadRequest("There are payment details repeated. You must specific quantity instead");
//            }

//            //validation: validate quantity values. They cannot be less than 1
//            var allAreValids = listInputQuantities.All(x => x > 0);
//            if (!allAreValids)
//            {
//                return BadRequest("There can't be quantities less than 1");
//            }

//            //verificar si todos los MembershipId ingresados existen en la base de datos
//            var listMembershipPlanIdsExistingInDb = await context.MembershipPlans
//                .Where(m => listInputMembershipPlanIds.Contains(m.Id))
//                .CountAsync();

//            if (listInputMembershipPlanIds.GroupBy(id => id).Count() != listMembershipPlanIdsExistingInDb)
//            {
//                return NotFound("There's a problem with membership plans");
//            }

//            //calculating and setting the amount
//            //var listPrices = await context.MembershipPlans.Where(m => listInputMembershipPlanIds.Contains(m.Id)).Select(x => x.Price).ToListAsync();

//            var listPlansSelected = await context.MembershipPlans
//                .Where(m => listInputMembershipPlanIds.Contains(m.Id))
//                .Select(x => new {price = x.Price, durationInDays = x.DurationInDays})
//                .ToListAsync();

//            var totalAmount = listPlansSelected.Zip(listInputQuantities, (plan, quantity) => plan.price * quantity)
//                .Sum();

//            //mapping of CreatePaymentDTO to Payment to save in the db
//            var payment = mapper.Map<Payment>(paymentDTO);

//            ////setting some dinamyc values
//            payment.DateTime = DateTime.Now;
//            payment.Amount = totalAmount;

//            context.Add(payment);
//            await context.SaveChangesAsync();

//            //luego de realizar el pago debo actualizar la membresia del miembro (StartDate y ExpirationDate)
//            var numberOfDays = listPlansSelected.Sum(x => x.durationInDays);
//            if (!member.MembershipCard.ExpirationDate.HasValue || member.MembershipCard.ExpirationDate < DateTime.Now)
//            {
//                member.MembershipCard.StartDate = DateTime.Now;
//                member.MembershipCard.ExpirationDate = DateTime.Now.AddDays(numberOfDays);
//            }
//            else
//            {
//                member.MembershipCard.ExpirationDate?.AddDays(numberOfDays);
//            }

//            await context.SaveChangesAsync();

//            return Ok();
//        }
//    }
//}