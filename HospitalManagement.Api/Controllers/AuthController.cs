using HospitalManagement.Api.Data;
using HospitalManagement.Api.DTOs;
using HospitalManagement.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace HospitalManagement.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly HospitalDbContext _context;
        private readonly IConfiguration _config;

        public AuthController(HospitalDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(UserRegisterDto dto)
        {
            if (_context.Users.Any(u => u.Email == dto.Email))
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
            return Ok(new { message = "User registered successfully." });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(UserLoginDto dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == dto.Email.ToLower());
            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
                return Unauthorized("Invalid credentials.");

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.FullName),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role) // Admin, Doctor, Nurse, Patient
            };

            var keyString = _config["JwtSettings:Key"]
                ?? throw new InvalidOperationException("JWT signing key is missing in configuration.");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyString));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _config["JwtSettings:Issuer"],
                audience: _config["JwtSettings:Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(2),
                signingCredentials: creds
            );

            return Ok(new
            {
                token = new JwtSecurityTokenHandler().WriteToken(token)
            });
        }
    }
}
