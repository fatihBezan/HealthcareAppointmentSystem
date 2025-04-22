using Healthcare.Application.DTOs;
using Healthcare.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Healthcare.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HospitalsController : ControllerBase
    {
        private readonly IHospitalService _hospitalService;

        public HospitalsController(IHospitalService hospitalService)
        {
            _hospitalService = hospitalService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<HospitalDto>>> GetAllHospitals()
        {
            var hospitals = await _hospitalService.GetAllHospitalsAsync();
            return Ok(hospitals);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<HospitalDto>> GetHospitalById(int id)
        {
            var hospital = await _hospitalService.GetHospitalByIdAsync(id);
            return Ok(hospital);
        }

        [HttpGet("city/{city}")]
        public async Task<ActionResult<IEnumerable<HospitalDto>>> GetHospitalsByCity(string city)
        {
            var hospitals = await _hospitalService.GetHospitalsByCityAsync(city);
            return Ok(hospitals);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<HospitalDto>> CreateHospital(CreateHospitalDto createHospitalDto)
        {
            var hospital = await _hospitalService.CreateHospitalAsync(createHospitalDto);
            return CreatedAtAction(nameof(GetHospitalById), new { id = hospital.Id }, hospital);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<HospitalDto>> UpdateHospital(int id, UpdateHospitalDto updateHospitalDto)
        {
            var hospital = await _hospitalService.UpdateHospitalAsync(id, updateHospitalDto);
            return Ok(hospital);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> DeleteHospital(int id)
        {
            var result = await _hospitalService.DeleteHospitalAsync(id);
            return Ok(new { success = result });
        }
    }
} 