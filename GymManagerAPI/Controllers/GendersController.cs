using AutoMapper;
using GymManagerAPI.Data.DTOs;
using GymManagerAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace GymManagerAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GendersController : ControllerBase
    {
        private readonly GenderService genderService;

        public GendersController(GenderService genderService)
        {
            this.genderService = genderService;
        }

        [HttpPost]
        public async Task<ActionResult> Create(GenderCreateDTO genderCreateDTO)
        {
            var result = await genderService.Create(genderCreateDTO);

            var genderDTO = result.Data;

            return CreatedAtAction("GetById", new { id = genderDTO.Id }, genderDTO);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<GenderDTO>> GetById(int id)
        {
            var result = await genderService.GetById(id);

            if (!result.Success)
            {
                return StatusCode(result.ErrorStatusCode, result.ErrorMessage);
            }

            var genderDTO = result.Data;

            return Ok(genderDTO);
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<GenderDTO>>> GetAll()
        {
            var result = await genderService.GetAll();

            var genderDTOList = result.Data;

            return Ok(genderDTOList);
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult<GenderDTO>> Update(int id, GenderUpdateDTO genderUpdateDTO)
        {
            var result = await genderService.UpdateGender(id, genderUpdateDTO);

            if (!result.Success)
            {
                return StatusCode(result.ErrorStatusCode, result.ErrorMessage);
            }

            var genderDTO = result.Data;

            return Ok(genderDTO);
        }


    }
}
