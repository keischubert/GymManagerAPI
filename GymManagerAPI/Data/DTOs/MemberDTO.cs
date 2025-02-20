using System.ComponentModel.DataAnnotations;
using GymManagerAPI.Models;

namespace GymManagerAPI.Data.DTOs
{
    public class MemberDTO
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Gender { get; set; }

        public string Ci { get; set; }

        public string Email { get; set; }

        public string PhoneNumber { get; set; }
    }
}
