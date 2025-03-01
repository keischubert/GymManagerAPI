using System.ComponentModel.DataAnnotations;

namespace GymManagerAPI.Models
{
    public class Subscription
    {
        public int Id { get; set; }

        public int MemberId { get; set; }
        public Member Member { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime ExpirationDate { get; set; }

        [Required]
        public Payment Payment { get; set; } //navigation property
    }
}
