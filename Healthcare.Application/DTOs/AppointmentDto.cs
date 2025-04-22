namespace Healthcare.Application.DTOs
{
    public class AppointmentDto
    {
        public int Id { get; set; }
        public int DoctorId { get; set; }
        public string DoctorFullName { get; set; } = string.Empty;
        public int PatientId { get; set; }
        public string PatientFullName { get; set; } = string.Empty;
        public DateTime AppointmentDate { get; set; }
        public int AppointmentDay { get; set; }
        public string AppointmentMonth { get; set; } = string.Empty;
        public int AppointmentYear { get; set; }
        public string Notes { get; set; } = string.Empty;
    }

    public class CreateAppointmentDto
    {
        public int DoctorId { get; set; }
        public DateTime AppointmentDate { get; set; }
        public string Notes { get; set; } = string.Empty;
    }

    public class UpdateAppointmentDto
    {
        public DateTime AppointmentDate { get; set; }
        public string Notes { get; set; } = string.Empty;
    }
} 