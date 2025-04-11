using GymManagerAPI.Data.DTOs;
using GymManagerAPI.Services;
using Microsoft.AspNetCore.Authorization;
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

        [Authorize(Policy = "AdminPolicy")]
        [HttpPost]
        public async Task<ActionResult<PlanDTO>> CreatePlan([FromBody] PlanCreateDTO planCreateDTO)
        {   
            var result = await planService.CreatePlan(planCreateDTO);

            var planDTO = result.Data;

            return CreatedAtAction("GetPlanById", new {id =  planDTO.Id}, planDTO);
        }

        [Authorize(Policy = "UserPolicy")]
        [HttpGet("{id:int}")]
        public async Task<ActionResult<PlanDTO>> GetPlanById([FromRoute] int id)
        {
            var result = await planService.GetById(id);

            if(!result.IsSuccess)
            {
                return StatusCode(result.StatusCode, result.Message);
            }

            return Ok(result.Data);
        }

        [Authorize(Policy = "UserPolicy")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PlanDTO>>> GetAllPlans()
        {
            var result = await planService.GetAll();

            var planDTOList = result.Data;

            return Ok(planDTOList);
        }

        [Authorize(Policy = "AdminPolicy")]
        [HttpPut("{id:int}")]
        public async Task<ActionResult> UpdatePlan([FromRoute] int id, [FromBody] PlanUpdateDTO planUpdateDTO)
        {
            var result = await planService.UpdatePlan(id, planUpdateDTO);

            if (!result.IsSuccess)
            {
                return StatusCode(result.StatusCode, result.Message);
            }

            var planDTO = result.Data;

            return Ok(planDTO);
        }
    }
}