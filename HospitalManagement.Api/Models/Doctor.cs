using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HospitalManagement.Api.Models
{
    public class Doctor
    {
        [Key]
        public Guid UserId { get; set; } // Primary key

        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Specialty { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        // Navigation to base User
        [ForeignKey(nameof(UserId))]
        public User User { get; set; } = null!;

        // Navigation for Patients and Nurses
        public ICollection<Patient> Patients { get; set; } = new List<Patient>();
        public ICollection<Nurse> Nurses { get; set; } = new List<Nurse>();
    }
}

