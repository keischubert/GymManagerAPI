using System.ComponentModel.DataAnnotations;
using GymManagerAPI.Models;

namespace GymManagerAPI.Data.DTOs
{
    public class CreatePaymentDetailDTO
    {
        public int PaymentId { get; set; }

        [Required]
        public int Quantity { get; set; }

        [Required]
        public int PlanId { get; set; }
    }
}
