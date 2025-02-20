namespace GymManagerAPI.Models
{
    public class Subscription
    {
        public int Id { get; set; }

        public int MemberId { get; set; }
        public Member Member { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime ExpirationDate { get; set; }

        public ICollection<Payment> Payments { get; set; }
    }
}
