namespace HospitalManagement.Api.DTOs
{
    public class CreateDoctorRequest
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;

        // Required for creating a doctor
        public string Specialty { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
    }
}

