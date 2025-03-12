using GymManagerAPI.Models;

namespace GymManagerAPI.Interfaces
{
    public interface IGenderRepository : IRepository<Gender>
    {
        Task<bool> DoesGenderExists(int id);
    }
}
