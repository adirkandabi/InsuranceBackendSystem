using System;
using AiGent.Core.Enums;

namespace AiGent.API.DTOs;

public class PolicyResponseDto
{
    public Guid Id { get; set; }
    public string PolicyNumber { get; set; } = string.Empty;
    public PolicyType Type { get; set; }
    public PolicyStatus Status { get; set; }
    public decimal Premium { get; set; }
    public decimal CoverageAmount { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public DateTime? CancelledAt { get; set; }

    // Using the slim DTO instead of the full database Entity to break the cycle!
    public CustomerMinDto? Customer { get; set; }
}