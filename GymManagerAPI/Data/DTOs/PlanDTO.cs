using System.ComponentModel.DataAnnotations;

namespace GymManagerAPI.Data.DTOs
{
    public class PlanDTO
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public double Price { get; set; }

        public int DurationInDays { get; set; }
    }
}
