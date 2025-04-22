namespace Healthcare.Domain.Entities;

public class Hospital
{
    public int Id { get; set; } // PK
    public string Name { get; set; }
    public string Address { get; set; }
    public string City { get; set; }

    // Navigasyon
    public ICollection<Doctor> Doctors { get; set; }
}
