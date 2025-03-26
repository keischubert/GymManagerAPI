using AutoMapper;
using GymManagerAPI.Data.Common;
using GymManagerAPI.Data.DTOs;
using GymManagerAPI.Interfaces;
using GymManagerAPI.Models;

namespace GymManagerAPI.Services
{
    public class PlanService
    {
        private readonly IPlanRepository planRepository;
        private readonly IMapper mapper;

        public PlanService(IPlanRepository planRepository, IMapper mapper)
        {
            this.planRepository = planRepository;
            this.mapper = mapper;
        }

        public async Task<OperationResult<PlanDTO>> CreatePlan(PlanCreateDTO planCreateDTO)
        {
            var plan = mapper.Map<Plan>(planCreateDTO);

            await planRepository.AddAsync(plan);
            await planRepository.SaveChangesAsync();

            var planDTO = mapper.Map<PlanDTO>(plan);

            return OperationResult<PlanDTO>.Ok(planDTO);
        }

        public async Task<OperationResult<IEnumerable<PlanDTO>>> GetAll()
        {
            var planList = await planRepository.GetAllAsync();

            var planDTOList = mapper.Map<IEnumerable<PlanDTO>>(planList);

            return OperationResult<IEnumerable<PlanDTO>>.Ok(planDTOList);
        }

        public async Task<OperationResult<PlanDTO>> GetById(int id)
        {
            //validation: existencia del plan segun el id obtenido
            var plan = await planRepository.GetByIdAsync(id);

            if (plan == null)
            {
                return OperationResult<PlanDTO>.Fail(404, "No existe ningun plan con el id proporcionado");
            }

            var planDTO = mapper.Map<PlanDTO>(plan);

            return OperationResult<PlanDTO>.Ok(planDTO);
        }

        public async Task<OperationResult<PlanDTO>> UpdatePlan(int id, PlanUpdateDTO planUpdateDTO)
        {
            //validation: verificar existencia del plan segun el id obtenido
            var plan = await planRepository.GetByIdAsync(id, withTrack: true);

            if (plan == null)
            {
                return OperationResult<PlanDTO>.Fail(404, "No existe ningun plan con el id proporcionado");
            }

            //update: solo los campos que fueron proporcionados
            if (planUpdateDTO.Name != null && !plan.Name.Equals(planUpdateDTO.Name))
            {
                plan.Name = planUpdateDTO.Name;
            }

            if (planUpdateDTO.Price != null && plan.Price != planUpdateDTO.Price)
            {
                plan.Price = planUpdateDTO.Price ?? 0;
            }

            if (planUpdateDTO.DurationInDays != null && plan.DurationInDays != planUpdateDTO.DurationInDays)
            {
                plan.DurationInDays = planUpdateDTO.DurationInDays ?? 0;
            }

            planRepository.Update(plan);
            await planRepository.SaveChangesAsync();

            var planDTO = mapper.Map<PlanDTO>(plan);

            return OperationResult<PlanDTO>.Ok(planDTO);
        }
    }
}