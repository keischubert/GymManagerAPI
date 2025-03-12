namespace GymManagerAPI.Interfaces
{
    public interface IRepository<T> where T : class
    {
        Task AddAsync(T model);
        Task SaveChangesAsync();
        Task<IEnumerable<T>> GetAllAsync();
        Task<T> GetByIdAsync(int id);
    }
}
