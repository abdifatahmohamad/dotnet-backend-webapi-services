namespace HospitalManagement.Api.DTOs
{
    public class AssignedPersonDto // Used to return nurse or patient info inside a doctor response.
    {
        public Guid UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }
}
