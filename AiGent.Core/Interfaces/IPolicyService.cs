using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AiGent.Core.Entities;
using AiGent.Core.Enums;
using AiGent.Core.Models;

namespace AiGent.Core.Interfaces;

public interface IPolicyService
{
    Task<Policy> IssuePolicyAsync(Guid customerId, PolicyType type, decimal premium, decimal coverageAmount, DateTime startDate, DateTime endDate);
    Task<Policy?> GetPolicyByIdAsync(Guid id);
    Task<IEnumerable<Policy>> GetCustomerPoliciesAsync(Guid customerId);
    Task<IEnumerable<Policy>> GetAllPoliciesAsync(PolicyType? type, PolicyStatus? status);
    Task<Policy?> CancelPolicyAsync(Guid id);
    Task<DashboardStats> GetDashboardStatsAsync();
}