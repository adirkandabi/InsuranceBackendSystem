using Microsoft.AspNetCore.Mvc;
using AiGent.Core.Interfaces;
using System.Threading.Tasks;

namespace AiGent.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DashboardController : ControllerBase
{
    private readonly IPolicyService _policyService;

    public DashboardController(IPolicyService policyService)
    {
        _policyService = policyService;
    }

    /// <summary>
    /// Gets high-level business intelligence metrics for the insurance agent dashboard.
    /// </summary>
    [HttpGet("stats")]
    public async Task<IActionResult> GetStats()
    {
        var stats = await _policyService.GetDashboardStatsAsync();
        return Ok(stats);
    }
}