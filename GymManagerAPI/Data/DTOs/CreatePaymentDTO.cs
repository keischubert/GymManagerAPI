using GymManagerAPI.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GymManagerAPI.Data.DTOs
{
    public class CreatePaymentDTO
    {
        public int SubscriptionId { get; set; }

        [Required]
        public DateTime DateTime { get; set; }

        [Required]
        public double Amount { get; set; }

        public ICollection<CreatePaymentDetailDTO> PaymentDetails { get; set; }
    }
}
