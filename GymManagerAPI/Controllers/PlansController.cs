using AutoMapper;
using GymManagerAPI.Data.Context;
using GymManagerAPI.Data.DTOs;
using GymManagerAPI.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GymManagerAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PlansController : ControllerBase
    {
        private readonly ApplicationDbContext applicationDbContext;
        private readonly IMapper mapper;

        public PlansController(ApplicationDbContext applicationDbContext, IMapper mapper)
        {
            this.applicationDbContext = applicationDbContext;
            this.mapper = mapper;
        }

        [HttpPost]
        public async Task<ActionResult<PlanDTO>> Create([FromBody] PlanCreateDTO planCreateDTO)
        {   
            var plan = mapper.Map<Plan>(planCreateDTO);

            applicationDbContext.Plans.Add(plan);
            await applicationDbContext.SaveChangesAsync();

            var planDTO = mapper.Map<PlanDTO>(plan);

            return CreatedAtAction("GetById", new {id =  planDTO.Id}, planDTO);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<PlanDTO>> GetById([FromRoute] int id)
        {
            //validation: existencia del plan segun el id obtenido
            var plan = await applicationDbContext.Plans.FindAsync(id);

            if(plan == null)
            {
                return NotFound("No existe ningun plan con el id proporcionado");
            }

            var planDTO = mapper.Map<PlanDTO>(plan);

            return Ok(planDTO);
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<PlanDTO>>> GetAll()
        {
            var planList = await applicationDbContext.Plans.ToListAsync();

            var planDTOList = mapper.Map<List<PlanDTO>>(planList);

            return Ok(planDTOList);
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult> Update([FromRoute] int id, [FromBody] PlanUpdateDTO planUpdateDTO)
        {
            //validation: verificar existencia del plan segun el id obtenido
            var plan = await applicationDbContext.Plans.FindAsync(id);

            if (plan == null)
            {
                return NotFound("No existe ningun plan con el id proporcionado");
            }

            //update: solo los campos que fueron proporcionados
            if(planUpdateDTO.Name != null)
            {
                plan.Name = planUpdateDTO.Name;
            }

            if(planUpdateDTO.Price != null)
            {
                plan.Price = planUpdateDTO.Price ?? 0;
            }

            if(planUpdateDTO.DurationInDays != null)
            {
                plan.DurationInDays = planUpdateDTO.DurationInDays ?? 0;
            }

            applicationDbContext.Plans.Update(plan);
            await applicationDbContext.SaveChangesAsync();

            var planDTO = mapper.Map<PlanDTO>(plan);

            return Ok(planDTO);
        }
    }
}
