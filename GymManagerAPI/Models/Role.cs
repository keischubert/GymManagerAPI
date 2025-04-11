using System.ComponentModel.DataAnnotations;

namespace GymManagerAPI.Models
{
    public class Role
    {
        public int Id { get; set; }

        [Required]
        [StringLength(maximumLength: 50)]
        public string Name { get; set; }

        [Required]
        [StringLength(maximumLength: 100)]
        public string Description { get; set; }

        public ICollection<UserRole> UserRoles { get; set; } //navigation property

    }
}
