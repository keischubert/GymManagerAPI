using System.ComponentModel.DataAnnotations;

namespace GymManagerAPI.Models
{
    public class Member
    {
        public int Id { get; set; }

        [Required]
        public int GenderId { get; set; }
        public Gender Gender { get; set; }

        [Required]
        [StringLength(maximumLength: 50)]
        public string Name { get; set; }

        [Required]
        [StringLength(maximumLength: 20)]
        public string Ci { get; set; }

        [EmailAddress]
        [StringLength(maximumLength: 50)]
        public string Email { get; set; }

        [StringLength(maximumLength: 20)]
        public string PhoneNumber { get; set; }

        public ICollection<Subscription> Subscriptions { get; set; }
    }
}
