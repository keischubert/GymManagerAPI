using GymManagerAPI.Models;

namespace GymManagerAPI.Data.DTOs
{
    public class PaymentDetailCreateDTO
    {
        public int PaymentMethodId { get; set; }
        public double Amount { get; set; }
    }
}
