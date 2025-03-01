using AutoMapper;
using GymManagerAPI.Data.DTOs;
using GymManagerAPI.Models;

namespace GymManagerAPI.Data.AutoMapperProfiles
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<MemberCreateDTO, Member>();

            CreateMap<Member, MemberDTO>();

            CreateMap<Member, MemberDetailsDTO>()
                .ForMember(dest => dest.GenderName, options => options.MapFrom(src => src.Gender.Name))

                .ForMember(dest => dest.PlanExpirationDate, options => options.MapFrom(src => src.Subscriptions
                    .OrderByDescending(x => x.ExpirationDate)
                    .Select(x => x.ExpirationDate)
                    .FirstOrDefault()));
                

            CreateMap<Member, MemberListDTO>();

            CreateMap<SubscriptionCreateDTO, Subscription>();

            CreateMap<Subscription, SubscriptionListDTO>()
                .ForMember(dest => dest.PlanName, options => options.MapFrom(s => s.Payment.Plan.Name))
                .ForMember(dest => dest.TotalAmount, options => options.MapFrom(s => s.Payment.TotalAmount));

            CreateMap<Subscription, SubscriptionDTO>();

            CreateMap<PaymentCreateDTO, Payment>();

            CreateMap<Gender, GenderDTO>();

            CreateMap<PlanCreateDTO, Plan>();

            CreateMap<Plan, PlanDTO>();
        }
    }
}
