namespace HospitalManagement.Api.DTOs
{
    public class CreateNurseRequest
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;

        // Required for creating a nurse
        public string DeleteDoctor { get; set; } = string.Empty;
    }
}