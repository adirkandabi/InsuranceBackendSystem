using Microsoft.EntityFrameworkCore;
using AiGent.Core.Entities;
using AiGent.Core.Interfaces;
using AiGent.Infrastructure.Data;
using System;
using System.Threading.Tasks;

namespace AiGent.Infrastructure.Repositories;

public class CustomerRepository : ICustomerRepository
{
    private readonly InsuranceDbContext _context;

    public CustomerRepository(InsuranceDbContext context)
    {
        _context = context;
    }

    public async Task<Customer?> GetByIdAsync(Guid id)
    {
        // Eager load policies so the service can check for active ones before deletion
        return await _context.Customers
            .Include(c => c.Policies)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<Customer?> GetByEmailOrPhoneAsync(string email, string phone)
    {
        return await _context.Customers
            .FirstOrDefaultAsync(c => c.Email == email || c.PhoneNumber == phone);
    }

    public async Task AddAsync(Customer customer)
    {
        await _context.Customers.AddAsync(customer);
        await _context.SaveChangesAsync();
    }
    public async Task UpdateAsync(Customer customer)
    {
        _context.Customers.Update(customer);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Customer customer)
    {
        _context.Customers.Remove(customer);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        return await _context.Customers.AnyAsync(c => c.Id == id);
    }
}