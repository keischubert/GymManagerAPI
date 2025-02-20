using AutoMapper;
using GymManagerAPI.Data.DTOs;
using GymManagerAPI.Models;

namespace GymManagerAPI.Data.AutoMapperProfiles
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<CreateMemberDTO, Member>();

            CreateMap<Member, MemberDTO>()
                .ForMember(dest => dest.Gender, options => options.MapFrom(src => src.Gender == null ? "No especificado" : src.Gender.Name));

            CreateMap<CreateSubscriptionDTO, Subscription>();

            CreateMap<CreatePaymentDTO, Payment>();

            CreateMap<CreatePaymentDetailDTO, PaymentDetail>();
        }
    }
}
