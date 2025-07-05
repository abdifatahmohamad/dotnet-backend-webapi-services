namespace HospitalManagement.Api.DTOs
{
    public class UserRegisterDto
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty; // Doctor, Nurse, Patient
        
        // ✅ Supports role-specific registration
        public string? Specialty { get; set; } // for doctors
        public string? Department { get; set; } // for nurses
        public string? Ailment { get; set; } // for patients
    }
}

