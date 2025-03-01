using GymManagerAPI.Models;
using System.ComponentModel.DataAnnotations;

namespace GymManagerAPI.Data.DTOs
{
    public class SubscriptionDTO
    {
        public int Id { get; set; }

        public int MemberId { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime ExpirationDate { get; set; }
    }
}
