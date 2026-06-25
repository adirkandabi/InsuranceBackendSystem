using System.Collections.Generic;

namespace AiGent.API.DTOs;

public class DashboardStatsDto
{
    public int TotalCustomers { get; set; }
    public int TotalActivePolicies { get; set; }
    public decimal TotalARR { get; set; } // Annual Recurring Revenue
    public Dictionary<string, int> PoliciesByType { get; set; } = new();
}