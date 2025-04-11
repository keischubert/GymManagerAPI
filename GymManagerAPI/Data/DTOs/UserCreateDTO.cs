using System.ComponentModel.DataAnnotations;

namespace GymManagerAPI.Data.DTOs
{
    public class UserCreateDTO
    {
        [Required]
        [StringLength(maximumLength: 100)]
        public string FullName { get; set; }

        [Required]
        [StringLength(maximumLength: 50)]
        public string UserName { get; set; }

        [Required]
        [StringLength(maximumLength: 50)]
        public string Password { get; set; }
    }
}
