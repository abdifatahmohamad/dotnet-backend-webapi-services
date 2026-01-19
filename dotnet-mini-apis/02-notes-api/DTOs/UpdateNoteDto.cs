using System.ComponentModel.DataAnnotations;

namespace Notes.Api.DTOs
{
    public class UpdateNoteDto
    {
        [Required]
        [StringLength(100, MinimumLength = 1)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Content { get; set; } = string.Empty;
    }
}