using AutoMapper;
using GymManagerAPI.Data.Context;
using GymManagerAPI.Data.DTOs;
using GymManagerAPI.Interfaces;
using GymManagerAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace GymManagerAPI.Repositories
{
    public class MemberRepository : GenericRepository<Member>, IMemberRepository
    {
        private readonly ApplicationDbContext applicationDbContext;

        public MemberRepository(ApplicationDbContext applicationDbContext) : base(applicationDbContext) 
        {
            this.applicationDbContext = applicationDbContext;
        }

        public async Task<bool> DoesCiExistsAsync(string ci)
        {
            return await applicationDbContext.Members.AnyAsync(x => x.Ci.Equals(ci));
        }

        public async Task<bool> DoesMemberExistsAsync(int id)
        {
            return await applicationDbContext.Members.AnyAsync(x => x.Id == id);
        }

        public async Task<Member> GetByIdWithDetailsAsync(int id, bool details)
        {
            var query = applicationDbContext.Members.AsQueryable();

            if (details)
            {
                query = query
                    .Include(x => x.Gender)
                    .Include(x => x.Subscriptions);
            }

            return await query.FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<IEnumerable<Member>> GetFilteredMembersAsync(MemberSearchDTO memberSearchDTO)
        {
            var query = applicationDbContext.Members.AsQueryable();

            if (!string.IsNullOrWhiteSpace(memberSearchDTO.Name))
            {
                query = query.Where(x => x.Name.Contains(memberSearchDTO.Name));
            }

            if (memberSearchDTO.GenderId.HasValue)
            {
                query = query.Where(x => x.GenderId == memberSearchDTO.GenderId);
            }

            if (!string.IsNullOrWhiteSpace(memberSearchDTO.Ci))
            {
                query = query.Where(x => x.Ci.Equals(memberSearchDTO.Ci));
            }

            if (!string.IsNullOrWhiteSpace(memberSearchDTO.Email))
            {
                query = query.Where(x => x.Email.Equals(memberSearchDTO.Email));
            }

            if (memberSearchDTO.ActiveMembersFromDate.HasValue)
            {
                var activeMemberIds = await applicationDbContext.Subscriptions
                    .Where(x => x.ExpirationDate >= memberSearchDTO.ActiveMembersFromDate)
                    .GroupBy(x => x.MemberId)
                    .Select(x => x.Key)
                    .ToListAsync();

                query = query.Where(x => activeMemberIds.Contains(x.Id));
            }

            var memberList = await query.ToListAsync();

            return memberList; ;
        }

        public async Task<DateTime> MemberLastSubscriptionExpirationDate(int id)
        {
            var expirationDateLastSubscription = await applicationDbContext.Subscriptions
                .Where(s => s.MemberId == id)
                .OrderByDescending(s => s.ExpirationDate)
                .Select(x => x.ExpirationDate)
                .FirstOrDefaultAsync();

            return expirationDateLastSubscription;
        }

        public void Update(Member member)
        {
            applicationDbContext.Members.Update(member);
        }


    }
}
