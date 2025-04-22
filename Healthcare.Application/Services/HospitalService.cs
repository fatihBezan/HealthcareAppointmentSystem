using Healthcare.Application.DTOs;
using Healthcare.Application.Exceptions;
using Healthcare.Application.Interfaces;
using Healthcare.Domain.Entities;
using Healthcare.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Healthcare.Application.Services
{
    public class HospitalService : IHospitalService
    {
        private readonly AppDbContext _context;

        public HospitalService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<HospitalDto>> GetAllHospitalsAsync()
        {
            var hospitals = await _context.Hospitals.ToListAsync();
            return hospitals.Select(h => MapToHospitalDto(h));
        }

        public async Task<HospitalDto> GetHospitalByIdAsync(int id)
        {
            var hospital = await _context.Hospitals
                .FirstOrDefaultAsync(h => h.Id == id);

            if (hospital == null)
            {
                throw new AppException($"Hospital with ID {id} not found");
            }

            return MapToHospitalDto(hospital);
        }

        public async Task<IEnumerable<HospitalDto>> GetHospitalsByCityAsync(string city)
        {
            var hospitals = await _context.Hospitals
                .Where(h => h.City.ToLower() == city.ToLower())
                .ToListAsync();

            return hospitals.Select(h => MapToHospitalDto(h));
        }

        public async Task<HospitalDto> CreateHospitalAsync(CreateHospitalDto createHospitalDto)
        {
            var hospital = new Hospital
            {
                Name = createHospitalDto.Name,
                Address = createHospitalDto.Address,
                City = createHospitalDto.City
            };

            _context.Hospitals.Add(hospital);
            await _context.SaveChangesAsync();

            return MapToHospitalDto(hospital);
        }

        public async Task<HospitalDto> UpdateHospitalAsync(int id, UpdateHospitalDto updateHospitalDto)
        {
            var hospital = await _context.Hospitals
                .FirstOrDefaultAsync(h => h.Id == id);

            if (hospital == null)
            {
                throw new AppException($"Hospital with ID {id} not found");
            }

            hospital.Name = updateHospitalDto.Name;
            hospital.Address = updateHospitalDto.Address;
            hospital.City = updateHospitalDto.City;

            await _context.SaveChangesAsync();

            return MapToHospitalDto(hospital);
        }

        public async Task<bool> DeleteHospitalAsync(int id)
        {
            var hospital = await _context.Hospitals
                .FirstOrDefaultAsync(h => h.Id == id);

            if (hospital == null)
            {
                throw new AppException($"Hospital with ID {id} not found");
            }

            // Check if hospital has doctors
            var hasDoctors = await _context.Doctors
                .AnyAsync(d => d.HospitalId == id);

            if (hasDoctors)
            {
                throw new AppException("Cannot delete hospital with assigned doctors");
            }

            _context.Hospitals.Remove(hospital);
            await _context.SaveChangesAsync();

            return true;
        }

        // Helper method to map Hospital entity to HospitalDto
        private HospitalDto MapToHospitalDto(Hospital hospital)
        {
            return new HospitalDto
            {
                Id = hospital.Id,
                Name = hospital.Name,
                Address = hospital.Address,
                City = hospital.City
            };
        }
    }
} 