using Microsoft.EntityFrameworkCore;
using AiGent.Core.Entities;
using AiGent.Core.Interfaces;
using AiGent.Core.Enums;
using AiGent.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AiGent.Infrastructure.Repositories;

public class PolicyRepository : IPolicyRepository
{
    private readonly InsuranceDbContext _context;

    public PolicyRepository(InsuranceDbContext context)
    {
        _context = context;
    }

    public async Task<Policy?> GetByIdAsync(Guid id)
    {
        // Fetch a single policy including its customer data
        return await _context.Policies
            .Include(p => p.Customer)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<IEnumerable<Policy>> GetByCustomerIdAsync(Guid customerId)
    {
        // Get all policies belonging to a specific customer
        return await _context.Policies
            .Where(p => p.CustomerId == customerId)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<IEnumerable<Policy>> GetAllAsync(PolicyType? type, PolicyStatus? status)
    {
        // Start with a base queryable for filtering
        var query = _context.Policies.AsQueryable();

        // Dynamically add filters based on agent selection
        if (type.HasValue)
        {
            query = query.Where(p => p.Type == type.Value);
        }

        if (status.HasValue)
        {
            query = query.Where(p => p.Status == status.Value);
        }

        return await query
            .Include(p => p.Customer) // Include customer details for the visibility view
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task AddAsync(Policy policy)
    {
        await _context.Policies.AddAsync(policy);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Policy policy)
    {
        _context.Policies.Update(policy);
        await _context.SaveChangesAsync();
    }
}