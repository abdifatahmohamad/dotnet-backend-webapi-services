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
            if (_context.Users.Any(u => u.Email.ToLower() == dto.Email.ToLower()))
                return BadRequest("User with this email already exists.");

            var user = new User
            {
                Id = Guid.NewGuid(),
                FullName = dto.FullName,
                Email = dto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                Role = dto.Role
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync(); // ✅ Save user first to resolve FK dependency

            switch (dto.Role.ToLower())
            {
                case "doctor":
                    _context.Doctors.Add(new Doctor
                    {
                        UserId = user.Id, // ✅ Assign FK
                        FullName = dto.FullName,
                        Email = dto.Email,
                        Specialty = dto.Specialty ?? "General",
                        Password = "Hidden"
                    });
                    break;

                case "nurse":
                    _context.Nurses.Add(new Nurse
                    {
                        UserId = user.Id, // ✅ Assign FK
                        FullName = dto.FullName,
                        Email = dto.Email,
                        Department = dto.Department ?? "General",
                        Password = "Hidden"
                    });
                    break;

                case "patient":
                    _context.Patients.Add(new Patient
                    {
                        UserId = user.Id, // ✅ Assign FK
                        FullName = dto.FullName,
                        Email = dto.Email,
                        Ailment = dto.Ailment ?? "Unspecified",
                        Password = "Hidden"
                    });
                    break;

                default:
                    return BadRequest("Invalid role. Must be Doctor, Nurse, or Patient.");
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
                query = query.Where(u => u.Role.ToLower() == role.ToLower());

            return await query.ToListAsync();
        }

        // [GET] /api/admin/users/{id} - Get user by ID
        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetUserById(Guid id)
        {
            var user = await _context.Users.FindAsync(id);
            return user == null ? NotFound() : Ok(user);
        }

        // [PUT] /api/admin/users/{id} - Update a user
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(Guid id, User updatedUser)
        {
            if (id != updatedUser.Id)
                return BadRequest("User ID mismatch.");

            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound();

            user.FullName = updatedUser.FullName;
            user.Email = updatedUser.Email;
            user.Role = updatedUser.Role;

            // If password is provided, update it
            if (!string.IsNullOrWhiteSpace(updatedUser.PasswordHash))
            {
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(updatedUser.PasswordHash);
            }

            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // [DELETE] /api/admin/users/{id} - Delete a user
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(Guid id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound();

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
