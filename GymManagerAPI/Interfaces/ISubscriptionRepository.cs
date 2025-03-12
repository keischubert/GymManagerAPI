using GymManagerAPI.Data.DTOs;
using GymManagerAPI.Models;

namespace GymManagerAPI.Interfaces
{
    public interface ISubscriptionRepository : IRepository<Subscription>
    {
        Task AddCascadeAsync(Subscription subscription);
        Task<Subscription> GetSubscriptionByIdWithDetails(int id);
        Task<IEnumerable<Subscription>> GetSubscriptionsByMemberId(int memberId);
        Task<IEnumerable<Subscription>> GetFilteredSubscriptions(SubscriptionSearchDTO subscriptionSearchDTO);
        Task SoftDelete(Subscription subscription);

    }
}
