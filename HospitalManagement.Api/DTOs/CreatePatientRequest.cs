namespace HospitalManagement.Api.DTOs
{
    public class CreatePatientRequest
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;

        // Required for creating a patient
        public string Ailment { get; set; } = string.Empty;
    }
}