using GymManagerAPI.Models;

namespace GymManagerAPI.Data.DTOs
{
    public class CreateSubscriptionDTO
    {
        public DateTime StartDate { get; set; }

        public DateTime ExpirationDate { get; set; }

        public ICollection<CreatePaymentDTO> Payments { get; set; }
    }
}
