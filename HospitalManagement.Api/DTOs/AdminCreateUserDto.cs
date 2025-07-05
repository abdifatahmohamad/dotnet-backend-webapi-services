namespace HospitalManagement.Api.DTOs
{
    public class AdminCreateUserDto
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty; // "Doctor", "Nurse", "Patient"

        // Optional fields depending on Role
        public string? Specialty { get; set; } // Doctor
        public string? Department { get; set; } // Nurse
        public string? Ailment { get; set; } // Patient
    }
}
