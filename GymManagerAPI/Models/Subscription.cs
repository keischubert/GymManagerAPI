using System.ComponentModel.DataAnnotations;

namespace GymManagerAPI.Models
{
    public class Subscription
    {
        public int Id { get; set; }
        public int MemberId { get; set; }
        public Member Member { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime ExpirationDate { get; set; }
        public bool IsDeleted { get; set; } //soft delete
        public Payment Payment { get; set; } //navigation property
        public DeletedSubscription DeletedSubscription { get; set; }



    }
}
