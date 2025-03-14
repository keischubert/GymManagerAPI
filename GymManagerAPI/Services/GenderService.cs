using AutoMapper;
using GymManagerAPI.Data.Common;
using GymManagerAPI.Data.DTOs;
using GymManagerAPI.Interfaces;
using GymManagerAPI.Models;

namespace GymManagerAPI.Services
{
    public class GenderService
    {
        private readonly IGenderRepository genderRepository;
        private readonly IMapper mapper;

        public GenderService(IGenderRepository genderRepository, IMapper mapper) 
        {
            this.genderRepository = genderRepository;
            this.mapper = mapper;
        }

        public async Task<OperationResult<GenderDTO>> Create(GenderCreateDTO genderCreateDTO)
        {
            var gender = mapper.Map<Gender>(genderCreateDTO);

            await genderRepository.AddAsync(gender);

            await genderRepository.SaveChangesAsync();

            var genderDTO = mapper.Map<GenderDTO>(gender);

            return OperationResult<GenderDTO>.Ok(genderDTO);
        }

        public async Task<OperationResult<GenderDTO>> GetById(int id)
        {
            var gender = await genderRepository.GetByIdAsync(id);

            if (gender == null)
            {
                return OperationResult<GenderDTO>.Fail(404, "No existe ningun genero con el id proporcionado");
            }

            var genderDTO = mapper.Map<GenderDTO>(gender);

            return OperationResult<GenderDTO>.Ok(genderDTO);
        }

        public async Task<OperationResult<IEnumerable<GenderDTO>>> GetAll()
        {
            var genderList = await genderRepository.GetAllAsync();

            var genderListDTO = mapper.Map<IEnumerable<GenderDTO>>(genderList);

            return OperationResult<IEnumerable<GenderDTO>>.Ok(genderListDTO);
        }

        public async Task<OperationResult<GenderDTO>> UpdateGender(int id, GenderUpdateDTO genderUpdateDTO)
        {
            var gender = await genderRepository.GetByIdAsync(id);

            if (gender == null)
            {
                return OperationResult<GenderDTO>.Fail(404, "No existe ningun genero con el id proporcionado");
            }

            if(!string.IsNullOrEmpty(genderUpdateDTO.Name) && !gender.Name.Equals(genderUpdateDTO.Name))
            {
                gender.Name = genderUpdateDTO.Name;

                genderRepository.Update(gender);
                await genderRepository.SaveChangesAsync();
            }

            var genderDTO = mapper.Map<GenderDTO>(gender);

            return OperationResult<GenderDTO>.Ok(genderDTO);
        }


    }
}
