using System.ComponentModel.DataAnnotations;
using AutoMapper;
using GymManagerAPI.Data.Context;
using GymManagerAPI.Data.DTOs;
using GymManagerAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GymManagerAPI.Controllers
{
    [ApiController]
    [Route("/api/members")]
    [Produces("application/json")]
    public class MembersController : Controller
    {
        private readonly ApplicationDbContext dbContext;
        private readonly IMapper mapper;

        public MembersController(ApplicationDbContext dbContext, IMapper mapper)
        {
            this.dbContext = dbContext;
            this.mapper = mapper;
        }

        [HttpPost]
        public async Task<ActionResult<MemberDTO>> Create(MemberCreateDTO memberCreateDTO)
        {
            //validation: no puede haber miembros con ci iguales
            var doesCiAlreadyExists = await dbContext.Members.AnyAsync(m => m.Ci.Equals(memberCreateDTO.Ci));

            if (doesCiAlreadyExists)
            {
                return BadRequest("El ci proporcionado ya existe");
            }

            //validation: verificar si el genderId existe en Genders y este fue enviado en la solicitud
            if(memberCreateDTO.GenderId != 0)
            {
                var doesGenderExists = await dbContext.Genders.AnyAsync(g => g.Id == memberCreateDTO.GenderId);

                if (!doesGenderExists)
                {
                    return NotFound("El genero ingresado no existe");
                }
            }
            
            //mapping: MemberCreateDTO a Member para guardarlo en la db
            var member = mapper.Map<Member>(memberCreateDTO);

            //db: insertando Member
            dbContext.Members.Add(member);
            await dbContext.SaveChangesAsync();

            //mapping: Member a MemberDetailDTO para la respuesta
            var memberDTO = mapper.Map<MemberDTO>(member);

            return CreatedAtAction("GetById", new { id = memberDTO.Id }, memberDTO);
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<MemberListDTO>>> GetMembers([FromQuery] string name)
        {
            var query = dbContext.Members.AsQueryable();

            //Filtrado de miembros segun el name de la solicitud
            if (!string.IsNullOrEmpty(name))
            {
                query = query
                    .Where(x => x.Name.ToLower().Contains(name.ToLower()));
            }

            //Listar los miembros segun el filtro o todos 
            var memberList = await query.ToListAsync();

            //mapping: List<Member> a List<MemberListDTO> para la respuesta
            var memberListDTO = mapper.Map<List<MemberListDTO>>(memberList);

            return Ok(memberListDTO);
        }

        [HttpGet("actives")]
        public async Task<ActionResult<IEnumerable<MemberListDTO>>> GetActiveMembersByDate([FromQuery] DateTime? date = null)
        {
            //definicion de un query para una consulta opcional, segun si se ha recibido una fecha o no
            var query = dbContext.Subscriptions.AsQueryable();

            if (date.HasValue)
            {
                query = query
                    .Where(x => x.ExpirationDate >= date);
            }
            else
            {
                query = query
                    .Where(x => x.ExpirationDate >= DateTime.Now);
            }

            //se filtran todos los miembros que tengan una suscripcion activa
            var activeMemberIds = await query
                .GroupBy(x => x.MemberId)
                .Select(x => x.Key)
                .ToListAsync();

            //se obtienen los registros a partir de la lista de Ids
            var activeMemberList = await dbContext.Members
                .Where(x => activeMemberIds.Contains(x.Id))
                .ToListAsync();

            //mapping: para la respuesta
            var activeMemberListDTO = mapper.Map<List<MemberListDTO>>(activeMemberList);

            return Ok(activeMemberListDTO);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<MemberDTO>> GetById([FromRoute] int id)
        {
            //validation: existencia del miembro
            var member = await dbContext.Members
                .FirstOrDefaultAsync(m => m.Id == id);

            if (member == null)
            {
                return NotFound("No existe ningun miembro con el id proporcionado");
            }

            var memberDTO = mapper.Map<MemberDTO>(member);

            return Ok(memberDTO);
        }

        [HttpGet("{id:int}/details")]
        public async Task<ActionResult<MemberDetailsDTO>> GetByIdDetails([FromRoute] int id)
        {
            //validation: existencia del miembro
            var member = await dbContext.Members
                .Include(m => m.Gender)
                .Include(m => m.Subscriptions)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (member == null)
            {
                return NotFound("No existe ningun miembro con el id proporcionado");
            }

            var memberDetailsDTO = mapper.Map<MemberDetailsDTO>(member);

            return Ok(memberDetailsDTO);
        }


        [HttpGet("by-ci")]
        public async Task<ActionResult<MemberDetailsDTO>> GetByCi([FromQuery] string ci)
        {
            //validation: verificar existencia de un Member con un ci de la solicitud
            var member = await dbContext.Members
                .Include(m => m.Gender)
                .Include(m => m.Subscriptions)
                .FirstOrDefaultAsync(m => m.Ci.Equals(ci));

            if (member == null)
            {
                return NotFound("No existe ningun miembro con el ci proporcionado");
            }

            var memberDetailsDTO = mapper.Map<MemberDetailsDTO>(member);

            return Ok(memberDetailsDTO);
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult<Member>> Update([FromRoute] int id, [FromBody] MemberUpdateDTO memberUpdateDTO)
        {
            //validation: verificar si existe el miembro con id obtenido por ruta
            var member = await dbContext.Members.FindAsync(id);

            if (member == null)
            {
                return NotFound("No existe ningun miembro con el id proporcionado");
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
                var ciInputExists = await dbContext.Members.AnyAsync(m => m.Ci == memberUpdateDTO.Ci);

                if (ciInputExists)
                {
                    return BadRequest("El ci ingresado ya existe");
                }

                member.Ci = memberUpdateDTO.Ci;
            }

            //validate and update: genderId fue cambiado por uno que no existe
            var wasGenderIdChanged = memberUpdateDTO.GenderId != 0 && memberUpdateDTO.GenderId != member.GenderId;
            
            if (wasGenderIdChanged) 
            {
                var doesGenderExists = await dbContext.Genders.AnyAsync(m => m.Id == memberUpdateDTO.GenderId);

                if (!doesGenderExists)
                {
                    return NotFound("El genero ingresado no existe");
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

            //mapping para la respuesta
            var memberDTO = mapper.Map<MemberDTO>(member);

            return Ok(memberDTO);
        }
    }
}