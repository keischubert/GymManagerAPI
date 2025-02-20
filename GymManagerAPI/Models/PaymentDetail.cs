using System.ComponentModel.DataAnnotations;

namespace GymManagerAPI.Models
{
    public class PaymentDetail
    {
        public int Id { get; set; }

        public int PaymentId { get; set; } //relation many to 1 with Payments
        public Payment Payment { get; set; } //navigation property

        [Required]
        public int Quantity { get; set; }

        public int PlanId { get; set; } //relation many to 1 with Memberships
        public Plan Plan { get; set; } //navigation property
    }
}
