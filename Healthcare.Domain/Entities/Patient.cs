namespace Healthcare.Domain.Entities;

public class Patient
{
    public int Id { get; set; } // PK
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public DateTime BirthDate { get; set; }

    public ICollection<Appointment> Appointments { get; set; }
}
