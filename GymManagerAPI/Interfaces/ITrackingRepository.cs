namespace GymManagerAPI.Interfaces
{
    public interface ITrackingRepository<T> where T : class
    {
        Task<T> GetByIdTrackingAsync(int id);
    }
}
