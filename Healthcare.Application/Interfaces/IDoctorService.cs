using Healthcare.Application.DTOs;

namespace Healthcare.Application.Interfaces
{
    public interface IDoctorService
    {
        Task<IEnumerable<DoctorDto>> GetAllDoctorsAsync();
        Task<DoctorDto> GetDoctorByIdAsync(int id);
        Task<IEnumerable<DoctorDto>> GetDoctorsByHospitalAsync(int hospitalId);
        Task<IEnumerable<DoctorDto>> GetDoctorsBySpecialtyAsync(string specialty);
        Task<DoctorDto> CreateDoctorAsync(CreateDoctorDto createDoctorDto);
        Task<DoctorDto> UpdateDoctorAsync(int id, UpdateDoctorDto updateDoctorDto);
        Task<bool> DeleteDoctorAsync(int id);
    }
} 