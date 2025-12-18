namespace HospitalSystem.Api.DTOs
{
    public class AdminUpdateUserDto
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;

        // Plaintext password (optional). If provided, we will hash it.
        public string Password { get; set; } = string.Empty;

        // Optional role-specific fields
        public string Specialty { get; set; } = string.Empty;   // for doctors
        public string Department { get; set; } = string.Empty;  // for nurses
        public string Ailment { get; set; } = string.Empty;     // for patients
    }
}