using GymManagerAPI.Data.Context;
using GymManagerAPI.Interfaces;
using GymManagerAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace GymManagerAPI.Repositories
{
    public class PlanRepository : GenericRepository<Plan>, IPlanRepository
    {
        private readonly ApplicationDbContext dbContext;

        public PlanRepository(ApplicationDbContext dbContext) : base(dbContext) 
        {
            this.dbContext = dbContext;
        }
        
    }
}
