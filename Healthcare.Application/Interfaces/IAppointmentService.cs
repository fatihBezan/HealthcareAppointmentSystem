using Healthcare.Application.DTOs;

namespace Healthcare.Application.Interfaces
{
    public interface IAppointmentService
    {
        Task<IEnumerable<AppointmentDto>> GetAllAppointmentsAsync();
        Task<AppointmentDto> GetAppointmentByIdAsync(int id);
        Task<IEnumerable<AppointmentDto>> GetAppointmentsByPatientIdAsync(int patientId);
        Task<IEnumerable<AppointmentDto>> GetAppointmentsByDoctorIdAsync(int doctorId);
        Task<IEnumerable<AppointmentDto>> GetUserAppointmentsAsync(int userId);
        Task<AppointmentDto> CreateAppointmentAsync(int userId, CreateAppointmentDto createAppointmentDto);
        Task<AppointmentDto> UpdateAppointmentAsync(int userId, int appointmentId, UpdateAppointmentDto updateAppointmentDto);
        Task<bool> DeleteAppointmentAsync(int userId, int appointmentId);
    }
} 