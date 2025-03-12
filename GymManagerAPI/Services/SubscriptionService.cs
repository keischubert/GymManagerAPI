using AutoMapper;
using GymManagerAPI.Data.Common;
using GymManagerAPI.Data.DTOs;
using GymManagerAPI.Interfaces;
using GymManagerAPI.Models;

namespace GymManagerAPI.Services
{
    public class SubscriptionService
    {
        private readonly IMapper mapper;
        private readonly ISubscriptionRepository subscriptionRepository;
        private readonly IMemberRepository memberRepository;
        private readonly IPlanRepository planRepository;

        public SubscriptionService(IMapper mapper, ISubscriptionRepository subscriptionRepository, IMemberRepository memberRepository, IPlanRepository planRepository)
        {
            this.mapper = mapper;
            this.subscriptionRepository = subscriptionRepository;
            this.memberRepository = memberRepository;
            this.planRepository = planRepository;
        }

        public async Task<OperationResult<SubscriptionDTO>> CreateSubscription(int memberId, SubscriptionCreateDTO subscriptionCreateDTO)
        {
            //validation: verificar si el memberId existe en Members
            if (!await memberRepository.DoesMemberExistsAsync(memberId))
            {
                return OperationResult<SubscriptionDTO>.Fail(404, "No existe ningun miembro con el id proporcionado.");
            }

            //validation: verificar si el plan seleccionado existe
            var planSelected = await planRepository.GetByIdAsync(subscriptionCreateDTO.Payment.PlanId);

            if (planSelected == null)
            {
                return OperationResult<SubscriptionDTO>.Fail(404, "El plan proporcionado no existe");
            }

            //mapping: SubscriptionCreateDTO a Subscription
            var subscription = mapper.Map<Subscription>(subscriptionCreateDTO);

            subscription.MemberId = memberId;

            //validation: si existe algun plan activo, la fecha de inicio sera la misma que la expiracion
            var expirationDate = await memberRepository.MemberLastSubscriptionExpirationDate(memberId);

            subscription.StartDate = expirationDate >= DateTime.Now ? expirationDate : DateTime.Now;

            subscription.ExpirationDate = subscription.StartDate.AddDays(planSelected.DurationInDays);

            //payment
            subscription.Payment.DateTime = DateTime.Now;

            var totalAmount = subscription.Payment.PaymentDetails.Sum(x => x.Amount);

            if (totalAmount != planSelected.Price)
            {
                return OperationResult<SubscriptionDTO>.Fail(400, "Hay un problema con el pago, este no coincide con el precio del plan.");
            }

            subscription.Payment.TotalAmount = subscription.Payment.PaymentDetails.Sum(x => x.Amount);

            //registrando la subscripcion y el pago
            await subscriptionRepository.AddCascadeAsync(subscription);
            await subscriptionRepository.SaveChangesAsync();

            //mapping: Subscription a SubscriptionDTO para la respuesta
            var subscriptionDTO = mapper.Map<SubscriptionDTO>(subscription);

            return OperationResult<SubscriptionDTO>.Ok(subscriptionDTO);
        }

        public async Task<OperationResult<SubscriptionDetailsDTO>> GetSubscriptionById(int id)
        {
            var subscription = await subscriptionRepository.GetSubscriptionByIdWithDetails(id);

            if (subscription == null)
            {
                return OperationResult<SubscriptionDetailsDTO>.Fail(400, "Ocurrio un error! No existe ninguna suscripcion con el id proporcionado");
            }

            //mapping: subscription a subscriptionDTO para la respuesta
            var subscriptionDetailsDTO = mapper.Map<SubscriptionDetailsDTO>(subscription);

            return OperationResult<SubscriptionDetailsDTO>.Ok(subscriptionDetailsDTO);
        }

        public async Task<OperationResult<IEnumerable<SubscriptionListDTO>>> GetSubscriptionsByMember(int memberId)
        {
            //validation: verificar existencia del MemberId
            if (!await memberRepository.DoesMemberExistsAsync(memberId))
            {
                return OperationResult<IEnumerable<SubscriptionListDTO>>.Fail(404, "No existe ningun miembro con el id proporcionado.");
            }

            var subscriptionList = await subscriptionRepository.GetSubscriptionsByMemberId(memberId);

            var subscriptionListDTO = mapper.Map<List<SubscriptionListDTO>>(subscriptionList);

            return OperationResult<IEnumerable<SubscriptionListDTO>>.Ok(subscriptionListDTO);
        }

        public async Task<OperationResult<IEnumerable<SubscriptionDetailsDTO>>> GetFilteredSubscriptions(SubscriptionSearchDTO subscriptionSearchDTO)
        {
            var subscriptionList = await subscriptionRepository.GetFilteredSubscriptions(subscriptionSearchDTO);

            var subscriptionDetailsDTOList = mapper.Map<IEnumerable<SubscriptionDetailsDTO>>(subscriptionList);

            return OperationResult<IEnumerable<SubscriptionDetailsDTO>>.Ok(subscriptionDetailsDTOList);
        }

        public async Task<OperationResult<Subscription>> SoftDeleteSubscription(int id)
        {
            //validation: verificar la existencia de la subscription
            var subscription = await subscriptionRepository.GetByIdAsync(id);

            if (subscription == null)
            {
                return OperationResult<Subscription>.Fail(404, "Ocurrio un error! No existe ninguna suscripcion con el id proporcionado");
            }

            //apply: aplicamos el softdelete
            await subscriptionRepository.SoftDelete(subscription);

            await subscriptionRepository.SaveChangesAsync();

            return OperationResult<Subscription>.Ok(subscription);
        }
    }
}
