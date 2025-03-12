using GymManagerAPI.Data.DTOs;
using GymManagerAPI.Models;

namespace GymManagerAPI.Interfaces
{
    public interface IMemberRepository : IRepository<Member>
    {
        Task<IEnumerable<Member>> GetFilteredMembersAsync(MemberSearchDTO memberSearchDTO);
        Task<Member> GetByIdWithDetailsAsync(int id, bool details);
        void Update(Member member);
        Task<bool> DoesMemberExistsAsync (int id);
        Task<bool> DoesCiExistsAsync(string ci);
        Task<DateTime> MemberLastSubscriptionExpirationDate(int id);
    }
}
