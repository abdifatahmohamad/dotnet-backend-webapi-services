using HospitalSystem.Api.Data;
using HospitalSystem.Api.DTOs;
using HospitalSystem.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace HospitalSystem.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly HospitalDbContext _context;
        private readonly IConfiguration _config;

        // ✅ Keep cookie + JWT expiration consistent (avoid “cookie expires but JWT still valid” confusion)
        private const int AuthMinutes = 30;

        // ✅ Centralize cookie settings so Login/Register/Logout always match
        private const string AccessTokenCookieName = "access_token";

        public AuthController(HospitalDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        // ------------------------------------------------------------
        // POST: /api/auth/register
        // Purpose:
        // - Allows ONLY Patients to self-register (Admin creates staff accounts)
        // - Optionally auto-logs-in the new patient by setting the auth cookie
        // Security:
        // - Does NOT return token in JSON
        // ------------------------------------------------------------
        [HttpPost("register")]
        public async Task<IActionResult> Register(UserRegisterDto dto)
        {
            // ✅ Normalize inputs
            var normalizedEmail = (dto.Email ?? string.Empty).Trim().ToLowerInvariant();
            var requestedRole = (dto.Role ?? string.Empty).Trim();

            // 1) Endpoint rule: ONLY Patient can self-register
            if (!string.Equals(requestedRole, "Patient", StringComparison.OrdinalIgnoreCase))
                return BadRequest("Only Patient can self-register. Admin creates staff accounts.");

            // 2) Business rule: Patients cannot self-register with hospital email domain
            if (normalizedEmail.EndsWith("@hospital.com", StringComparison.OrdinalIgnoreCase))
                return BadRequest("Patients cannot self-register with a hospital email.");

            // 3) Uniqueness: email must be unique
            var exists = await _context.Users.AnyAsync(u => u.Email.ToLower() == normalizedEmail);
            if (exists)
                return BadRequest("User with this email already exists.");

            // 4) Hash password before storing
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

            // 5) Create user record (Patient only)
            var user = new User
            {
                Id = Guid.NewGuid(),
                FullName = (dto.FullName ?? string.Empty).Trim(),
                Email = normalizedEmail,
                PasswordHash = passwordHash,
                Role = "Patient",
                IsSelfRegistered = true,
                IsSystemAccount = false,
                CreatedAtUtc = DateTime.UtcNow,
                UpdatedAtUtc = DateTime.UtcNow
            };

            // 6) Save to DB
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // 7) Optional: auto-login after register (cookie-based session)
            //    - Generate JWT
            //    - Store it in HttpOnly cookie (not returned to frontend)
            var token = GenerateJwtToken(user);
            SetAuthCookie(token);

            return Ok(new { message = "User registered successfully." });
        }

        // ------------------------------------------------------------
        // POST: /api/auth/login
        // Purpose:
        // - Validate credentials
        // - Set HttpOnly cookie containing JWT
        // Security:
        // - Returns minimal response (no token / no profile in JSON)
        // ------------------------------------------------------------
        [HttpPost("login")]
        public async Task<IActionResult> Login(UserLoginDto dto)
        {
            var normalizedEmail = (dto.Email ?? string.Empty).Trim().ToLowerInvariant();

            // 1) Lookup user by email
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == normalizedEmail);

            // 2) Validate credentials
            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
                return Unauthorized("Invalid credentials.");

            // 3) Issue JWT and store in HttpOnly cookie
            var token = GenerateJwtToken(user);
            SetAuthCookie(token);

            return Ok(new { message = "Login successful." });
        }

        // ------------------------------------------------------------
        // GET: /api/auth/me
        // Purpose:
        // - Used by frontend to confirm session + display minimal user info
        // Security:
        // - Only returns what UI needs (id/name/email/role)
        // - Requires authentication (cookie -> JWT)
        // ------------------------------------------------------------
        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> Me()
        {
            // JWT middleware populates User.Claims from the cookie token
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!Guid.TryParse(userIdStr, out var userId))
                return Unauthorized();

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return Unauthorized();

            return Ok(new
            {
                userId = user.Id,
                fullName = user.FullName,
                email = user.Email,
                role = user.Role
            });
        }

        // ------------------------------------------------------------
        // POST: /api/auth/logout
        // Purpose:
        // - Clears the auth cookie so the browser session is ended
        // IMPORTANT:
        // - Cookie deletion MUST match the cookie options used when setting it
        //   (Path, SameSite, Secure, etc.)
        // ------------------------------------------------------------
        [HttpPost("logout")]
        public IActionResult Logout()
        {
            ClearAuthCookie();
            return Ok(new { success = true, message = "Logged out successfully." });
        }

        // =========================
        // Cookie helpers (centralized to avoid mismatches)
        // =========================

        /// <summary>
        /// Sets the JWT into an HttpOnly cookie. Angular sends it automatically via withCredentials:true.
        /// SameSite=None is required for cross-site cookie (localhost:4200 -> localhost:8080).
        /// Secure must be true when SameSite=None (browser requirement).
        /// </summary>
        private void SetAuthCookie(string token)
        {
            Response.Cookies.Append(AccessTokenCookieName, token, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,                 // required when SameSite=None
                SameSite = SameSiteMode.None,  // allow cross-site cookie for Angular dev
                Expires = DateTimeOffset.UtcNow.AddMinutes(AuthMinutes),
                Path = "/"
            });
        }

        /// <summary>
        /// Clears the auth cookie. Must use the SAME cookie options (especially SameSite/Path) as SetAuthCookie.
        /// Some browsers are picky; to be extra safe, we also overwrite the cookie with an expired value.
        /// </summary>
        private void ClearAuthCookie()
        {
            // Delete using matching options
            Response.Cookies.Delete(AccessTokenCookieName, new CookieOptions
            {
                Secure = true,
                SameSite = SameSiteMode.None,
                Path = "/",
                HttpOnly = true
            });

            // Extra-safe overwrite/expire (helps with stubborn browser behaviors)
            Response.Cookies.Append(AccessTokenCookieName, string.Empty, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Path = "/",
                Expires = DateTimeOffset.UtcNow.AddDays(-1)
            });
        }

        // =========================
        // JWT generator
        // =========================

        /// <summary>
        /// Generates a signed JWT containing the minimal claims the API needs for authorization:
        /// - NameIdentifier (user id)
        /// - Name, Email, Role
        /// IMPORTANT:
        /// - JWT expiration MUST match cookie expiration to avoid inconsistent sessions.
        /// </summary>
        private string GenerateJwtToken(User user)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.FullName ?? string.Empty),
                new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
                new Claim(ClaimTypes.Role, user.Role ?? string.Empty)
            };

            var keyString = _config["JwtSettings:Key"]
                ?? throw new InvalidOperationException("JWT signing key is missing in configuration (JwtSettings:Key).");

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyString));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _config["JwtSettings:Issuer"],
                audience: _config["JwtSettings:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(AuthMinutes), // ✅ match cookie expiration
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
