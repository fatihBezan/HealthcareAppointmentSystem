using Healthcare.Application.DTOs;

namespace Healthcare.Application.Interfaces
{
    public interface IHospitalService
    {
        Task<IEnumerable<HospitalDto>> GetAllHospitalsAsync();
        Task<HospitalDto> GetHospitalByIdAsync(int id);
        Task<IEnumerable<HospitalDto>> GetHospitalsByCityAsync(string city);
        Task<HospitalDto> CreateHospitalAsync(CreateHospitalDto createHospitalDto);
        Task<HospitalDto> UpdateHospitalAsync(int id, UpdateHospitalDto updateHospitalDto);
        Task<bool> DeleteHospitalAsync(int id);
    }
} 