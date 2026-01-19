using HospitalManagement.Api.Data;
using HospitalManagement.Api.Models;
using HospitalManagement.Api.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace HospitalManagement.Api.Controllers
{
    [ApiController]
    [Route("api/admin/assignments")]
    [Authorize(Roles = "Admin")]
    public class AdminAssignmentsController : ControllerBase
    {
        private readonly HospitalDbContext _context;

        public AdminAssignmentsController(HospitalDbContext context)
        {
            _context = context;
        }

        // ✅ Assign Nurse -> Doctor
        // POST /api/admin/assignments/doctor-nurse
        [HttpPost("doctor-nurse")]
        public async Task<IActionResult> AssignNurseToDoctor(Guid doctorId, Guid nurseId)
        {
            var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.UserId == doctorId);
            var nurse = await _context.Nurses.FirstOrDefaultAsync(n => n.UserId == nurseId);

            if (doctor == null || nurse == null) return NotFound("Doctor or Nurse not found.");

            nurse.AssignedDoctorId = doctor.UserId;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Nurse assigned to doctor." });
        }

        // ✅ Unassign Nurse from Doctor
        // DELETE /api/admin/assignments/doctor-nurse
        [HttpDelete("doctor-nurse")]
        public async Task<IActionResult> UnassignNurseFromDoctor(Guid nurseId)
        {
            var nurse = await _context.Nurses.FirstOrDefaultAsync(n => n.UserId == nurseId);
            if (nurse == null) return NotFound("Nurse not found.");

            nurse.AssignedDoctorId = null;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Nurse unassigned from doctor." });
        }

        // ✅ Assign Patient -> Doctor
        // POST /api/admin/assignments/doctor-patient
        [HttpPost("doctor-patient")]
        public async Task<IActionResult> AssignPatientToDoctor(Guid doctorId, Guid patientId)
        {
            var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.UserId == doctorId);
            var patient = await _context.Patients.FirstOrDefaultAsync(p => p.UserId == patientId);

            if (doctor == null || patient == null) return NotFound("Doctor or Patient not found.");

            patient.AssignedDoctorId = doctor.UserId;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Patient assigned to doctor." });
        }

        // ✅ Unassign Patient from Doctor
        // DELETE /api/admin/assignments/doctor-patient
        [HttpDelete("doctor-patient")]
        public async Task<IActionResult> UnassignPatientFromDoctor(Guid patientId)
        {
            var patient = await _context.Patients.FirstOrDefaultAsync(p => p.UserId == patientId);
            if (patient == null) return NotFound("Patient not found.");

            patient.AssignedDoctorId = null;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Patient unassigned from doctor." });
        }

        // ✅ Assign Patient -> Nurse
        // POST /api/admin/assignments/nurse-patient
        [HttpPost("nurse-patient")]
        public async Task<IActionResult> AssignPatientToNurse(Guid nurseId, Guid patientId)
        {
            var nurse = await _context.Nurses.FirstOrDefaultAsync(n => n.UserId == nurseId);
            var patient = await _context.Patients.FirstOrDefaultAsync(p => p.UserId == patientId);

            if (nurse == null || patient == null) return NotFound("Nurse or Patient not found.");

            patient.AssignedNurseId = nurse.UserId;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Patient assigned to nurse." });
        }

        // ✅ Unassign Patient from Nurse
        // DELETE /api/admin/assignments/nurse-patient
        [HttpDelete("nurse-patient")]
        public async Task<IActionResult> UnassignPatientFromNurse(Guid patientId)
        {
            var patient = await _context.Patients.FirstOrDefaultAsync(p => p.UserId == patientId);
            if (patient == null) return NotFound("Patient not found.");

            patient.AssignedNurseId = null;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Patient unassigned from nurse." });
        }
    }
}
