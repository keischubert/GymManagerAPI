using System.ComponentModel.DataAnnotations;

namespace GymManagerAPI.Data.DTOs
{
    public class PlanUpdateDTO
    {
        public string Name { get; set; }
        public double? Price { get; set; }
        public int? DurationInDays { get; set; }
    }
}
