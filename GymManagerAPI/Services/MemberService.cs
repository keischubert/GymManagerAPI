using AutoMapper;
using GymManagerAPI.Data.Common;
using GymManagerAPI.Data.DTOs;
using GymManagerAPI.Interfaces;
using GymManagerAPI.Models;

namespace GymManagerAPI.Services
{
    public class MemberService
    {
        private readonly IMemberRepository memberRepository;
        private readonly IGenderRepository genderRepository;
        private readonly IMapper mapper;

        public MemberService(IMemberRepository memberRepository, IGenderRepository genderRepository, IMapper mapper)
        {
            this.memberRepository = memberRepository;
            this.genderRepository = genderRepository;
            this.mapper = mapper;
        }

        public async Task<OperationResult<MemberDTO>> CreateMember(MemberCreateDTO memberCreateDTO)
        {
            //validation: no puede haber miembros con ci iguales
            if (!await memberRepository.DoesCiExistsAsync(memberCreateDTO.Ci))
            {
                return OperationResult<MemberDTO>.Fail(400, "El Ci ingresado ya existe");
            }

            //validation: verificar si el genderId existe en Genders y este fue enviado en la solicitud
            var doesGenderExists = memberCreateDTO.GenderId != 0 ? await genderRepository.DoesGenderExists(memberCreateDTO.GenderId) : false;

            if (!doesGenderExists)
            {
                return OperationResult<MemberDTO>.Fail(404, "El genero ingresado no existe");
            }

            //mapping: MemberCreateDTO a Member para guardarlo en la db
            var member = mapper.Map<Member>(memberCreateDTO);

            //db: insertando Member
            await memberRepository.AddAsync(member);
            await memberRepository.SaveChangesAsync();

            //mapping: Member a MemberDetailDTO para la respuesta
            var memberDTO = mapper.Map<MemberDTO>(member);

            return OperationResult<MemberDTO>.Ok(memberDTO);
        }

        public async Task<IEnumerable<MemberListDTO>> GetFilteredMembers(MemberSearchDTO memberSearchDTO)
        {
            var memberList = await memberRepository.GetFilteredMembersAsync(memberSearchDTO);

            var memberListDTO = mapper.Map<List<MemberListDTO>>(memberList);

            return memberListDTO;
        }

        public async Task<OperationResult<MemberDTO>> GetMemberDTOById(int id, bool details)
        {
            var member = await memberRepository.GetByIdWithDetailsAsync(id, details);

            //validation: existencia del miembro
            if (member == null)
            {
                return OperationResult<MemberDTO>.Fail(404, "No existe ningun miembro con el id proporcionado");
            }

            var memberDTO = mapper.Map<MemberDTO>(member);

            return OperationResult<MemberDTO>.Ok(memberDTO);
        }

        public async Task<OperationResult<MemberDTO>> UpdateMember(int id, MemberUpdateDTO memberUpdateDTO)
        {
            //validation: verificar si existe el miembro con id obtenido por ruta
            var member = await memberRepository.GetByIdAsync(id);

            if (member == null)
            {
                return OperationResult<MemberDTO>.Fail(404, "No existe ningun miembro con el id proporcionado");
            }

            //validation and update: Name no es nulo y fue cambiado
            var wasNameChanged = memberUpdateDTO.Name != null && !member.Name.Equals(memberUpdateDTO.Name);

            if (wasNameChanged)
            {
                member.Name = memberUpdateDTO.Name;
            }

            //validate and update: ci ingresado fue cambiado por otro que ya existe
            var wasCiChanged = memberUpdateDTO.Ci != null && !member.Ci.Equals(memberUpdateDTO.Ci);

            if (wasCiChanged)
            {
                var ciInputExists = await memberRepository.DoesCiExistsAsync(memberUpdateDTO.Ci);

                if (ciInputExists)
                {
                    return OperationResult<MemberDTO>.Fail(400, "El ci ingresado ya existe");
                }

                member.Ci = memberUpdateDTO.Ci;
            }

            //validate and update: genderId fue cambiado por uno que no existe
            var wasGenderIdChanged = memberUpdateDTO.GenderId != 0 && memberUpdateDTO.GenderId != member.GenderId;

            if (wasGenderIdChanged)
            {
                var doesGenderExists = await genderRepository.DoesGenderExists(memberUpdateDTO.GenderId);

                if (!doesGenderExists)
                {
                    return OperationResult<MemberDTO>.Fail(400, "El genero ingresado no existe");
                }

                member.GenderId = memberUpdateDTO.GenderId;
            }

            //validate and update: phone_number
            var wasPhoneNumberChanged = memberUpdateDTO.PhoneNumber != null && !member.PhoneNumber.Equals(memberUpdateDTO.PhoneNumber);
            if (wasPhoneNumberChanged)
            {
                member.PhoneNumber = memberUpdateDTO.PhoneNumber;
            }

            //database: update
            memberRepository.Update(member);
            await memberRepository.SaveChangesAsync();

            var memberDTO = mapper.Map<MemberDTO>(member);

            return OperationResult<MemberDTO>.Ok(memberDTO);
        }
    }
}
