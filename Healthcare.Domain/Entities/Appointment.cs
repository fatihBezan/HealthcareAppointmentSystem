namespace Healthcare.Domain.Entities;

public class Appointment
{
    public int Id { get; set; } // PK
    public int DoctorId { get; set; }
    public int PatientId { get; set; }
    public DateTime AppointmentDate { get; set; }
    public string Notes { get; set; }

    public Doctor Doctor { get; set; }
    public Patient Patient { get; set; }
}
