using Healthcare.Application.DTOs;

namespace Healthcare.Application.Interfaces
{
    public interface IPatientService
    {
        Task<IEnumerable<PatientDto>> GetAllPatientsAsync();
        Task<PatientDto> GetPatientByIdAsync(int id);
        Task<PatientDto> GetPatientByUserIdAsync(int userId);
        Task<PatientDto> CreatePatientAsync(CreatePatientDto createPatientDto);
        Task<PatientDto> UpdatePatientAsync(int id, UpdatePatientDto updatePatientDto);
        Task<bool> DeletePatientAsync(int id);
    }
} 