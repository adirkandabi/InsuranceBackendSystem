using System;
using System.Collections.Generic;

namespace AiGent.Core.Entities;

public class Customer
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation property for Entity Framework Core (One-to-Many)
    public ICollection<Policy> Policies { get; set; } = new List<Policy>();
}