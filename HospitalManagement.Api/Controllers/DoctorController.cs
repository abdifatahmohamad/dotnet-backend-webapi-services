using HospitalManagement.Api.Data;
using HospitalManagement.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace HospitalManagement.Api.Controllers
{
    [Route("api/doctor")]
    [ApiController]
    [Authorize(Roles = "Admin,Doctor")]
    public class DoctorController : ControllerBase
    {
        private readonly HospitalDbContext _context;
        private readonly IHttpContextAccessor _httpContext;

        public DoctorController(HospitalDbContext context, IHttpContextAccessor httpContext)
        {
            _context = context;
            _httpContext = httpContext;
        }

        // ✅ GET /api/doctor - Get currently logged-in doctor's info
        [HttpGet]
        public async Task<ActionResult<Doctor>> GetMyDoctorProfile()
        {
            var email = _httpContext.HttpContext?.User?.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(email)) return Unauthorized();

            var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.Email == email);
            return doctor == null ? NotFound() : Ok(doctor);
        }

        // ✅ GET /api/doctor/{id} - Admin can get any doctor by ID
        [HttpGet("{id}")]
        public async Task<ActionResult<Doctor>> GetDoctor(Guid id)
        {
            var role = _httpContext.HttpContext?.User?.FindFirst(ClaimTypes.Role)?.Value;
            var email = _httpContext.HttpContext?.User?.FindFirst(ClaimTypes.Email)?.Value;

            if (string.IsNullOrEmpty(role) || string.IsNullOrEmpty(email)) return Unauthorized();

            if (role != "Admin")
            {
                var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.UserId == id && d.Email == email);
                return doctor == null ? Forbid() : Ok(doctor);
            }

            var found = await _context.Doctors.FindAsync(id);
            return found == null ? NotFound() : Ok(found);
        }

        // ✅ POST /api/doctor - Admin creates a new doctor
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<Doctor>> CreateDoctor(Doctor doctor)
        {
            _context.Doctors.Add(doctor);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetDoctor), new { id = doctor.UserId }, doctor);
        }

        // ✅ PUT /api/doctor/{id} - Admin or Doctor updates their own record
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateDoctor(Guid id, Doctor updatedDoctor)
        {
            var role = _httpContext.HttpContext?.User?.FindFirst(ClaimTypes.Role)?.Value;
            var email = _httpContext.HttpContext?.User?.FindFirst(ClaimTypes.Email)?.Value;

            if (string.IsNullOrEmpty(role) || string.IsNullOrEmpty(email)) return Unauthorized();

            if (role != "Admin" && updatedDoctor.Email != email)
                return Forbid();

            if (id != updatedDoctor.UserId)
                return BadRequest("Doctor ID mismatch.");

            var existing = await _context.Doctors.FindAsync(id);
            if (existing == null) return NotFound();

            existing.FullName = updatedDoctor.FullName;
            existing.Specialty = updatedDoctor.Specialty;

            _context.Doctors.Update(existing);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // ✅ DELETE /api/doctor/{id} - Admin only
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteDoctor(Guid id)
        {
            var doctor = await _context.Doctors.FindAsync(id);
            if (doctor == null) return NotFound();

            _context.Doctors.Remove(doctor);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
