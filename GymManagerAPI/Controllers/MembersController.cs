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

        //Endpoint para filtrar members segun algunos parametros
        [HttpGet]
        public async Task<ActionResult<List<MemberListDTO>>> Get(
            [FromQuery] string name, 
            [FromQuery] int? gender, 
            [FromQuery] string ci, 
            [FromQuery] string email, 
            [FromQuery] DateTime? activeMembersFromDate)
        {
            var query = dbContext.Members.AsQueryable();

            if (!string.IsNullOrWhiteSpace(name))
            {
                query = query.Where(x => x.Name.Contains(name));
            }

            if (gender.HasValue)
            {
                query = query.Where(x => x.GenderId == gender);
            }

            if (!string.IsNullOrWhiteSpace(ci))
            {
                query = query.Where(x => x.Ci.Equals(ci));
            }

            if (!string.IsNullOrWhiteSpace(email))
            {
                query = query.Where(x => x.Email.Equals(email));
            }

            if (activeMembersFromDate.HasValue)
            {
                var activeMemberIds = await dbContext.Subscriptions
                    .Where(x => x.ExpirationDate > DateTime.Now)
                    .GroupBy(x => x.MemberId)
                    .Select(x => x.Key)
                    .ToListAsync();

                query = query.Where(x => activeMemberIds.Contains(x.Id));
            }

            var memberList = await query.ToListAsync();

            var memberListDTO = mapper.Map<List<MemberListDTO>>(memberList);

            return Ok(memberListDTO);
        } 

        [HttpGet("{id:int}")]
        public async Task<ActionResult<MemberDTO>> GetById([FromRoute] int id, [FromQuery] bool details)
        {
            var query = dbContext.Members.AsQueryable();

            if(details)
            {
                query = query
                    .Include(x => x.Gender)
                    .Include(x => x.Subscriptions);
            }

            var member = await query.FirstOrDefaultAsync(x => x.Id == id);

            //validation: existencia del miembro
            if (member == null)
            {
                return NotFound("No existe ningun miembro con el id proporcionado");
            }

            var memberDTO = mapper.Map<MemberDTO>(member);

            return Ok(memberDTO);
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