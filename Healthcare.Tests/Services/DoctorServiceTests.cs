using Healthcare.Application.DTOs;
using Healthcare.Application.Exceptions;
using Healthcare.Application.Services;
using Healthcare.Domain.Entities;
using Healthcare.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Healthcare.Tests.Services
{
    [TestFixture]
    public class DoctorServiceTests
    {
        private AppDbContext _context;
        private DoctorService _doctorService;
        private int _hospitalId;

        [SetUp]
        public void Setup()
        {
            // Create in-memory database for testing
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: $"HealthcareTestDb_{Guid.NewGuid()}")
                .Options;
            
            _context = new AppDbContext(options);
            
            // Create test data
            SetupTestData();
            
            // Create service to test
            _doctorService = new DoctorService(_context);
        }

        [TearDown]
        public void TearDown()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        private void SetupTestData()
        {
            // Create a hospital
            var hospital = new Hospital
            {
                Name = "Test Hospital",
                Address = "123 Test St",
                City = "Test City"
            };
            _context.Hospitals.Add(hospital);
            _context.SaveChanges();
            _hospitalId = hospital.Id;
        }

        [Test]
        public async Task GetAllDoctorsAsync_ReturnsDoctors()
        {
            // Arrange
            var doctor = new Doctor
            {
                FirstName = "Test",
                LastName = "Doctor",
                Specialty = "General",
                HospitalId = _hospitalId
            };
            _context.Doctors.Add(doctor);
            await _context.SaveChangesAsync();

            // Act
            var result = await _doctorService.GetAllDoctorsAsync();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count());
            Assert.AreEqual("Test", result.First().FirstName);
            Assert.AreEqual("Doctor", result.First().LastName);
        }

        [Test]
        public async Task CreateDoctorAsync_CreatesDoctor()
        {
            // Arrange
            var createDto = new CreateDoctorDto
            {
                FirstName = "New",
                LastName = "Doctor",
                Specialty = "Cardiology",
                HospitalId = _hospitalId
            };

            // Act
            var result = await _doctorService.CreateDoctorAsync(createDto);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(createDto.FirstName, result.FirstName);
            Assert.AreEqual(createDto.LastName, result.LastName);
            Assert.AreEqual(createDto.Specialty, result.Specialty);
            Assert.AreEqual(createDto.HospitalId, result.HospitalId);

            // Check if the doctor was actually saved to the database
            var dbDoctor = await _context.Doctors.FirstOrDefaultAsync(d => d.FirstName == createDto.FirstName && d.LastName == createDto.LastName);
            Assert.IsNotNull(dbDoctor);
        }

        [Test]
        public void CreateDoctorAsync_ThrowsException_WhenHospitalNotFound()
        {
            // Arrange
            var createDto = new CreateDoctorDto
            {
                FirstName = "New",
                LastName = "Doctor",
                Specialty = "Cardiology",
                HospitalId = 999 // Non-existent hospital ID
            };

            // Act & Assert
            var ex = Assert.ThrowsAsync<AppException>(async () => 
                await _doctorService.CreateDoctorAsync(createDto));
            
            Assert.That(ex.Message, Does.Contain("Hospital with ID 999 not found"));
        }

        [Test]
        public async Task CreateDoctorAsync_ThrowsException_WhenSpecialistLimitReached()
        {
            // Arrange
            const string specialty = "Cardiology";
            
            // Add 10 doctors with the same specialty
            for (int i = 0; i < 10; i++)
            {
                _context.Doctors.Add(new Doctor
                {
                    FirstName = $"Test{i}",
                    LastName = "Doctor",
                    Specialty = specialty,
                    HospitalId = _hospitalId
                });
            }
            await _context.SaveChangesAsync();

            // Try to add one more doctor with the same specialty
            var createDto = new CreateDoctorDto
            {
                FirstName = "One More",
                LastName = "Doctor",
                Specialty = specialty,
                HospitalId = _hospitalId
            };

            // Act & Assert
            var ex = Assert.ThrowsAsync<DoctorLimitExceededException>(async () => 
                await _doctorService.CreateDoctorAsync(createDto));
            
            Assert.IsNotNull(ex);
            Assert.That(ex.Message, Does.Contain("has reached the maximum number of doctors"));
        }

        [Test]
        public async Task UpdateDoctorAsync_UpdatesDoctor()
        {
            // Arrange
            var doctor = new Doctor
            {
                FirstName = "Test",
                LastName = "Doctor",
                Specialty = "General",
                HospitalId = _hospitalId
            };
            _context.Doctors.Add(doctor);
            await _context.SaveChangesAsync();

            var updateDto = new UpdateDoctorDto
            {
                FirstName = "Updated",
                LastName = "Doctor",
                Specialty = "Neurology",
                HospitalId = _hospitalId
            };

            // Act
            var result = await _doctorService.UpdateDoctorAsync(doctor.Id, updateDto);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(updateDto.FirstName, result.FirstName);
            Assert.AreEqual(updateDto.Specialty, result.Specialty);

            // Check if the doctor was actually updated in the database
            var dbDoctor = await _context.Doctors.FindAsync(doctor.Id);
            Assert.IsNotNull(dbDoctor);
            Assert.AreEqual(updateDto.FirstName, dbDoctor.FirstName);
            Assert.AreEqual(updateDto.Specialty, dbDoctor.Specialty);
        }

        [Test]
        public async Task UpdateDoctorAsync_ThrowsException_WhenSpecialistLimitReached()
        {
            // Arrange
            const string specialty1 = "General";
            const string specialty2 = "Cardiology";
            
            // Create a doctor with specialty1
            var doctor = new Doctor
            {
                FirstName = "Test",
                LastName = "Doctor",
                Specialty = specialty1,
                HospitalId = _hospitalId
            };
            _context.Doctors.Add(doctor);
            
            // Add 10 doctors with specialty2
            for (int i = 0; i < 10; i++)
            {
                _context.Doctors.Add(new Doctor
                {
                    FirstName = $"Test{i}",
                    LastName = "Doctor",
                    Specialty = specialty2,
                    HospitalId = _hospitalId
                });
            }
            await _context.SaveChangesAsync();

            // Try to update doctor to specialty2
            var updateDto = new UpdateDoctorDto
            {
                FirstName = "Updated",
                LastName = "Doctor",
                Specialty = specialty2, // Specialty with limit already reached
                HospitalId = _hospitalId
            };

            // Act & Assert
            var ex = Assert.ThrowsAsync<DoctorLimitExceededException>(async () => 
                await _doctorService.UpdateDoctorAsync(doctor.Id, updateDto));
            
            Assert.IsNotNull(ex);
            Assert.That(ex.Message, Does.Contain("has reached the maximum number of doctors"));
        }

        [Test]
        public async Task DeleteDoctorAsync_DeletesDoctor()
        {
            // Arrange
            var doctor = new Doctor
            {
                FirstName = "Test",
                LastName = "Doctor",
                Specialty = "General",
                HospitalId = _hospitalId
            };
            _context.Doctors.Add(doctor);
            await _context.SaveChangesAsync();

            // Act
            var result = await _doctorService.DeleteDoctorAsync(doctor.Id);

            // Assert
            Assert.IsTrue(result);

            // Check if the doctor was actually deleted from the database
            var dbDoctor = await _context.Doctors.FindAsync(doctor.Id);
            Assert.IsNull(dbDoctor);
        }

        [Test]
        public async Task DeleteDoctorAsync_ThrowsException_WhenDoctorHasAppointments()
        {
            // Arrange
            // Create a patient
            var patient = new Patient
            {
                FirstName = "Test",
                LastName = "Patient",
                BirthDate = new DateTime(1990, 1, 1),
                UserId = 1 // Dummy user ID for test
            };
            _context.Patients.Add(patient);
            await _context.SaveChangesAsync();

            // Create a doctor
            var doctor = new Doctor
            {
                FirstName = "Test",
                LastName = "Doctor",
                Specialty = "General",
                HospitalId = _hospitalId
            };
            _context.Doctors.Add(doctor);
            await _context.SaveChangesAsync();

            // Create an appointment
            var appointment = new Appointment
            {
                DoctorId = doctor.Id,
                PatientId = patient.Id,
                AppointmentDate = DateTime.Now.AddDays(7),
                Notes = "Test appointment"
            };
            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();

            // Act & Assert
            var ex = Assert.ThrowsAsync<AppException>(async () => 
                await _doctorService.DeleteDoctorAsync(doctor.Id));
            
            Assert.That(ex.Message, Does.Contain("Cannot delete doctor with existing appointments"));
        }
    }
} 