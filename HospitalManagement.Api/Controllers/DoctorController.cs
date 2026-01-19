using HospitalManagement.Api.Data;
using HospitalManagement.Api.Models;
using HospitalManagement.Api.DTOs;
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

        // GET /api/doctor - Get currently logged-in doctor's info
        [HttpGet]
        public async Task<ActionResult<Doctor>> GetMyDoctorProfile()
        {
            var email = _httpContext.HttpContext?.User?.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(email)) return Unauthorized();

            var doctor = await _context.Doctors
                .Include(d => d.Nurses).ThenInclude(n => n.User)
                .Include(d => d.Patients).ThenInclude(p => p.User)
                .FirstOrDefaultAsync(d => d.Email == email);

            return doctor == null ? NotFound() : Ok(doctor);
        }

        // GET /api/doctor/{id} - Admin can get any doctor, doctor can get self only
        [HttpGet("{id}")]
        public async Task<ActionResult<Doctor>> GetDoctor(Guid id)
        {
            var role = _httpContext.HttpContext?.User?.FindFirst(ClaimTypes.Role)?.Value;
            var email = _httpContext.HttpContext?.User?.FindFirst(ClaimTypes.Email)?.Value;

            if (string.IsNullOrEmpty(role) || string.IsNullOrEmpty(email)) return Unauthorized();

            if (role != "Admin")
            {
                var self = await _context.Doctors
                    .Include(d => d.Nurses).ThenInclude(n => n.User)
                    .Include(d => d.Patients).ThenInclude(p => p.User)
                    .FirstOrDefaultAsync(d => d.UserId == id && d.Email == email);

                return self == null ? Forbid() : Ok(self);
            }

            var found = await _context.Doctors
                .Include(d => d.Nurses).ThenInclude(n => n.User)
                .Include(d => d.Patients).ThenInclude(p => p.User)
                .FirstOrDefaultAsync(d => d.UserId == id);

            return found == null ? NotFound() : Ok(found);
        }
    }
}
