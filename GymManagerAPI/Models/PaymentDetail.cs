namespace GymManagerAPI.Models
{
    public class PaymentDetail
    {
        public int Id { get; set; }

        public int PaymentId { get; set; }
        public Payment Payment { get; set; } //navigation property

        public int PaymentMethodId { get; set; }
        public PaymentMethod PaymentMethod { get; set; } //navigation property

        public double Amount { get; set; }
    }
}
