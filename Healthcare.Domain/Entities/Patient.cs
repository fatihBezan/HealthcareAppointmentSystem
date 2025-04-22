namespace Healthcare.Domain.Entities;

public class Patient
{
    public int Id { get; set; } // PK
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateTime BirthDate { get; set; }
    
    // Foreign Key to User
    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
}
