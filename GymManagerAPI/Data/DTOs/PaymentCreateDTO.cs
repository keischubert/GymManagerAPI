using GymManagerAPI.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GymManagerAPI.Data.DTOs
{
    public class PaymentCreateDTO
    {
        public int PlanId { get; set; }

        [Required]
        public ICollection<PaymentDetailCreateDTO> PaymentDetails { get; set; }
    }
}
