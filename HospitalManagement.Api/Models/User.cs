using System.ComponentModel.DataAnnotations;

namespace HospitalManagement.Api.Models
{
    public class User
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty; // "Doctor", "Nurse", or "Patient"
        // Department / Specialty / Ailment (role-based meaning)
        public string? ClinicalContext { get; set; }
    }
}

