using GymManagerAPI.Data.Context;
using GymManagerAPI.Interfaces;
using GymManagerAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace GymManagerAPI.Repositories
{
    public class GenderRepository : GenericRepository<Gender>, IGenderRepository
    {
        private readonly ApplicationDbContext dbContext;

        public GenderRepository(ApplicationDbContext dbContext) : base(dbContext) 
        {
            this.dbContext = dbContext;
        }

        public async Task<bool> DoesGenderExists(int id)
        {
            return await dbContext.Genders.AnyAsync(x => x.Id == id);
        }
    }
}
