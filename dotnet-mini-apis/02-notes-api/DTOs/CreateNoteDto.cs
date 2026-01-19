using System.ComponentModel.DataAnnotations;

namespace Notes.Api.DTOs
{
    /// <summary>
    /// used for creating a new note and includes simple validation via data annotations.
    /// </summary>
    public class CreateNoteDto
    {
        [Required]
        [StringLength(100, MinimumLength = 1)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Content { get; set; } = string.Empty;
    }
}