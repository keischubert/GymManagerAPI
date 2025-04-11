using System.ComponentModel.DataAnnotations;

namespace GymManagerAPI.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required]
        [StringLength(maximumLength: 100)]
        public string FullName { get; set; }

        [Required]
        [StringLength(maximumLength: 50)]
        public string UserName { get; set; }

        [Required]
        public byte[] PasswordHash { get; set; }

        [Required]
        public byte[] PasswordSalt { get; set; }

        public bool IsActive { get; set; }

        public ICollection<UserRole> UserRoles { get; set; } //navigation property
        public ICollection<Subscription> Subscriptions { get; set; } //navigation property
        public ICollection<DeletedSubscription> DeletedSubscriptions { get; set; } //navigation property
        public ICollection<RefreshToken> RefreshTokens { get; set; } //navigation property

    }
}
