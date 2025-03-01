using AutoMapper;
using GymManagerAPI.Data.Context;
using GymManagerAPI.Data.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GymManagerAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GendersController : ControllerBase
    {
        private readonly ApplicationDbContext applicationDbContext;
        private readonly IMapper mapper;

        public GendersController(ApplicationDbContext applicationDbContext, IMapper mapper)
        {
            this.applicationDbContext = applicationDbContext;
            this.mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<GenderDTO>>> GetAll()
        {
            var genderList = await applicationDbContext.Genders.ToListAsync();

            var genderDTOList = mapper.Map<List<GenderDTO>>(genderList);

            return Ok(genderDTOList);
        }
    }
}
