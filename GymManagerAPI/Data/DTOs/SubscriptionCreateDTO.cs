using System.ComponentModel.DataAnnotations;
using GymManagerAPI.Models;

namespace GymManagerAPI.Data.DTOs
{
    public class SubscriptionCreateDTO
    {
        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public PaymentCreateDTO Payment { get; set; }
    }
}
