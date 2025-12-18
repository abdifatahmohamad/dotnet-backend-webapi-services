using System.ComponentModel.DataAnnotations;

namespace HospitalSystem.Api.Models
{
    public class User
    {
        // Primary Key for the table (Users)
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        // Human readable name (required)
        [Required]
        [MaxLength(100)]
        public string FullName { get; set; } = string.Empty;

        // Unique login identity (required)
        [Required]
        [EmailAddress]
        [MaxLength(200)]
        public string Email { get; set; } = string.Empty;

        // We NEVER store plain password. Only store a hash.
        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        // Role used by JWT + [Authorize(Roles="Admin")]
        [Required]
        [MaxLength(20)]
        public string Role { get; set; } = "Patient"; // default role if you want

        // True only for users created from the public /api/Auth/register flow
        public bool IsSelfRegistered { get; set; } = false;

        // Optional: makes your “system admin” rule cleaner (instead of relying only on a fixed GUID)
        public bool IsSystemAccount { get; set; } = false;

        // Audit fields (recommended)
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;

        // Optional: helper method to update UpdatedAtUtc consistently
        public void Touch()
        {
            UpdatedAtUtc = DateTime.UtcNow;
        }
    }
}
