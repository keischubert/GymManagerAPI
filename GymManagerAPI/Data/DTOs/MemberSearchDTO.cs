using Microsoft.AspNetCore.Mvc;

namespace GymManagerAPI.Data.DTOs
{
    public class MemberSearchDTO
    {
        public string Name { get; set; }
        public int? GenderId { get; set; }
        public string Ci { get; set; }
        public string Email { get; set; }
        public DateTime? ActiveMembersFromDate { get; set; }
    }
}
