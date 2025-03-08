using System.ComponentModel.DataAnnotations;

namespace GymManagerAPI.Models
{
    public class PaymentMethod
    {
        public int Id { get; set; }

        [Required]
        [StringLength(maximumLength: 50)]
        public string Name { get; set; }

        public ICollection<PaymentDetail> PaymentDetails { get; set; }
    }
}
