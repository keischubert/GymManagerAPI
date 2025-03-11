using System.Reflection;
using AutoMapper;
using GymManagerAPI.Data.Common;
using GymManagerAPI.Data.Context;
using GymManagerAPI.Data.DTOs;
using GymManagerAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace GymManagerAPI.Services
{
    public class MemberService
    {
        private readonly ApplicationDbContext dbContext;
        private readonly IMapper mapper;

        public MemberService(ApplicationDbContext dbContext, IMapper mapper)
        {
            this.dbContext = dbContext;
            this.mapper = mapper;
        }

        public async Task<OperationResult<MemberDTO>> CreateMember(MemberCreateDTO memberCreateDTO)
        {
            //validation: no puede haber miembros con ci iguales
            var doesCiExists = await DoesCiExists(memberCreateDTO.Ci);

            if (doesCiExists)
            {
                return OperationResult<MemberDTO>.Fail(400, "El Ci ingresado ya existe");
            }

            //validation: verificar si el genderId existe en Genders y este fue enviado en la solicitud
            var doesGenderExists = memberCreateDTO.GenderId != 0 ? await DoesGenderExists(memberCreateDTO.GenderId) : false;

            if (!doesGenderExists)
            {
                return OperationResult<MemberDTO>.Fail(404, "El genero ingresado no existe");
            }

            //mapping: MemberCreateDTO a Member para guardarlo en la db
            var member = mapper.Map<Member>(memberCreateDTO);

            //db: insertando Member
            dbContext.Members.Add(member);
            await dbContext.SaveChangesAsync();

            //mapping: Member a MemberDetailDTO para la respuesta
            var memberDTO = mapper.Map<MemberDTO>(member);

            return OperationResult<MemberDTO>.Ok(memberDTO);
        }

        public async Task<IEnumerable<MemberListDTO>> GetFilteredMembers(MemberSearchDTO memberSearchDTO)
        {
            var query = dbContext.Members.AsQueryable();

            if (!string.IsNullOrWhiteSpace(memberSearchDTO.Name))
            {
                query = query.Where(x => x.Name.Contains(memberSearchDTO.Name));
            }

            if (memberSearchDTO.GenderId.HasValue)
            {
                query = query.Where(x => x.GenderId == memberSearchDTO.GenderId);
            }

            if (!string.IsNullOrWhiteSpace(memberSearchDTO.Ci))
            {
                query = query.Where(x => x.Ci.Equals(memberSearchDTO.Ci));
            }

            if (!string.IsNullOrWhiteSpace(memberSearchDTO.Email))
            {
                query = query.Where(x => x.Email.Equals(memberSearchDTO.Email));
            }

            if (memberSearchDTO.ActiveMembersFromDate.HasValue)
            {
                var activeMemberIds = await dbContext.Subscriptions
                    .Where(x => x.ExpirationDate >= memberSearchDTO.ActiveMembersFromDate)
                    .GroupBy(x => x.MemberId)
                    .Select(x => x.Key)
                    .ToListAsync();

                query = query.Where(x => activeMemberIds.Contains(x.Id));
            }

            var memberList = await query.ToListAsync();

            var memberListDTO = mapper.Map<List<MemberListDTO>>(memberList);

            return memberListDTO;
        }

        public async Task<OperationResult<MemberDTO>> GetMemberDTOById(int id, bool details)
        {
            var query = dbContext.Members.AsQueryable();

            if (details)
            {
                query = query
                    .Include(x => x.Gender)
                    .Include(x => x.Subscriptions);
            }

            var member = await query.FirstOrDefaultAsync(x => x.Id == id);

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
            var member = await dbContext.Members.FindAsync(id);

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
                var ciInputExists = await DoesCiExists(memberUpdateDTO.Ci);

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
                var doesGenderExists = await DoesGenderExists(memberUpdateDTO.GenderId);

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
            dbContext.Members.Update(member);
            await dbContext.SaveChangesAsync();

            var memberDTO = mapper.Map<MemberDTO>(member);

            return OperationResult<MemberDTO>.Ok(memberDTO);
        }






        public async Task<bool> DoesCiExists(string ci)
        {
            return await dbContext.Members.AnyAsync(m => m.Ci.Equals(ci));
        }

        public async Task<bool> DoesGenderExists(int id)
        {
            return await dbContext.Genders.AnyAsync(g => g.Id == id);
        }


    }
}
