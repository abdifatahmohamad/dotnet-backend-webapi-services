namespace HospitalManagement.Api.DTOs
{
    public class AdminUpdateUserDto
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;

        // Plaintext password (optional). If provided, we will hash it.
        public string? Password { get; set; }

        // Optional role-specific fields (used to keep ClinicalContext in sync)
        public string? Specialty { get; set; }   // Doctor
        public string? Department { get; set; }  // Nurse
        public string? Ailment { get; set; }     // Patient
    }
}
