using Healthcare.Application.DTOs;
using Healthcare.Application.Exceptions;
using Healthcare.Application.Interfaces;
using Healthcare.Domain.Entities;
using Healthcare.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Healthcare.Application.Services
{
    public class DoctorService : IDoctorService
    {
        private readonly AppDbContext _context;
        private const int MaxSpecialistPerHospital = 10;

        public DoctorService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<DoctorDto>> GetAllDoctorsAsync()
        {
            var doctors = await _context.Doctors
                .Include(d => d.Hospital)
                .ToListAsync();

            return doctors.Select(d => MapToDoctorDto(d));
        }

        public async Task<DoctorDto> GetDoctorByIdAsync(int id)
        {
            var doctor = await _context.Doctors
                .Include(d => d.Hospital)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (doctor == null)
            {
                throw new AppException($"Doctor with ID {id} not found");
            }

            return MapToDoctorDto(doctor);
        }

        public async Task<IEnumerable<DoctorDto>> GetDoctorsByHospitalAsync(int hospitalId)
        {
            var doctors = await _context.Doctors
                .Include(d => d.Hospital)
                .Where(d => d.HospitalId == hospitalId)
                .ToListAsync();

            return doctors.Select(d => MapToDoctorDto(d));
        }

        public async Task<IEnumerable<DoctorDto>> GetDoctorsBySpecialtyAsync(string specialty)
        {
            var doctors = await _context.Doctors
                .Include(d => d.Hospital)
                .Where(d => d.Specialty.ToLower() == specialty.ToLower())
                .ToListAsync();

            return doctors.Select(d => MapToDoctorDto(d));
        }

        public async Task<DoctorDto> CreateDoctorAsync(CreateDoctorDto createDoctorDto)
        {
            // Check if hospital exists
            var hospital = await _context.Hospitals
                .FirstOrDefaultAsync(h => h.Id == createDoctorDto.HospitalId);

            if (hospital == null)
            {
                throw new AppException($"Hospital with ID {createDoctorDto.HospitalId} not found");
            }

            // Check if the hospital already has the maximum number of doctors for this specialty
            var specialtyCount = await _context.Doctors
                .Where(d => d.HospitalId == createDoctorDto.HospitalId && 
                       d.Specialty.ToLower() == createDoctorDto.Specialty.ToLower())
                .CountAsync();

            if (specialtyCount >= MaxSpecialistPerHospital)
            {
                throw new DoctorLimitExceededException(createDoctorDto.Specialty, hospital.Name);
            }

            // Create the doctor
            var doctor = new Doctor
            {
                FirstName = createDoctorDto.FirstName,
                LastName = createDoctorDto.LastName,
                Specialty = createDoctorDto.Specialty,
                HospitalId = createDoctorDto.HospitalId
            };

            _context.Doctors.Add(doctor);
            await _context.SaveChangesAsync();

            // Refresh the doctor with hospital
            var createdDoctor = await _context.Doctors
                .Include(d => d.Hospital)
                .FirstOrDefaultAsync(d => d.Id == doctor.Id);

            if (createdDoctor == null)
            {
                throw new AppException("Failed to create doctor");
            }

            return MapToDoctorDto(createdDoctor);
        }

        public async Task<DoctorDto> UpdateDoctorAsync(int id, UpdateDoctorDto updateDoctorDto)
        {
            var doctor = await _context.Doctors
                .Include(d => d.Hospital)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (doctor == null)
            {
                throw new AppException($"Doctor with ID {id} not found");
            }

            // Check if hospital exists if hospital is being changed
            if (doctor.HospitalId != updateDoctorDto.HospitalId)
            {
                var hospital = await _context.Hospitals
                    .FirstOrDefaultAsync(h => h.Id == updateDoctorDto.HospitalId);

                if (hospital == null)
                {
                    throw new AppException($"Hospital with ID {updateDoctorDto.HospitalId} not found");
                }
            }

            // Check if specialty is being changed and if so, check the limit in the target hospital
            if (doctor.Specialty.ToLower() != updateDoctorDto.Specialty.ToLower() || 
                doctor.HospitalId != updateDoctorDto.HospitalId)
            {
                // Check if the hospital already has the maximum number of doctors for this specialty
                var specialtyCount = await _context.Doctors
                    .Where(d => d.HospitalId == updateDoctorDto.HospitalId && 
                           d.Specialty.ToLower() == updateDoctorDto.Specialty.ToLower() && 
                           d.Id != id)
                    .CountAsync();

                if (specialtyCount >= MaxSpecialistPerHospital)
                {
                    var hospital = await _context.Hospitals
                        .FirstOrDefaultAsync(h => h.Id == updateDoctorDto.HospitalId);
                    
                    throw new DoctorLimitExceededException(updateDoctorDto.Specialty, hospital?.Name ?? "Unknown");
                }
            }

            // Update the doctor
            doctor.FirstName = updateDoctorDto.FirstName;
            doctor.LastName = updateDoctorDto.LastName;
            doctor.Specialty = updateDoctorDto.Specialty;
            doctor.HospitalId = updateDoctorDto.HospitalId;

            await _context.SaveChangesAsync();

            // Refresh the doctor with hospital
            var updatedDoctor = await _context.Doctors
                .Include(d => d.Hospital)
                .FirstOrDefaultAsync(d => d.Id == doctor.Id);

            if (updatedDoctor == null)
            {
                throw new AppException($"Doctor with ID {id} not found after update");
            }

            return MapToDoctorDto(updatedDoctor);
        }

        public async Task<bool> DeleteDoctorAsync(int id)
        {
            var doctor = await _context.Doctors
                .FirstOrDefaultAsync(d => d.Id == id);

            if (doctor == null)
            {
                throw new AppException($"Doctor with ID {id} not found");
            }

            // Check if doctor has appointments
            var hasAppointments = await _context.Appointments
                .AnyAsync(a => a.DoctorId == id);

            if (hasAppointments)
            {
                throw new AppException("Cannot delete doctor with existing appointments");
            }

            _context.Doctors.Remove(doctor);
            await _context.SaveChangesAsync();

            return true;
        }

        // Helper method to map Doctor entity to DoctorDto
        private DoctorDto MapToDoctorDto(Doctor doctor)
        {
            return new DoctorDto
            {
                Id = doctor.Id,
                FirstName = doctor.FirstName,
                LastName = doctor.LastName,
                Specialty = doctor.Specialty,
                HospitalId = doctor.HospitalId,
                HospitalName = doctor.Hospital?.Name ?? "Unknown"
            };
        }
    }
} 