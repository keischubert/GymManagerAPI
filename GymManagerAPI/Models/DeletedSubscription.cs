namespace GymManagerAPI.Models
{
    public class DeletedSubscription
    {
        public int Id { get; set; }
        public int SubscriptionId { get; set; }
        public Subscription Subscription { get; set; } //navigation propery
        public int UserId { get; set; }
        public User User { get; set; } //navigation propery
        public DateTime DeletedAt { get; set; }
    }
}
