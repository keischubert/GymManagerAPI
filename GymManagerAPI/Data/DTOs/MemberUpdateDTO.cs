using System.ComponentModel.DataAnnotations;
using GymManagerAPI.Models;

namespace GymManagerAPI.Data.DTOs
{
    public class MemberUpdateDTO
    {
        public int GenderId { get; set; }

        [StringLength(maximumLength: 50)]
        public string Name { get; set; }

        [StringLength(maximumLength: 20)]
        public string Ci { get; set; }

        [EmailAddress]
        [StringLength(maximumLength: 50)]
        public string Email { get; set; }

        [StringLength(maximumLength: 20)]
        public string PhoneNumber { get; set; }
    }
}
