using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HospitalManagement.Api.Models
{
    public class Patient
    {
        [Key]
        public Guid UserId { get; set; } // PK

        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Ailment { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        // Navigation to base User
        [ForeignKey(nameof(UserId))]
        public User User { get; set; } = null!;

        // Relationships - assigned doctor
        public Guid? AssignedDoctorId { get; set; } // FK
        public Doctor? AssignedDoctor { get; set; }

        // Relationships - assigned nurses
        public Guid? AssignedNurseId { get; set; } // FK
        public Nurse? AssignedNurse { get; set; }
    }
}

