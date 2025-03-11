using System.ComponentModel.DataAnnotations;
using AutoMapper;
using GymManagerAPI.Data.Context;
using GymManagerAPI.Data.DTOs;
using GymManagerAPI.Models;
using GymManagerAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GymManagerAPI.Controllers
{
    [ApiController]
    [Route("/api/members")]
    [Produces("application/json")]
    public class MembersController : Controller
    {
        private readonly MemberService memberService;

        public MembersController(MemberService memberService)
        {
            this.memberService = memberService;
        }

        [HttpPost]
        public async Task<ActionResult<MemberDTO>> Create(MemberCreateDTO memberCreateDTO)
        {
            var createdMember = await memberService.CreateMember(memberCreateDTO);

            if(!createdMember.Success)
            {
                return StatusCode(createdMember.ErrorStatusCode, createdMember.ErrorMessage);
            }

            return CreatedAtAction("GetById", new { id = createdMember.Data.Id }, createdMember.Data);
        }

        //Endpoint para filtrar members segun algunos parametros
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MemberListDTO>>> Get([FromQuery] MemberSearchDTO memberSearchDTO)
        {
            var memberFilteredList = await memberService.GetFilteredMembers(memberSearchDTO);

            return Ok(memberFilteredList);
        } 

        [HttpGet("{id:int}")]
        public async Task<ActionResult<MemberDTO>> GetById([FromRoute] int id, [FromQuery] bool details)
        {
            var result = await memberService.GetMemberDTOById(id, details);

            if(!result.Success)
            {
                return StatusCode(result.ErrorStatusCode, result.ErrorMessage);
            }

            var memberDTO = result.Data;

            return Ok(memberDTO);
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult<MemberDTO>> Update([FromRoute] int id, [FromBody] MemberUpdateDTO memberUpdateDTO)
        {
            var updatedMember = await memberService.UpdateMember(id, memberUpdateDTO);

            if(!updatedMember.Success)
            {
                return StatusCode(updatedMember.ErrorStatusCode, updatedMember.ErrorMessage);
            }

            var memberDTO = updatedMember.Data;

            return Ok(memberDTO);
        }
    }
}