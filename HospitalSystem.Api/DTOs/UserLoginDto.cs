using System.ComponentModel.DataAnnotations;

namespace HospitalSystem.Api.DTOs
{
    /// <summary>
    /// Data Transfer Object used when a user logs in.
    /// This represents credentials only (not a database entity).
    /// </summary>
    public class UserLoginDto
    {
        [Required]
        [EmailAddress]
        [MaxLength(150)]
        public string Email { get; set; } = string.Empty;
        // - Required: login cannot happen without email
        // - EmailAddress: ensures valid format
        // - MaxLength: protects DB & avoids abuse

        [Required]
        public string Password { get; set; } = string.Empty;
        // - Required: password must be provided
        // - Plain password exists ONLY here
        // - Will be verified against PasswordHash
    }
}
