using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AiGent.Core.Entities;
using AiGent.Core.Enums;
using AiGent.Core.Interfaces;
using AiGent.Core.Models;

namespace AiGent.Core.Services;

public class PolicyService : IPolicyService
{
    private readonly IPolicyRepository _policyRepository;
    private readonly ICustomerRepository _customerRepository;

    public PolicyService(IPolicyRepository policyRepository, ICustomerRepository customerRepository)
    {
        _policyRepository = policyRepository;
        _customerRepository = customerRepository;
    }

    public async Task<Policy> IssuePolicyAsync(Guid customerId, PolicyType type, decimal premium, decimal coverageAmount, DateTime startDate, DateTime endDate)
    {
        // Business Rule: Ensure the customer actually exists
        var customerExists = await _customerRepository.ExistsAsync(customerId);
        if (!customerExists)
        {
            throw new ArgumentException("Cannot issue policy. Target customer does not exist.");
        }

        // Business Rule: Validate dates strictly for financial compliance
        if (startDate.Date < DateTime.UtcNow.Date.AddDays(-1)) // Allowing a small 1-day buffer for timezone offsets
        {
            throw new InvalidOperationException("Policy start date cannot be in the past.");
        }

        if (endDate <= startDate.AddMonths(6))
        {
            throw new InvalidOperationException("Policy duration must be at least 6 months.");
        }

        if (premium <= 0 || coverageAmount <= 0)
        {
            throw new InvalidOperationException("Premium and Coverage Amount must be greater than zero.");
        }

        // Generate a clean, unique policy number
        string policyNumber = GenerateSmartPolicyNumber(type);

        var policy = new Policy
        {
            Id = Guid.NewGuid(),
            CustomerId = customerId,
            PolicyNumber = policyNumber,
            Type = type,
            Status = PolicyStatus.Active,
            Premium = premium,
            CoverageAmount = coverageAmount,
            StartDate = startDate.ToUniversalTime(),
            EndDate = endDate.ToUniversalTime()
        };

        await _policyRepository.AddAsync(policy);
        return policy;
    }

    public async Task<Policy?> GetPolicyByIdAsync(Guid id)
    {
        return await _policyRepository.GetByIdAsync(id);
    }

    public async Task<IEnumerable<Policy>> GetCustomerPoliciesAsync(Guid customerId)
    {
        return await _policyRepository.GetByCustomerIdAsync(customerId);
    }

    public async Task<IEnumerable<Policy>> GetAllPoliciesAsync(PolicyType? type, PolicyStatus? status)
    {
        return await _policyRepository.GetAllAsync(type, status);
    }

    public async Task<Policy?> CancelPolicyAsync(Guid id)
    {
        var policy = await _policyRepository.GetByIdAsync(id);
        if (policy == null) return null;

        if (policy.Status == PolicyStatus.Cancelled)
        {
            throw new InvalidOperationException("This policy has already been cancelled.");
        }

        // Business Rule: Implement Soft Delete by terminating the cycle and logging the timestamp
        policy.Status = PolicyStatus.Cancelled;
        policy.CancelledAt = DateTime.UtcNow;

        await _policyRepository.UpdateAsync(policy);
        return policy;
    }
    public async Task<DashboardStats> GetDashboardStatsAsync()
    {
        // Fetch all active policies with no tracking for maximum performance
        var allPolicies = await _policyRepository.GetAllAsync(null, null);
        var activePolicies = allPolicies.Where(p => p.Status == PolicyStatus.Active).ToList();

        // Group policies by their type and count them
        var policiesByType = activePolicies
            .GroupBy(p => p.Type.ToString())
            .ToDictionary(g => g.Key, g => g.Count());

        // Calculate Total ARR (Sum of premiums of all active policies)
        decimal totalARR = activePolicies.Sum(p => p.Premium);

        // Count unique customers who have at least one active policy
        int totalActiveCustomers = activePolicies.Select(p => p.CustomerId).Distinct().Count();

        return new DashboardStats
        {
            TotalCustomers = totalActiveCustomers,
            TotalActivePolicies = activePolicies.Count,
            TotalARR = totalARR,
            PoliciesByType = policiesByType
        };
    }
    // Helper method to generate the smart readable business identifier
    private static string GenerateSmartPolicyNumber(PolicyType type)
    {
        string typeCode = type.ToString().ToUpper();
        string currentYear = DateTime.UtcNow.Year.ToString();

        // Generate a 4-character random alphanumeric suffix for uniqueness
        string randomSuffix = Guid.NewGuid().ToString("N").Substring(0, 4).ToUpper();

        // Result format example: POL-CAR-2026-F8A2
        return $"POL-{typeCode}-{currentYear}-{randomSuffix}";
    }
}