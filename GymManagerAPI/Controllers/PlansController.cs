using GymManagerAPI.Data.DTOs;
using GymManagerAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace GymManagerAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PlansController : ControllerBase
    {
        private readonly PlanService planService;

        public PlansController(PlanService planService)
        {
            this.planService = planService;
        }

        [HttpPost]
        public async Task<ActionResult<PlanDTO>> Create([FromBody] PlanCreateDTO planCreateDTO)
        {   
            var result = await planService.CreatePlan(planCreateDTO);

            var planDTO = result.Data;

            return CreatedAtAction("GetById", new {id =  planDTO.Id}, planDTO);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<PlanDTO>> GetById([FromRoute] int id)
        {
            var result = await planService.GetById(id);

            if(!result.Success)
            {
                return StatusCode(result.ErrorStatusCode, result.ErrorMessage);
            }

            return Ok(result.Data);
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<PlanDTO>>> GetAll()
        {
            var result = await planService.GetAll();

            var planDTOList = result.Data;

            return Ok(planDTOList);
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult> Update([FromRoute] int id, [FromBody] PlanUpdateDTO planUpdateDTO)
        {
            var result = await planService.UpdatePlan(id, planUpdateDTO);

            if (!result.Success)
            {
                return StatusCode(result.ErrorStatusCode, result.ErrorMessage);
            }

            var planDTO = result.Data;

            return Ok(planDTO);
        }
    }
}
