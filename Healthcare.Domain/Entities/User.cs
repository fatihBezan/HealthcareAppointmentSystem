namespace Healthcare.Domain.Entities;

public class User
{
    public int Id { get; set; } // PK
    public string Username { get; set; }
    public string Email { get; set; }
    public byte[] PasswordHash { get; set; }
    public byte[] PasswordSalt { get; set; }

    public ICollection<UserRole> UserRoles { get; set; }
}
