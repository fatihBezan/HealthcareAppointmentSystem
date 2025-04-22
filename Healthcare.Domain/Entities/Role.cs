namespace Healthcare.Domain.Entities;

public class Role
{
    public int Id { get; set; } // PK
    public string Name { get; set; }
    public string Description { get; set; }

    public ICollection<UserRole> UserRoles { get; set; }
}
