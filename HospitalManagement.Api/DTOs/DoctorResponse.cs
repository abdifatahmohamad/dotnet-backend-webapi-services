namespace HospitalManagement.Api.DTOs
{
    /** 
        This is the response for:
        POST /api/doctor
        GET /api/doctor/{id}
        GET /api/doctor (logged-in doctor)
    **/
    public class DoctorResponse
    {
        public Guid UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Specialty { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;

        // Assigned staff (auto-populated by server logic)
        public List<AssignedPersonDto> Nurses { get; set; } = new();
        public List<AssignedPersonDto> Patients { get; set; } = new();
    }
}
