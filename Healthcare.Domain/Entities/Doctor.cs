namespace Healthcare.Domain.Entities;

public class Doctor
{
    public int Id { get; set; } // Primary Key
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Specialty { get; set; }

    // Foreign Key
    public int HospitalId { get; set; }
    public Hospital Hospital { get; set; }

    // Navigasyon (bir doktorun randevuları olabilir)
    public ICollection<Appointment> Appointments { get; set; }
}
