using System;
using AiGent.Core.Enums;

namespace AiGent.Core.Entities;

public class Policy
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }

    // Business identifier (e.g., POL-CAR-2026-0001)
    public string PolicyNumber { get; set; } = string.Empty;
    public PolicyType Type { get; set; }
    public PolicyStatus Status { get; set; } = PolicyStatus.Active;

    // Financial fields using decimal for high precision
    public decimal Premium { get; set; }
    public decimal CoverageAmount { get; set; }

    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public DateTime? CancelledAt { get; set; }

    // Navigation property back to the customer
    public Customer? Customer { get; set; }
}