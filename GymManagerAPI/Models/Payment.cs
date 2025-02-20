using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GymManagerAPI.Models
{
    public class Payment
    {
        public int Id { get; set; }

        public int SubscriptionId { get; set; }
        public Subscription Subscription { get; set; }

        [Required]
        public DateTime DateTime { get; set; }

        [Required]
        public double TotalAmount { get; set; }

        //[NotMapped]
        public ICollection<PaymentDetail> PaymentDetails { get; set; } //navigation property to set an one to many relation with PaymentDetails
    }
}
