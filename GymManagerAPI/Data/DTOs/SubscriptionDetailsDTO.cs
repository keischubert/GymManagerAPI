namespace GymManagerAPI.Data.DTOs
{
    public class SubscriptionDetailsDTO
    {
        public int Id { get; set; }

        public string MemberName {  get; set; }

        public string PlanName { get; set; } //mapping from Payment.Plan

        public DateTime StartDate { get; set; }

        public DateTime ExpirationDate { get; set; }

        public DateTime PaymentDate { get; set; } //mapping from Payment

        public double TotalAmount { get; set; } //mapping from Payment
    }
}
