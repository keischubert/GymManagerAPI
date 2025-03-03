namespace GymManagerAPI.Models
{
    public class DeletedSubscription
    {
        public int Id { get; set; }

        public int SubscriptionId { get; set; }

        public Subscription Subscription { get; set; }

        public string DeletedBy { get; set; }

        public DateTime DeletedAt { get; set; }
    }
}
