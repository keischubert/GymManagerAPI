using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GymManagerAPI.Models
{
    public class Payment
    {
        public int Id { get; set; }

        [Required]
        public DateTime DateTime { get; set; }

        public double TotalAmount { get; set; }

        public int SubscriptionId { get; set; }
        public Subscription Subscription { get; set; }

        public int PlanId { get; set; }

        [Required]
        public Plan Plan { get; set; }
    }
}
