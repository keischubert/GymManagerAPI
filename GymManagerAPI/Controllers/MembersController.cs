using GymManagerAPI.Data.DTOs;
using GymManagerAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GymManagerAPI.Controllers
{
    [ApiController]
    [Route("/api/members")]
    [Produces("application/json")]
    [Authorize(Policy = "UserPolicy")]
    public class MembersController : Controller
    {
        private readonly MemberService memberService;

        public MembersController(MemberService memberService)
        {
            this.memberService = memberService;
        }

        [HttpPost]
        public async Task<ActionResult<MemberDTO>> CreateMember(MemberCreateDTO memberCreateDTO)
        {
            var result = await memberService.CreateMember(memberCreateDTO);

            if(!result.IsSuccess)
            {
                return StatusCode(result.StatusCode, result.Message);
            }

            return CreatedAtAction("GetMemberById", new { id = result.Data.Id }, result.Data);
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<MemberListDTO>>> GetFilteredMembers([FromQuery] MemberSearchDTO memberSearchDTO)
        {
            var memberFilteredList = await memberService.GetFilteredMembers(memberSearchDTO);

            return Ok(memberFilteredList);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<MemberDTO>> GetMemberById([FromRoute] int id, [FromQuery] bool details)
        {
            var result = await memberService.GetMemberDTOById(id, details);

            if(!result.IsSuccess)
            {
                return StatusCode(result.StatusCode, result.Message);
            }

            var memberDTO = result.Data;

            return Ok(memberDTO);
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult<MemberDTO>> UpdateMember([FromRoute] int id, [FromBody] MemberUpdateDTO memberUpdateDTO)
        {
            var result = await memberService.UpdateMember(id, memberUpdateDTO);

            if(!result.IsSuccess)
            {
                return StatusCode(result.StatusCode, result.Message);
            }

            var memberDTO = result.Data;

            return Ok(memberDTO);
        }
    }
}