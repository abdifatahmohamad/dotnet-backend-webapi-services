using HospitalManagement.Api.Data;
using HospitalManagement.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace HospitalManagement.Api.Controllers
{
    [Authorize]
    [Route("api/profile")]
    public class UserProfileController : ControllerBase
    {
        private readonly HospitalDbContext _context;
        private readonly IHttpContextAccessor _httpContext;

        public UserProfileController(HospitalDbContext context, IHttpContextAccessor httpContext)
        {
            _context = context;
            _httpContext = httpContext;
        }

        [HttpGet]
        public async Task<ActionResult<User>> GetOwnProfile()
        {
            var email = _httpContext?.HttpContext?.User?.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(email)) return Forbid();

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            return user == null ? NotFound() : user;
        }
    }

}
