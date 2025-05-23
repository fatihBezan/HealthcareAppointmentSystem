﻿namespace Healthcare.Domain.Entities;

public class UserRole
{
    public int Id { get; set; } // PK
    public int UserId { get; set; }
    public int RoleId { get; set; }

    public User User { get; set; }
    public Role Role { get; set; }
}