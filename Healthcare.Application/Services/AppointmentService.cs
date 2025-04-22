using Healthcare.Application.DTOs;
using Healthcare.Application.Exceptions;
using Healthcare.Application.Interfaces;
using Healthcare.Domain.Entities;
using Healthcare.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace Healthcare.Application.Services
{
    public class AppointmentService : IAppointmentService
    {
        private readonly AppDbContext _context;
        private readonly IAuthService _authService;

        public AppointmentService(AppDbContext context, IAuthService authService)
        {
            _context = context;
            _authService = authService;
        }

        public async Task<IEnumerable<AppointmentDto>> GetAllAppointmentsAsync()
        {
            var appointments = await _context.Appointments
                .Include(a => a.Doctor)
                .Include(a => a.Patient)
                .ToListAsync();

            return appointments.Select(a => MapToAppointmentDto(a));
        }

        public async Task<AppointmentDto> GetAppointmentByIdAsync(int id)
        {
            var appointment = await _context.Appointments
                .Include(a => a.Doctor)
                .Include(a => a.Patient)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (appointment == null)
            {
                throw new AppException($"Appointment with ID {id} not found");
            }

            return MapToAppointmentDto(appointment);
        }

        public async Task<IEnumerable<AppointmentDto>> GetAppointmentsByPatientIdAsync(int patientId)
        {
            var appointments = await _context.Appointments
                .Include(a => a.Doctor)
                .Include(a => a.Patient)
                .Where(a => a.PatientId == patientId)
                .ToListAsync();

            return appointments.Select(a => MapToAppointmentDto(a));
        }

        public async Task<IEnumerable<AppointmentDto>> GetAppointmentsByDoctorIdAsync(int doctorId)
        {
            var appointments = await _context.Appointments
                .Include(a => a.Doctor)
                .Include(a => a.Patient)
                .Where(a => a.DoctorId == doctorId)
                .ToListAsync();

            return appointments.Select(a => MapToAppointmentDto(a));
        }

        public async Task<IEnumerable<AppointmentDto>> GetUserAppointmentsAsync(int userId)
        {
            // Find the patient associated with this user
            var patient = await _context.Patients
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (patient == null)
            {
                throw new AppException("No patient profile found for this user");
            }

            // Get appointments for this patient
            var appointments = await _context.Appointments
                .Include(a => a.Doctor)
                .Include(a => a.Patient)
                .Where(a => a.PatientId == patient.Id)
                .ToListAsync();

            return appointments.Select(a => MapToAppointmentDto(a));
        }

        public async Task<AppointmentDto> CreateAppointmentAsync(int userId, CreateAppointmentDto createAppointmentDto)
        {
            // Find the patient associated with this user
            var patient = await _context.Patients
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (patient == null)
            {
                throw new AppException("No patient profile found for this user");
            }

            // Check if the doctor exists
            var doctor = await _context.Doctors
                .FirstOrDefaultAsync(d => d.Id == createAppointmentDto.DoctorId);

            if (doctor == null)
            {
                throw new AppException($"Doctor with ID {createAppointmentDto.DoctorId} not found");
            }

            // Check if patient already has an appointment with this doctor within a week
            var oneWeekAgo = DateTime.Now.AddDays(-7);
            var oneWeekFromNow = DateTime.Now.AddDays(7);
            
            var existingAppointment = await _context.Appointments
                .Where(a => a.PatientId == patient.Id && 
                       a.DoctorId == createAppointmentDto.DoctorId &&
                       ((a.AppointmentDate >= oneWeekAgo && a.AppointmentDate <= DateTime.Now) ||
                        (a.AppointmentDate >= DateTime.Now && a.AppointmentDate <= oneWeekFromNow)))
                .FirstOrDefaultAsync();

            if (existingAppointment != null)
            {
                throw new AppointmentLimitExceededException($"{doctor.FirstName} {doctor.LastName}");
            }

            // Create the appointment
            var appointment = new Appointment
            {
                DoctorId = createAppointmentDto.DoctorId,
                PatientId = patient.Id,
                AppointmentDate = createAppointmentDto.AppointmentDate,
                Notes = createAppointmentDto.Notes
            };

            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();

            // Refresh the appointment with related entities
            appointment = await _context.Appointments
                .Include(a => a.Doctor)
                .Include(a => a.Patient)
                .FirstOrDefaultAsync(a => a.Id == appointment.Id);

            if (appointment != null)
            {
                return MapToAppointmentDto(appointment);
            }
            
            throw new AppException("Failed to create appointment");
        }

        public async Task<AppointmentDto> UpdateAppointmentAsync(int userId, int appointmentId, UpdateAppointmentDto updateAppointmentDto)
        {
            // Find the patient associated with this user
            var patient = await _context.Patients
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (patient == null)
            {
                throw new AppException("No patient profile found for this user");
            }

            // Check if the appointment exists and belongs to this patient
            var appointment = await _context.Appointments
                .Include(a => a.Doctor)
                .Include(a => a.Patient)
                .FirstOrDefaultAsync(a => a.Id == appointmentId);

            if (appointment == null)
            {
                throw new AppException($"Appointment with ID {appointmentId} not found");
            }

            // Check if user is admin or the appointment belongs to this patient
            var isAdmin = await _authService.IsInRoleAsync(userId, "Admin");
            
            if (!isAdmin && appointment.PatientId != patient.Id)
            {
                throw new Healthcare.Application.Exceptions.UnauthorizedAccessException("this appointment");
            }

            // Update the appointment
            appointment.AppointmentDate = updateAppointmentDto.AppointmentDate;
            appointment.Notes = updateAppointmentDto.Notes;

            await _context.SaveChangesAsync();

            return MapToAppointmentDto(appointment);
        }

        public async Task<bool> DeleteAppointmentAsync(int userId, int appointmentId)
        {
            // Find the patient associated with this user
            var patient = await _context.Patients
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (patient == null)
            {
                throw new AppException("No patient profile found for this user");
            }

            // Check if the appointment exists
            var appointment = await _context.Appointments
                .FirstOrDefaultAsync(a => a.Id == appointmentId);

            if (appointment == null)
            {
                throw new AppException($"Appointment with ID {appointmentId} not found");
            }

            // Check if user is admin or the appointment belongs to this patient
            var isAdmin = await _authService.IsInRoleAsync(userId, "Admin");
            
            if (!isAdmin && appointment.PatientId != patient.Id)
            {
                throw new Healthcare.Application.Exceptions.UnauthorizedAccessException("this appointment");
            }

            // Delete the appointment
            _context.Appointments.Remove(appointment);
            await _context.SaveChangesAsync();

            return true;
        }

        // Helper method to map Appointment entity to AppointmentDto
        private AppointmentDto MapToAppointmentDto(Appointment appointment)
        {
            return new AppointmentDto
            {
                Id = appointment.Id,
                DoctorId = appointment.DoctorId,
                DoctorFullName = $"{appointment.Doctor?.FirstName} {appointment.Doctor?.LastName}",
                PatientId = appointment.PatientId,
                PatientFullName = $"{appointment.Patient?.FirstName} {appointment.Patient?.LastName}",
                AppointmentDate = appointment.AppointmentDate,
                AppointmentDay = appointment.AppointmentDate.Day,
                AppointmentMonth = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(appointment.AppointmentDate.Month),
                AppointmentYear = appointment.AppointmentDate.Year,
                Notes = appointment.Notes
            };
        }
    }
} 