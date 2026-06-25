using AiGent.Core.Entities;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;

namespace AiGent.Infrastructure.Data;

public class InsuranceDbContext : DbContext
{
    public InsuranceDbContext(DbContextOptions<InsuranceDbContext> options) : base(options)
    {
    }

    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Policy> Policies => Set<Policy>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Customer Configuration
        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasKey(c => c.Id);
            entity.Property(c => c.FirstName).IsRequired().HasMaxLength(50);
            entity.Property(c => c.LastName).IsRequired().HasMaxLength(50);
            entity.Property(c => c.Email).IsRequired().HasMaxLength(100);
            entity.Property(c => c.PhoneNumber).IsRequired().HasMaxLength(20);

            // Ensure unique email and phone number to prevent duplicates
            entity.HasIndex(c => c.Email).IsUnique();
            entity.HasIndex(c => c.PhoneNumber).IsUnique();
        });

        // Policy Configuration
        modelBuilder.Entity<Policy>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.Property(p => p.PolicyNumber).IsRequired().HasMaxLength(30);
            entity.Property(p => p.Premium).HasColumnType("decimal(18,2)");
            entity.Property(p => p.CoverageAmount).HasColumnType("decimal(18,2)");

            // Define the One-to-Many relationship
            entity.HasOne(p => p.Customer)
                  .WithMany(c => c.Policies)
                  .HasForeignKey(p => p.CustomerId)
                  .OnDelete(DeleteBehavior.Restrict); // Prevent accidental customer deletion cascade
        });
    }
}