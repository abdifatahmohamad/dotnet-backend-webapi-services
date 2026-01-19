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

        // ✅ POST /api/doctor - Admin creates a new doctor or auto-assigns staff
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<object>> CreateOrAssignDoctor(CreateDoctorRequest request)
        {
            var normalizedEmail = request.Email.Trim().ToLower();

            // 1) Try to find existing Doctor (with current assignments)
            var existingDoctor = await _context.Doctors
                .Include(d => d.User)
                .Include(d => d.Nurses).ThenInclude(n => n.User)
                .Include(d => d.Patients).ThenInclude(p => p.User)
                .FirstOrDefaultAsync(d => d.Email.ToLower() == normalizedEmail);

            Doctor doctor;
            bool isNewDoctor = false;

            if (existingDoctor == null)
            {
                // 1a) Make sure email is not already used by some *other* user
                var existingUser = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email.ToLower() == normalizedEmail);

                if (existingUser != null && existingUser.Role != "Doctor")
                {
                    // There is already a non-doctor user with this email
                    return Conflict(new
                    {
                        message = "A user with this email already exists and is not a doctor.",
                        email = request.Email
                    });
                }

                // 2) Create base User with hashed password (like /register)
                var user = existingUser ?? new User
                {
                    Id = Guid.NewGuid(),
                    FullName = request.FullName,
                    Email = request.Email,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                    Role = "Doctor",
                    ClinicalContext = request.Department
                };

                // 3) Create Doctor profile linked to this User
                doctor = new Doctor
                {
                    UserId = user.Id,
                    FullName = request.FullName,
                    Email = request.Email,
                    Specialty = request.Specialty,
                    Department = request.Department,
                    Password = "Hidden",
                    User = user
                };

                if (existingUser == null)
                {
                    _context.Users.Add(user);
                }

                _context.Doctors.Add(doctor);
                isNewDoctor = true;

                // Save so doctor has a real key and relationships can point to it
                await _context.SaveChangesAsync();
            }
            else
            {
                // 4) Doctor already exists – reuse it
                doctor = existingDoctor;
            }

            // 5) Find NEW nurses to auto-assign (Department == Doctor.Specialty, not already assigned)
            var nursesToAssign = await _context.Nurses
                .Include(n => n.User)
                .Where(n =>
                    n.AssignedDoctorId == null &&
                    n.Department == doctor.Specialty)
                .ToListAsync();

            // 6) Find NEW patients to auto-assign (Ailment == Doctor.Specialty, not already assigned)
            var patientsToAssign = await _context.Patients
                .Include(p => p.User)
                .Where(p =>
                    p.AssignedDoctorId == null &&
                    p.Ailment == doctor.Specialty)
                .ToListAsync();

            // 7) If doctor already exists *and* there are no new matches → return a guarded response
            if (!isNewDoctor && nursesToAssign.Count == 0 && patientsToAssign.Count == 0)
            {
                // Reload existing doctor with current assignments (in case context changed)
                var currentDoctor = await _context.Doctors
                    .Include(d => d.Nurses).ThenInclude(n => n.User)
                    .Include(d => d.Patients).ThenInclude(p => p.User)
                    .FirstAsync(d => d.UserId == doctor.UserId);

                var currentResponse = BuildDoctorResponse(currentDoctor);

                return Conflict(new
                {
                    message = "Doctor with this email already exists. No new nurses or patients were auto-assigned.",
                    doctor = currentResponse
                });
            }

            // A) Load all doctors in this specialty with their current patients (for balancing)
            var doctorsInSpecialty = await _context.Doctors
                .Include(d => d.Patients)
                .Where(d => d.Specialty == doctor.Specialty)
                .ToListAsync();

            if (doctorsInSpecialty.Count == 0)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    message = "No doctors found for this specialty while attempting auto-assignment."
                });
            }


            // 8) Assign nurses (simple: attach all new matching unassigned nurses to THIS doctor)
            foreach (var nurse in nursesToAssign)
            {
                nurse.AssignedDoctorId = doctor.UserId;
            }

            // 9) Assign patients (balanced):
            // For each patient:
            // - pick doctor with fewest current patients
            // - pick nurse with fewest current patients (matching specialty)
            foreach (var patient in patientsToAssign)
            {
                var targetDoctor = ChooseDoctorWithFewestPatients(doctorsInSpecialty);
                patient.AssignedDoctorId = targetDoctor.UserId;

                // Keep in-memory counts accurate for the next iteration
                targetDoctor.Patients.Add(patient);

                // Nurse assignment requires Patient.AssignedNurseId to exist
                var targetNurse = await ChooseNurseWithFewestPatientsAsync(doctor.Specialty);
                if (targetNurse != null)
                {
                    patient.AssignedNurseId = targetNurse.UserId;
                }
            }

            await _context.SaveChangesAsync();

            // 9) Build response with *all* current assignments (old + newly added)
            var doctorWithAllAssignments = await _context.Doctors
                .Include(d => d.Nurses).ThenInclude(n => n.User)
                .Include(d => d.Patients).ThenInclude(p => p.User)
                .FirstAsync(d => d.UserId == doctor.UserId);

            var response = BuildDoctorResponse(doctorWithAllAssignments);

            if (isNewDoctor)
            {
                // New doctor created
                return CreatedAtAction(nameof(GetDoctor), new { id = doctor.UserId }, response);
            }

            // Existing doctor got new assignments
            return Ok(new
            {
                message = "Existing doctor found. New matching nurses/patients were auto-assigned.",
                doctor = response
            });
        }

        /// <summary>
        /// Helper to build a safe DTO with all current assignments.
        /// </summary>
        private static DoctorResponse BuildDoctorResponse(Doctor doctor)
        {
            return new DoctorResponse
            {
                UserId = doctor.UserId,
                FullName = doctor.FullName,
                Email = doctor.Email,
                Specialty = doctor.Specialty,
                Department = doctor.Department,
                Nurses = doctor.Nurses
                    .Select(n => new AssignedPersonDto
                    {
                        UserId = n.UserId,
                        FullName = n.FullName,
                        Role = n.User.Role
                    })
                    .ToList(),
                Patients = doctor.Patients
                    .Select(p => new AssignedPersonDto
                    {
                        UserId = p.UserId,
                        FullName = p.FullName,
                        Role = p.User.Role
                    })
                    .ToList()
            };
        }

        /// <summary>
        /// Helper method to choose a doctor for assignment.
        /// Chooses the doctor with the fewest currently assigned patients.
        /// Expects doctors list to already include Patients navigation.
        /// </summary>
        private static Doctor ChooseDoctorWithFewestPatients(List<Doctor> doctorsInSpecialty)
        {
            return doctorsInSpecialty
                .OrderBy(d => d.Patients.Count)
                .First();
        }

        /// <summary>
        /// Helper method to choose a nurse for assignment.
        /// Chooses the nurse (matching specialty/department) who currently has the fewest assigned patients.
        /// Assumes Patient has Guid? AssignedNurseId.
        /// </summary>
        private async Task<Nurse?> ChooseNurseWithFewestPatientsAsync(string specialty)
        {
            var nursesWithLoad = await _context.Nurses
                .Where(n => n.Department == specialty)
                .Select(n => new
                {
                    Nurse = n,
                    PatientCount = _context.Patients.Count(p => p.AssignedNurseId == n.UserId)
                })
                .OrderBy(x => x.PatientCount)
                .ToListAsync();

            return nursesWithLoad.FirstOrDefault()?.Nurse;
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
