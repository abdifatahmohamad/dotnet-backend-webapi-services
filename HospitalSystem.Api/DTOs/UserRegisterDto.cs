using System.ComponentModel.DataAnnotations;

namespace HospitalSystem.Api.DTOs
{
    /// <summary>
    /// Data Transfer Object used when a user registers.
    /// This represents input data only (not a database entity).
    /// </summary>
    public class UserRegisterDto
    {
        // -------------------------
        // Core User Information
        // -------------------------

        [Required]
        [MaxLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [MaxLength(150)]
        public string Email { get; set; } = string.Empty;
        // - EmailAddress ensures valid format
        // - Unique check happens in service/controller, not DTO

        [Required]
        [MinLength(8)]
        public string Password { get; set; } = string.Empty;
        // - Plain password ONLY exists here
        // - Will be hashed before saving
        // - Never stored directly in DB

        [Required]
        public string Role { get; set; } = string.Empty; // Doctor, Nurse, Patient
        // Validation of allowed roles happens in logic layer

        // -------------------------
        // Role-Specific Fields
        // -------------------------

        public string Specialty { get; set; } = string.Empty; // for Doctors
        public string Department { get; set; } = string.Empty; // for Nurses
        public string Ailment { get; set; } = string.Empty; // for Patients
    }
}
