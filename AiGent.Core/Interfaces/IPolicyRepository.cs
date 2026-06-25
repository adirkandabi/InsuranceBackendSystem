using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AiGent.Core.Entities;
using AiGent.Core.Enums;

namespace AiGent.Core.Interfaces;

public interface IPolicyRepository
{
    Task<Policy?> GetByIdAsync(Guid id);
    Task<IEnumerable<Policy>> GetByCustomerIdAsync(Guid customerId);
    Task<IEnumerable<Policy>> GetAllAsync(PolicyType? type, PolicyStatus? status);
    Task AddAsync(Policy policy);
    Task UpdateAsync(Policy policy);
}