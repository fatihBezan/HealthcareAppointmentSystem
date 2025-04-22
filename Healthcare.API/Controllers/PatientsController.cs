using Healthcare.Application.DTOs;
using Healthcare.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Healthcare.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PatientsController : ControllerBase
    {
        private readonly IPatientService _patientService;
        private readonly IAuthService _authService;

        public PatientsController(IPatientService patientService, IAuthService authService)
        {
            _patientService = patientService;
            _authService = authService;
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<PatientDto>>> GetAllPatients()
        {
            var patients = await _patientService.GetAllPatientsAsync();
            return Ok(patients);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<PatientDto>> GetPatientById(int id)
        {
            // Get the user ID from the JWT token
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            // Check if the user is an admin
            var isAdmin = await _authService.IsInRoleAsync(int.Parse(userId), "Admin");
            
            // If not admin, check if trying to access own patient record
            if (!isAdmin)
            {
                var ownPatient = await _patientService.GetPatientByUserIdAsync(int.Parse(userId));
                if (ownPatient.Id != id)
                {
                    return Forbid();
                }
            }

            var patient = await _patientService.GetPatientByIdAsync(id);
            return Ok(patient);
        }

        [HttpGet("user")]
        public async Task<ActionResult<PatientDto>> GetPatientByUser()
        {
            // Get the user ID from the JWT token
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var patient = await _patientService.GetPatientByUserIdAsync(int.Parse(userId));
            return Ok(patient);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<PatientDto>> CreatePatient(CreatePatientDto createPatientDto)
        {
            var patient = await _patientService.CreatePatientAsync(createPatientDto);
            return CreatedAtAction(nameof(GetPatientById), new { id = patient.Id }, patient);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<PatientDto>> UpdatePatient(int id, UpdatePatientDto updatePatientDto)
        {
            // Get the user ID from the JWT token
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            // Check if the user is an admin
            var isAdmin = await _authService.IsInRoleAsync(int.Parse(userId), "Admin");
            
            // If not admin, check if trying to update own patient record
            if (!isAdmin)
            {
                var ownPatient = await _patientService.GetPatientByUserIdAsync(int.Parse(userId));
                if (ownPatient.Id != id)
                {
                    return Forbid();
                }
            }

            var patient = await _patientService.UpdatePatientAsync(id, updatePatientDto);
            return Ok(patient);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> DeletePatient(int id)
        {
            var result = await _patientService.DeletePatientAsync(id);
            return Ok(new { success = result });
        }
    }
} 