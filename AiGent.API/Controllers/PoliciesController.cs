using Microsoft.AspNetCore.Mvc;
using AiGent.Core.Interfaces;
using AiGent.Core.Enums;
using AiGent.API.DTOs;
using System;
using System.Threading.Tasks;

namespace AiGent.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PoliciesController : ControllerBase
{
    private readonly IPolicyService _policyService;

    public PoliciesController(IPolicyService policyService)
    {
        _policyService = policyService;
    }

    /// <summary>
    /// Issues a new insurance policy for an existing customer.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Issue([FromBody] IssuePolicyDto dto)
    {
        try
        {
            var policy = await _policyService.IssuePolicyAsync(
                dto.CustomerId, dto.Type, dto.Premium, dto.CoverageAmount, dto.StartDate, dto.EndDate);

            return CreatedAtAction(nameof(GetById), new { id = policy.Id }, policy);
        }
        catch (ArgumentException ex)
        {
            // Customer does not exist
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            // Business rules validation failed (e.g., date issues, premium <= 0)
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Retrieves a single policy by its unique ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var policy = await _policyService.GetPolicyByIdAsync(id);
        if (policy == null)
        {
            return NotFound(new { message = "Policy not found." });
        }
        return Ok(policy);
    }

    /// <summary>
    /// Retrieves all policies with optional dynamic filters for type and status.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] PolicyType? type, [FromQuery] PolicyStatus? status)
    {
        var policies = await _policyService.GetAllPoliciesAsync(type, status);
        return Ok(policies);
    }

    /// <summary>
    /// Retrieves all policies associated with a specific customer.
    /// </summary>
    [HttpGet("customer/{customerId:guid}")]
    public async Task<IActionResult> GetByCustomer(Guid customerId)
    {
        var policies = await _policyService.GetCustomerPoliciesAsync(customerId);
        return Ok(policies);
    }

    /// <summary>
    /// Performs a Soft Delete by terminating/cancelling an active policy.
    /// </summary>
    [HttpPatch("{id:guid}/cancel")]
    public async Task<IActionResult> Cancel(Guid id)
    {
        try
        {
            var cancelledPolicy = await _policyService.CancelPolicyAsync(id);
            if (cancelledPolicy == null)
            {
                return NotFound(new { message = "Policy not found." });
            }
            return Ok(cancelledPolicy);
        }
        catch (InvalidOperationException ex)
        {
            // Policy is already cancelled
            return BadRequest(new { message = ex.Message });
        }
    }
}