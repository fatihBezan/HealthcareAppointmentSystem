using Healthcare.Application.DTOs;
using Healthcare.Application.Exceptions;
using Healthcare.Application.Interfaces;
using Healthcare.Domain.Entities;
using Healthcare.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Healthcare.Application.Services
{
    public class PatientService : IPatientService
    {
        private readonly AppDbContext _context;

        public PatientService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<PatientDto>> GetAllPatientsAsync()
        {
            var patients = await _context.Patients.ToListAsync();
            return patients.Select(p => MapToPatientDto(p));
        }

        public async Task<PatientDto> GetPatientByIdAsync(int id)
        {
            var patient = await _context.Patients
                .FirstOrDefaultAsync(p => p.Id == id);

            if (patient == null)
            {
                throw new AppException($"Patient with ID {id} not found");
            }

            return MapToPatientDto(patient);
        }

        public async Task<PatientDto> GetPatientByUserIdAsync(int userId)
        {
            var patient = await _context.Patients
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (patient == null)
            {
                throw new AppException($"No patient profile found for user with ID {userId}");
            }

            return MapToPatientDto(patient);
        }

        public async Task<PatientDto> CreatePatientAsync(CreatePatientDto createPatientDto)
        {
            // Check if the user exists and doesn't already have a patient profile
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == createPatientDto.UserId);

            if (user == null)
            {
                throw new AppException($"User with ID {createPatientDto.UserId} not found");
            }

            var existingPatient = await _context.Patients
                .FirstOrDefaultAsync(p => p.UserId == createPatientDto.UserId);

            if (existingPatient != null)
            {
                throw new AppException($"A patient profile already exists for user with ID {createPatientDto.UserId}");
            }

            var patient = new Patient
            {
                FirstName = createPatientDto.FirstName,
                LastName = createPatientDto.LastName,
                BirthDate = createPatientDto.BirthDate,
                UserId = createPatientDto.UserId
            };

            _context.Patients.Add(patient);
            await _context.SaveChangesAsync();

            return MapToPatientDto(patient);
        }

        public async Task<PatientDto> UpdatePatientAsync(int id, UpdatePatientDto updatePatientDto)
        {
            var patient = await _context.Patients
                .FirstOrDefaultAsync(p => p.Id == id);

            if (patient == null)
            {
                throw new AppException($"Patient with ID {id} not found");
            }

            patient.FirstName = updatePatientDto.FirstName;
            patient.LastName = updatePatientDto.LastName;
            patient.BirthDate = updatePatientDto.BirthDate;

            await _context.SaveChangesAsync();

            return MapToPatientDto(patient);
        }

        public async Task<bool> DeletePatientAsync(int id)
        {
            var patient = await _context.Patients
                .FirstOrDefaultAsync(p => p.Id == id);

            if (patient == null)
            {
                throw new AppException($"Patient with ID {id} not found");
            }

            // Check if patient has appointments
            var hasAppointments = await _context.Appointments
                .AnyAsync(a => a.PatientId == id);

            if (hasAppointments)
            {
                throw new AppException("Cannot delete patient with existing appointments");
            }

            _context.Patients.Remove(patient);
            await _context.SaveChangesAsync();

            return true;
        }

        // Helper method to map Patient entity to PatientDto
        private PatientDto MapToPatientDto(Patient patient)
        {
            return new PatientDto
            {
                Id = patient.Id,
                FirstName = patient.FirstName,
                LastName = patient.LastName,
                BirthDate = patient.BirthDate,
                UserId = patient.UserId
            };
        }
    }
} 