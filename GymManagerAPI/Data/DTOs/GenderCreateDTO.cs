using System.ComponentModel.DataAnnotations;

namespace GymManagerAPI.Data.DTOs
{
    public class GenderCreateDTO
    {
        [Required]
        public string Name { get; set; }
    }
}
