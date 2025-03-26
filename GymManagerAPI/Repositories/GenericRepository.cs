using GymManagerAPI.Data.Context;
using GymManagerAPI.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GymManagerAPI.Repositories
{
    public class GenericRepository<T> : IRepository<T> where T : class
    {
        private readonly ApplicationDbContext dbContext;
        private readonly DbSet<T> table;

        public GenericRepository(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
            this.table = dbContext.Set<T>();
        }
        public async Task AddAsync(T model)
        {
            await table.AddAsync(model);
        }

        public async Task<IEnumerable<T>> GetAllAsync()
        {
            return await table.AsNoTracking().ToListAsync();
        }

        public async Task<T> GetByIdAsync(int id, bool withTrack = false)
        {
            return withTrack ? await table.FindAsync(id) : await table.AsNoTracking().FirstOrDefaultAsync(x => EF.Property<int>(x, "Id") == id);
        }

        public void Update(T model)
        {
            table.Update(model);
        }

        public async Task SaveChangesAsync()
        {
            await dbContext.SaveChangesAsync();
        }
    }
}
