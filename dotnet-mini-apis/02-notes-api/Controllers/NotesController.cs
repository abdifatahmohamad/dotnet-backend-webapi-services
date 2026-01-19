using Microsoft.AspNetCore.Mvc;
using Notes.Api.Models;
using Notes.Api.DTOs;

namespace Notes.Api.Controllers
{
    /// <summary>
    /// Controller for managing notes.
    /// </summary>
    [Route("api/notes")]
    [ApiController]
    public class NotesController : ControllerBase
    {
        // In-memory data store (replace with a service in a real application)
        private static readonly List<Note> _notes = new List<Note>
        {
            new Note
            {
                Id = 1,
                Title = "First Note",
                Content = "This is the content of the first note."
            },
            new Note
            {
                Id = 2,
                Title = "Second Note",
                Content = "This is the content of the second note."
            }
        };

        private static int _nextId = 3;

        /// <summary>
        /// Maps a Note to a NoteDto.
        /// </summary>
        private static NoteDto MapToDto(Note note)
        {
            return new NoteDto
            {
                Id = note.Id,
                Title = note.Title,
                Content = note.Content,
                CreatedAt = note.CreatedAt,
                UpdatedAt = note.UpdatedAt
            };
        }

        /// <summary>
        /// Retrieves all notes.
        /// </summary>
        [HttpGet]
        public ActionResult<IEnumerable<NoteDto>> GetNotes()
        {
            var noteDtos = _notes.Select(MapToDto).ToList();
            return Ok(noteDtos);
        }

        /// <summary>
        /// Retrieves a specific note by ID.
        /// </summary>
        [HttpGet("{id}")]
        public ActionResult<NoteDto> GetNoteById(int id)
        {
            var note = _notes.FirstOrDefault(n => n.Id == id);
            if (note == null)
            {
                return NotFound(); // Status code 404
            }
            var noteDto = MapToDto(note);
            return Ok(noteDto); // Status code 200
        }

        /// <summary>
        /// Creates a new note.
        /// </summary>
        [HttpPost]
        public ActionResult<NoteDto> CreateNote(CreateNoteDto createNoteDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState); // Status code 400
            }
            var newNote = new Note
            {
                Id = _nextId++,
                Title = createNoteDto.Title,
                Content = createNoteDto.Content,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            _notes.Add(newNote);
            var noteDto = MapToDto(newNote);
            return CreatedAtAction(
                nameof(GetNoteById),
                new { id = newNote.Id },
                noteDto
            ); // Status code 201   
        }

        /// <summary>
        /// Updates an existing note.
        /// </summary>
        [HttpPut("{id}")]
        public ActionResult<NoteDto> UpdateNote(int id, UpdateNoteDto updateNoteDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState); // Status code 400
            }
            var existingNote = _notes.FirstOrDefault(n => n.Id == id);
            if (existingNote == null)
            {
                return NotFound(); // Status code 404
            }

            existingNote.Title = updateNoteDto.Title;
            existingNote.Content = updateNoteDto.Content;
            existingNote.UpdatedAt = DateTime.UtcNow;

            var noteDto = MapToDto(existingNote);
            return Ok(noteDto); // Status code 200
        }

        /// <summary>
        /// Deletes a note by ID.
        /// </summary>
        [HttpDelete("{id}")]
        public ActionResult DeleteNote(int id)
        {
            var existingNote = _notes.FirstOrDefault(n => n.Id == id);
            if (existingNote == null)
            {
                return NotFound(); // Status code 404
            }
            _notes.Remove(existingNote);
            return NoContent(); // Status code 204
        }
    }
}