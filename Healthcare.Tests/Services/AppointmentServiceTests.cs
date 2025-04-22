using Healthcare.Application.DTOs;
using Healthcare.Application.Exceptions;
using Healthcare.Application.Interfaces;
using Healthcare.Application.Services;
using Healthcare.Domain.Entities;
using Healthcare.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Healthcare.Tests.Services
{
    [TestFixture]
    public class AppointmentServiceTests
    {
        private AppDbContext _context;
        private Mock<IAuthService> _mockAuthService;
        private AppointmentService _appointmentService;
        private int _patientId;
        private int _doctorId;
        private int _userId;

        [SetUp]
        public void Setup()
        {
            // Create in-memory database for testing
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: $"HealthcareTestDb_{Guid.NewGuid()}")
                .Options;
            
            _context = new AppDbContext(options);
            _mockAuthService = new Mock<IAuthService>();
            
            // Create test data
            SetupTestData();
            
            // Create service to test
            _appointmentService = new AppointmentService(_context, _mockAuthService.Object);
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

            // Create a doctor
            var doctor = new Doctor
            {
                FirstName = "Test",
                LastName = "Doctor",
                Specialty = "General",
                HospitalId = hospital.Id
            };
            _context.Doctors.Add(doctor);
            _context.SaveChanges();
            _doctorId = doctor.Id;

            // Create a user
            var user = new User
            {
                Username = "testuser",
                Email = "test@example.com",
                PasswordHash = new byte[64],
                PasswordSalt = new byte[128]
            };
            _context.Users.Add(user);
            _context.SaveChanges();
            _userId = user.Id;

            // Create a patient
            var patient = new Patient
            {
                FirstName = "Test",
                LastName = "Patient",
                BirthDate = new DateTime(1990, 1, 1),
                UserId = user.Id
            };
            _context.Patients.Add(patient);
            _context.SaveChanges();
            _patientId = patient.Id;
        }

        [Test]
        public async Task GetUserAppointmentsAsync_ReturnsUserAppointments()
        {
            // Arrange
            var appointment = new Appointment
            {
                DoctorId = _doctorId,
                PatientId = _patientId,
                AppointmentDate = DateTime.Now.AddDays(7),
                Notes = "Test appointment"
            };
            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();

            // Act
            var result = await _appointmentService.GetUserAppointmentsAsync(_userId);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count());
            Assert.AreEqual(_patientId, result.First().PatientId);
            Assert.AreEqual(_doctorId, result.First().DoctorId);
        }

        [Test]
        public async Task CreateAppointmentAsync_CreatesAppointment()
        {
            // Arrange
            var createDto = new CreateAppointmentDto
            {
                DoctorId = _doctorId,
                AppointmentDate = DateTime.Now.AddDays(14),
                Notes = "New test appointment"
            };

            // Act
            var result = await _appointmentService.CreateAppointmentAsync(_userId, createDto);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(_patientId, result.PatientId);
            Assert.AreEqual(_doctorId, result.DoctorId);
            Assert.AreEqual(createDto.Notes, result.Notes);

            // Check if the appointment was actually saved to the database
            var dbAppointment = await _context.Appointments.FirstOrDefaultAsync(a => a.Notes == createDto.Notes);
            Assert.IsNotNull(dbAppointment);
        }

        [Test]
        public void CreateAppointmentAsync_ThrowsException_WhenDoctorNotFound()
        {
            // Arrange
            var createDto = new CreateAppointmentDto
            {
                DoctorId = 999, // Non-existent doctor ID
                AppointmentDate = DateTime.Now.AddDays(14),
                Notes = "Test appointment"
            };

            // Act & Assert
            var ex = Assert.ThrowsAsync<AppException>(async () => 
                await _appointmentService.CreateAppointmentAsync(_userId, createDto));
            
            Assert.That(ex.Message, Does.Contain("Doctor with ID 999 not found"));
        }

        [Test]
        public async Task CreateAppointmentAsync_ThrowsException_WhenPatientAlreadyHasAppointmentWithDoctor()
        {
            // Arrange
            // First add an existing appointment
            var existingAppointment = new Appointment
            {
                DoctorId = _doctorId,
                PatientId = _patientId,
                AppointmentDate = DateTime.Now.AddDays(3),
                Notes = "Existing appointment"
            };
            _context.Appointments.Add(existingAppointment);
            await _context.SaveChangesAsync();

            // Create DTO for new appointment with same doctor
            var createDto = new CreateAppointmentDto
            {
                DoctorId = _doctorId,
                AppointmentDate = DateTime.Now.AddDays(5), // Within a week of existing appointment
                Notes = "Test appointment"
            };

            // Act & Assert
            var ex = Assert.ThrowsAsync<AppointmentLimitExceededException>(async () =>
                await _appointmentService.CreateAppointmentAsync(_userId, createDto));
            
            Assert.IsNotNull(ex);
        }

        [Test]
        public async Task UpdateAppointmentAsync_UpdatesAppointment()
        {
            // Arrange
            var appointment = new Appointment
            {
                DoctorId = _doctorId,
                PatientId = _patientId,
                AppointmentDate = DateTime.Now.AddDays(7),
                Notes = "Test appointment"
            };
            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();

            var updateDto = new UpdateAppointmentDto
            {
                AppointmentDate = DateTime.Now.AddDays(10),
                Notes = "Updated test appointment"
            };

            // Setup mock auth service to simulate the user is allowed to update
            _mockAuthService.Setup(x => x.IsInRoleAsync(It.IsAny<int>(), It.IsAny<string>()))
                .ReturnsAsync(false); // User is not admin

            // Act
            var result = await _appointmentService.UpdateAppointmentAsync(_userId, appointment.Id, updateDto);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(updateDto.Notes, result.Notes);
            Assert.AreEqual(updateDto.AppointmentDate.Year, result.AppointmentYear);
            Assert.AreEqual(updateDto.AppointmentDate.Day, result.AppointmentDay);

            // Check if the appointment was actually updated in the database
            var dbAppointment = await _context.Appointments.FindAsync(appointment.Id);
            Assert.IsNotNull(dbAppointment);
            Assert.AreEqual(updateDto.Notes, dbAppointment.Notes);
        }

        [Test]
        public async Task DeleteAppointmentAsync_DeletesAppointment()
        {
            // Arrange
            var appointment = new Appointment
            {
                DoctorId = _doctorId,
                PatientId = _patientId,
                AppointmentDate = DateTime.Now.AddDays(7),
                Notes = "Test appointment"
            };
            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();

            // Setup mock auth service to simulate the user is allowed to delete
            _mockAuthService.Setup(x => x.IsInRoleAsync(It.IsAny<int>(), It.IsAny<string>()))
                .ReturnsAsync(false); // User is not admin

            // Act
            var result = await _appointmentService.DeleteAppointmentAsync(_userId, appointment.Id);

            // Assert
            Assert.IsTrue(result);

            // Check if the appointment was actually deleted from the database
            var dbAppointment = await _context.Appointments.FindAsync(appointment.Id);
            Assert.IsNull(dbAppointment);
        }
    }
} 