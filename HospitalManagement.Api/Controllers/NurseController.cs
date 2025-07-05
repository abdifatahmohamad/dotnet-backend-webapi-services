using HospitalManagement.Api.Data;
using HospitalManagement.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace HospitalManagement.Api.Controllers
{
    [Authorize(Roles = "nurse")]
    [Route("api/[controller]")]
    [ApiController]
    public class NurseController : ControllerBase
    {
        private readonly HospitalDbContext _context;

        public NurseController(HospitalDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Nurse>>> GetNurses()
        {
            return await _context.Nurses
                .Include(n => n.AssignedDoctor)
                .ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Nurse>> GetNurse(Guid id)
        {
            var nurse = await _context.Nurses
                .Include(n => n.AssignedDoctor)
                .FirstOrDefaultAsync(n => n.UserId == id);

            return nurse == null ? NotFound() : nurse;
        }

        [HttpPost]
        public async Task<ActionResult<Nurse>> PostNurse(Nurse nurse)
        {
            _context.Nurses.Add(nurse);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetNurse), new { id = nurse.UserId }, nurse);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutNurse(Guid id, Nurse nurse)
        {
            if (id != nurse.UserId) return BadRequest();

            _context.Entry(nurse).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteNurse(int id)
        {
            var nurse = await _context.Nurses.FindAsync(id);
            if (nurse == null) return NotFound();

            _context.Nurses.Remove(nurse);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }

}
