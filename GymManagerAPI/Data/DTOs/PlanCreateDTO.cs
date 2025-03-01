using System.ComponentModel.DataAnnotations;

namespace GymManagerAPI.Data.DTOs
{
    public class PlanCreateDTO
    {
        [Required]
        [StringLength(maximumLength: 20)]
        public string Name { get; set; }

        [Required]
        [Range(0, 250000)]
        public double Price { get; set; }

        [Required]
        [Range(1, 30)]
        public int DurationInDays { get; set; }
    }
}
