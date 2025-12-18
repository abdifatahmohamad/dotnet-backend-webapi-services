using System.Security.Claims;
using HospitalSystem.Api.Data;
using HospitalSystem.Api.DTOs;
using HospitalSystem.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HospitalSystem.Api.Controllers
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

        // POST: /api/admin/users
        [HttpPost]
        public async Task<IActionResult> CreateUser(AdminCreateUserDto dto)
        {
            var normalizedEmail = dto.Email.Trim().ToLower();
            var role = dto.Role.Trim();

            // 2) Validate role
            var allowedRoles = new[] { "Admin", "Doctor", "Nurse", "Patient" };
            if (!allowedRoles.Any(r => r.Equals(role, StringComparison.OrdinalIgnoreCase)))
                return BadRequest("Invalid role. Must be Admin, Doctor, Nurse, or Patient.");

            // 3) Enforce email rules (staff vs patient)
            // Staff (Admin/Doctor/Nurse) must use @hospital.com
            // Patient must NOT use @hospital.com
            var requiresHospitalEmail = RoleRequiresHospitalEmail(role);

            if (requiresHospitalEmail && !IsHospitalEmail(normalizedEmail))
                return BadRequest("Staff accounts must use a hospital email ending with @hospital.com.");

            if (!requiresHospitalEmail && IsHospitalEmail(normalizedEmail))
                return BadRequest("Patient accounts cannot use a hospital email ending with @hospital.com.");

            // 4) Check if email already exists
            var exists = await _context.Users.AnyAsync(u => u.Email.ToLower() == normalizedEmail);
            if (exists)
                return BadRequest("User with this email already exists.");

            // 5) Create user
            var user = new User
            {
                Id = Guid.NewGuid(),
                FullName = dto.FullName.Trim(),
                Email = normalizedEmail,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                Role = role,
                IsSelfRegistered = false,
                IsSystemAccount = false,
                CreatedAtUtc = DateTime.UtcNow,
                UpdatedAtUtc = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetUserById), new { id = user.Id }, new
            {
                user.Id,
                user.FullName,
                user.Email,
                user.Role
            });
        }

        // GET: /api/admin/users?role=Doctor
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetUsers([FromQuery] string? role)
        {
            var query = _context.Users.AsQueryable();

            if (!string.IsNullOrWhiteSpace(role))
            {
                var normalizedRole = role.Trim().ToLower();
                query = query.Where(u => u.Role.ToLower() == normalizedRole);
            }

            var users = await query
                .OrderBy(u => u.Role)
                .ThenBy(u => u.FullName)
                .Select(u => new
                {
                    u.Id,
                    u.FullName,
                    u.Email,
                    u.Role,
                    u.CreatedAtUtc
                })
                .ToListAsync();

            return Ok(users);
        }

        // GET: /api/admin/users/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById(Guid id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            return Ok(new
            {
                user.Id,
                user.FullName,
                user.Email,
                user.Role,
                user.CreatedAtUtc
            });
        }

        // PUT: /api/admin/users/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(Guid id, AdminUpdateUserDto dto)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            // Normalize inputs once (use these everywhere)
            var normalizedEmail = dto.Email.Trim().ToLower();
            var roleAfter = dto.Role.Trim();
            var roleBefore = user.Role;

            // Enforce email rules on update too (staff vs patient)
            var requiresHospitalEmailAfter = RoleRequiresHospitalEmail(roleAfter);

            if (requiresHospitalEmailAfter && !IsHospitalEmail(normalizedEmail))
                return BadRequest("Staff accounts must use a hospital email ending with @hospital.com.");

            if (!requiresHospitalEmailAfter && IsHospitalEmail(normalizedEmail))
                return BadRequest("Patient accounts cannot use a hospital email ending with @hospital.com.");


            // Email uniqueness check if email changed
            var emailTakenByOther = await _context.Users.AnyAsync(u => u.Id != id && u.Email.ToLower() == normalizedEmail);
            if (emailTakenByOther)
                return BadRequest("Another user already uses this email.");

            // ✅ Admin safety rules (BEST PLACE: before modifying the entity)
            var seededAdminId = Guid.Parse("11111111-1111-1111-1111-111111111111");

            // Prevent demoting the system admin
            if (id == seededAdminId &&
                !string.Equals(roleAfter, "Admin", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest("System Admin role cannot be changed.");
            }

            // Prevent demoting the last admin
            var isCurrentlyAdmin = string.Equals(roleBefore, "Admin", StringComparison.OrdinalIgnoreCase);
            var willBecomeNonAdmin = !string.Equals(roleAfter, "Admin", StringComparison.OrdinalIgnoreCase);

            if (isCurrentlyAdmin && willBecomeNonAdmin)
            {
                var adminCount = await _context.Users.CountAsync(u => u.Role.ToLower() == "admin");
                if (adminCount <= 1)
                    return BadRequest("You cannot demote the last Admin account.");
            }

            // Block changing Role for self-registered accounts
            // Example: a patient who signed up publicly should NOT become a Doctor via generic update.
            var roleChanging =
                !string.Equals(user.Role, roleAfter, StringComparison.OrdinalIgnoreCase);

            if (roleChanging && user.IsSelfRegistered)
            {
                return BadRequest("Role cannot be changed for self-registered accounts. Create a new staff account instead.");
            }

            // ✅ Now it's safe to update the entity
            user.FullName = dto.FullName.Trim();
            user.Email = normalizedEmail;
            user.Role = roleAfter;

            if (!string.IsNullOrWhiteSpace(dto.Password))
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

            user.Touch();

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE: /api/admin/users/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(Guid id)
        {
            // 1) Find user
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound("User not found.");

            // Fixed System Admin ID (bootstrap account)
            var systemAdminId = Guid.Parse("11111111-1111-1111-1111-111111111111");

            // 2) Prevent deleting the System Admin
            if (user.Id == systemAdminId)
                return BadRequest("System Admin account cannot be deleted.");

            // 3) Prevent admin from deleting themselves
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (currentUserId == user.Id.ToString())
                return BadRequest("You cannot delete your own account.");

            // 4) Prevent deleting the last Admin
            if (string.Equals(user.Role, "Admin", StringComparison.OrdinalIgnoreCase))
            {
                var adminCount = await _context.Users.CountAsync(u => u.Role.ToLower() == "admin");
                if (adminCount <= 1)
                    return BadRequest("You cannot delete the last Admin account.");
            }

            // 5) Delete user
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // ✅ Hospital email domain rule (centralized so we reuse it in Create + Update)
        private static bool IsHospitalEmail(string email)
        {
            // Keep it simple: only hospital.com is considered internal staff email.
            return email.EndsWith("@hospital.com", StringComparison.OrdinalIgnoreCase);
        }

        private static bool RoleRequiresHospitalEmail(string role)
        {
            return role.Equals("Admin", StringComparison.OrdinalIgnoreCase)
                || role.Equals("Doctor", StringComparison.OrdinalIgnoreCase)
                || role.Equals("Nurse", StringComparison.OrdinalIgnoreCase);
        }

    }
}
