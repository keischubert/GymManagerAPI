using System.ComponentModel.DataAnnotations;

namespace GymManagerAPI.Models
{
    public class Gender
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        public ICollection<Member> Members { get; set; }
    }
}
