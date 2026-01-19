using HospitalManagement.Api.Data;
using HospitalManagement.Api.DTOs;
using HospitalManagement.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HospitalManagement.Api.Controllers
{
    [ApiController]
    [Route("api/admin/users")]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly HospitalDbContext _context;

        public AdminController(HospitalDbContext context)
        {
            _context = context;
        }

        // [POST] /api/admin/users - Create a new user (Doctor/Nurse/Patient)
        [HttpPost]
        public async Task<IActionResult> CreateUser(AdminCreateUserDto dto)
        {
            var normalizedEmail = dto.Email.Trim().ToLower();

            var emailExists = await _context.Users
                .AnyAsync(u => u.Email.ToLower() == normalizedEmail);

            if (emailExists)
                return BadRequest("User with this email already exists.");

            var roleNormalized = dto.Role.Trim().ToLower();

            // ✅ ClinicalContext mapping based on YOUR actual schema:
            // Doctor  -> Specialty
            // Nurse   -> Department
            // Patient -> Ailment
            // Admin   -> null
            string? clinicalContext = roleNormalized switch
            {
                "doctor" => dto.Specialty ?? "General",
                "nurse" => dto.Department ?? "General",
                "patient" => dto.Ailment ?? "Unspecified",
                "admin" => null,
                _ => null
            };

            if (clinicalContext == null && roleNormalized != "admin")
                return BadRequest("Invalid role. Must be Doctor, Nurse, Patient, or Admin.");

            var user = new User
            {
                Id = Guid.NewGuid(),
                FullName = dto.FullName,
                Email = dto.Email.Trim(),
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password), // ✅ same as /register
                Role = dto.Role.Trim(),
                ClinicalContext = clinicalContext
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync(); // ✅ save first so FK is valid

            switch (roleNormalized)
            {
                case "doctor":
                    _context.Doctors.Add(new Doctor
                    {
                        UserId = user.Id,
                        FullName = dto.FullName,
                        Email = dto.Email.Trim(),
                        Specialty = dto.Specialty ?? "General",
                        Password = "Hidden"
                    });
                    break;

                case "nurse":
                    _context.Nurses.Add(new Nurse
                    {
                        UserId = user.Id,
                        FullName = dto.FullName,
                        Email = dto.Email.Trim(),
                        Department = dto.Department ?? "General",
                        Password = "Hidden",
                        AssignedDoctorId = null
                    });
                    break;

                case "patient":
                    _context.Patients.Add(new Patient
                    {
                        UserId = user.Id,
                        FullName = dto.FullName,
                        Email = dto.Email.Trim(),
                        Ailment = dto.Ailment ?? "Unspecified",
                        Password = "Hidden",
                        AssignedDoctorId = null,
                        AssignedNurseId = null // ✅ only if your Patient model has it
                    });
                    break;

                case "admin":
                    // Admin has no profile table record
                    break;
            }

            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetUserById), new { id = user.Id }, user);
        }

        // [GET] /api/admin/users - Get all users or filtered by role
        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> GetUsers([FromQuery] string? role)
        {
            var query = _context.Users.AsQueryable();

            if (!string.IsNullOrEmpty(role))
            {
                var normalized = role.Trim().ToLower();
                query = query.Where(u => u.Role.ToLower() == normalized);
            }

            // Optional: order for consistent UI
            return await query
                .OrderBy(u => u.Role)
                .ThenBy(u => u.FullName)
                .ToListAsync();
        }

        // [GET] /api/admin/users/{id} - Get user by ID
        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetUserById(Guid id)
        {
            var user = await _context.Users.FindAsync(id);
            return user == null ? NotFound() : Ok(user);
        }

        // ✅ NEW: safer update DTO (recommended)
        // If you MUST keep "User updatedUser" input, see note below.
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(Guid id, AdminUpdateUserDto dto)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            user.FullName = dto.FullName.Trim();
            user.Email = dto.Email.Trim();
            user.Role = dto.Role.Trim();

            var roleNormalized = user.Role.Trim().ToLower();

            // If password provided, hash it (plaintext in dto)
            if (!string.IsNullOrWhiteSpace(dto.Password))
            {
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);
            }

            // Keep ClinicalContext consistent with role-based meaning
            user.ClinicalContext = roleNormalized switch
            {
                "doctor" => dto.Specialty ?? user.ClinicalContext ?? "General",
                "nurse" => dto.Department ?? user.ClinicalContext ?? "General",
                "patient" => dto.Ailment ?? user.ClinicalContext ?? "Unspecified",
                "admin" => null,
                _ => user.ClinicalContext
            };

            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // [DELETE] /api/admin/users/{id} - Delete a user
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(Guid id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
