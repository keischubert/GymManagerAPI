namespace GymManagerAPI.Data.DTOs
{
    public class SubscriptionListDTO
    {
        public int Id { get; set; }

        public string PlanName { get; set; } //mapping from Plan

        public DateTime StartDate { get; set; }

        public DateTime ExpirationDate { get; set; }

        public double TotalAmount {  get; set; } //mapping from Payment
    }
}
