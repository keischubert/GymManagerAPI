using GymManagerAPI.Data.Context;
using GymManagerAPI.Data.DTOs;
using GymManagerAPI.Interfaces;
using GymManagerAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace GymManagerAPI.Repositories
{
    public class SubscriptionRepository : GenericRepository<Subscription>, ISubscriptionRepository
    {
        private readonly ApplicationDbContext dbContext;

        public SubscriptionRepository(ApplicationDbContext dbContext) : base(dbContext) 
        {
            this.dbContext = dbContext;
        }

        public async Task AddCascadeAsync(Subscription subscription)
        {
            await dbContext.AddAsync(subscription);
        }

        public async Task<IEnumerable<Subscription>> GetFilteredSubscriptions(SubscriptionSearchDTO subscriptionSearchDTO)
        {
            var query = dbContext.Subscriptions
                .Include(x => x.Member)
                .Include(x => x.Payment)
                .ThenInclude(x => x.Plan)
                .AsQueryable();

            if (subscriptionSearchDTO.PaymentDate.HasValue && !subscriptionSearchDTO.PaymentDateEnd.HasValue)
            {
                query = query
                    .Where(x => x.Payment.DateTime.Date == subscriptionSearchDTO.PaymentDate);
            }

            if (subscriptionSearchDTO.PaymentDate.HasValue && subscriptionSearchDTO.PaymentDateEnd.HasValue)
            {
                query = query
                    .Where(x => x.Payment.DateTime.Date >= subscriptionSearchDTO.PaymentDate && x.Payment.DateTime.Date <= subscriptionSearchDTO.PaymentDateEnd);
            }

            if (subscriptionSearchDTO.ExpirationDate.HasValue)
            {
                query = query
                    .Where(x => x.ExpirationDate.Date.Equals(subscriptionSearchDTO.ExpirationDate.Value.Date));
            }

            return await query.ToListAsync();
        }

        public async Task<Subscription> GetSubscriptionByIdWithDetails(int id) 
        {
            var subscription = await dbContext.Subscriptions
                .Include(x => x.Member)
                .Include(x => x.Payment)
                .ThenInclude(x => x.Plan)
                .FirstOrDefaultAsync(x => x.Id == id);

            return subscription;
        }

        public async Task<IEnumerable<Subscription>> GetSubscriptionsByMemberId(int memberId)
        {
            return await dbContext.Subscriptions
                .Where(s => s.MemberId == memberId)
                .OrderByDescending(s => s.ExpirationDate)
                .Include(s => s.Payment)
                .ThenInclude(p => p.Plan)
                .ToListAsync();
        }

        public async Task SoftDelete(Subscription subscription)
        {
            subscription.IsDeleted = true;
            dbContext.Subscriptions.Update(subscription);

            //registramos el softdelete en DeletedSubscriptions
            var deletedSubscription = new DeletedSubscription()
            {
                SubscriptionId = subscription.Id,
                DeletedBy = "Juan",
                DeletedAt = DateTime.Now
            };

            await dbContext.DeletedSubscriptions.AddAsync(deletedSubscription);
        }
    }
}