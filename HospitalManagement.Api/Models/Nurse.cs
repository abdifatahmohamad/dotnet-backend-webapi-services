using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HospitalManagement.Api.Models
{
    public class Nurse
    {
        [Key]
        public Guid UserId { get; set; }

        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        // Navigation to base User
        [ForeignKey(nameof(UserId))]
        public User User { get; set; } = null!;

        // Relationships - assigned Doctor
        public Guid? AssignedDoctorId { get; set; } // FK
        public Doctor? AssignedDoctor { get; set; }

        public List<Patient> Patients { get; set; } = new();
    }
}

