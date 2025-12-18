using System.ComponentModel.DataAnnotations;

namespace HospitalSystem.Api.DTOs
{
    /// <summary>
    /// Data Transfer Object used by Admins to create users.
    /// Unlike self-registration, this is a privileged operation.
    /// </summary>
    public class AdminCreateUserDto
    {
        // -------------------------
        // Core User Information
        // -------------------------

        [Required]
        [MaxLength(100)]
        public string FullName { get; set; } = string.Empty;
        // Admin must provide a name for the user

        [Required]
        [EmailAddress]
        [MaxLength(150)]
        public string Email { get; set; } = string.Empty;
        // Used as login identifier
        // Must be unique (validated in service/controller)

        [Required]
        [MinLength(8)]
        public string Password { get; set; } = string.Empty;
        // Plain password:
        // - Exists ONLY in DTO
        // - Will be hashed before saving

        [Required]
        public string Role { get; set; } = string.Empty;
        // Expected values:
        // "Doctor", "Nurse", "Patient"
        // Admin role validation happens in logic layer

        // -------------------------
        // Role-Specific Fields
        // -------------------------

        public string Specialty { get; set; } = string.Empty; // for Doctors
        public string Department { get; set; } = string.Empty; // for Nurses
        public string Ailment { get; set; } = string.Empty; // for Patients
    }
}
