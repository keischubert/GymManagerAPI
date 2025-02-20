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
    public class MembersController : Controller
    {
        private readonly ApplicationDbContext dbContext;
        private readonly IMapper mapper;
        private string errorMsg { get; } = "Invalid data provided";

        public MembersController(ApplicationDbContext dbContext, IMapper mapper)
        {
            this.dbContext = dbContext;
            this.mapper = mapper;
        }

        [HttpPost]
        public async Task<ActionResult<Member>> Create(CreateMemberDTO createMemberDTO)
        {
            //validation: no puede haber ci repetidos en Members
            var memberCiExists = await dbContext.Members.AnyAsync(m => m.Ci.Equals(createMemberDTO.Ci));

            if (memberCiExists)
            {
                return BadRequest(errorMsg);
            }

            //validation: verificar si el genderId ingresado existe
            if (createMemberDTO.GenderId != null)
            {
                var genderExists = await dbContext.Genders.AnyAsync(g => g.Id.Equals(createMemberDTO.GenderId));

                if (!genderExists)
                {
                    return NotFound(errorMsg);
                }
            }

            //mapeo del DTO de creacion a Member para crear el registro
            var member = mapper.Map<Member>(createMemberDTO);

            dbContext.Members.Add(member);

            await dbContext.SaveChangesAsync();

            return Ok(member);
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<MemberDTO>>> GetAll()
        {
            var listMembers = await dbContext.Members.Include(m => m.Gender).ToListAsync();

            var listMembersDTO = mapper.Map<List<MemberDTO>>(listMembers);

            return Ok(listMembersDTO);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<MemberDTO>> GetById([FromRoute] int id)
        {
            //validation: member id invalido
            var member = await dbContext.Members.Include(m => m.Gender).FirstOrDefaultAsync(m => m.Id.Equals(id));

            if (member == null)
            {
                return NotFound(errorMsg);
            }

            var memberDTO = mapper.Map<MemberDTO>(member);

            return Ok(memberDTO);
        }

        [HttpGet("ci/{ci}")]
        public async Task<ActionResult<MemberDTO>> GetByCi([FromRoute] string ci)
        {
            //validation: member ci invalido
            var member = await dbContext.Members.Include(m => m.Gender).FirstOrDefaultAsync(m => m.Ci.Equals(ci));

            if (member == null)
            {
                return NotFound(errorMsg);
            }

            var memberDTO = mapper.Map<MemberDTO>(member);

            return Ok(memberDTO);
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult<Member>> Update([FromRoute] int id, [FromBody] UpdateMemberDTO updateMemberDTO)
        {
            //validation: existencia del MemberId 
            var member = await dbContext.Members.FindAsync(id);

            if (member == null)
            {
                return NotFound(errorMsg);
            }

            //validation and update: Name no es nulo y fue cambiado
            if (updateMemberDTO.Name != null && !member.Name.Equals(updateMemberDTO.Name))
            {
                member.Name = updateMemberDTO.Name;
            }

            //validate and update: ci ingresado fue cambiado por otro que ya existe
            var isCiDifferent = !member.Ci.Equals(updateMemberDTO.Ci);

            if (isCiDifferent)
            {
                var ciInputExists = await dbContext.Members.AnyAsync(m => m.Ci == updateMemberDTO.Ci);

                if(ciInputExists)
                {
                    return NotFound(errorMsg);
                }

                member.Ci = updateMemberDTO.Ci;
            }

            //validate and update: genderId fue cambiado por uno que no existe
            var isGenderIdDifferent = !member.GenderId.Equals(updateMemberDTO.GenderId);

            if (isGenderIdDifferent && updateMemberDTO.GenderId != null)
            {
                var genderExists = await dbContext.Members.AnyAsync(m => m.Id == updateMemberDTO.GenderId);

                if (!genderExists)
                {
                    return NotFound(errorMsg);
                }

                member.GenderId = updateMemberDTO.GenderId;
            }

            //validate and update: phone_number
            if (updateMemberDTO.PhoneNumber != null && !member.PhoneNumber.Equals(updateMemberDTO.PhoneNumber))
            {
                member.PhoneNumber = updateMemberDTO.PhoneNumber;
            }

            //update database
            dbContext.Members.Update(member);
            await dbContext.SaveChangesAsync();

            return Ok(member);
        }

        [HttpDelete("{id:int}")]
        public async Task<ActionResult> Delete([FromRoute] int id)
        {
            //validate: member exists
            var member = await dbContext.Members.FindAsync(id);

            if (member == null)
            {
                return BadRequest(errorMsg);
            }


            dbContext.Members.Remove(member);
            await dbContext.SaveChangesAsync();

            return NoContent();
        }


    }
}