using Microsoft.AspNetCore.Mvc;
using AiGent.Core.Interfaces;
using AiGent.Core.Enums;
using AiGent.API.DTOs;
using System;
using System.Linq;
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

            // Map to response DTO to avoid object cycles during creation response if relations are populated
            var response = MapToResponseDto(policy);

            return CreatedAtAction(nameof(GetById), new { id = policy.Id }, response);
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

        var response = MapToResponseDto(policy);
        return Ok(response);
    }

    /// <summary>
    /// Retrieves all policies with optional dynamic filters for type and status.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] PolicyType? type, [FromQuery] PolicyStatus? status)
    {
        var policies = await _policyService.GetAllPoliciesAsync(type, status);

        // Transform the full database entities collection into safe flat DTOs
        var response = policies.Select(MapToResponseDto);
        return Ok(response);
    }

    /// <summary>
    /// Retrieves all policies associated with a specific customer.
    /// </summary>
    [HttpGet("customer/{customerId:guid}")]
    public async Task<IActionResult> GetByCustomer(Guid customerId)
    {
        var policies = await _policyService.GetCustomerPoliciesAsync(customerId);

        var response = policies.Select(p => new PolicyResponseDto
        {
            Id = p.Id,
            PolicyNumber = p.PolicyNumber,
            Type = p.Type,
            Status = p.Status,
            Premium = p.Premium,
            CoverageAmount = p.CoverageAmount,
            StartDate = p.StartDate,
            EndDate = p.EndDate,
            CancelledAt = p.CancelledAt,
            Customer = null // Setting to null since the caller filtering by customer already has the identity
        });

        return Ok(response);
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

            var response = MapToResponseDto(cancelledPolicy);
            return Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            // Policy is already cancelled
            return BadRequest(new { message = ex.Message });
        }
    }

    // DRY Helper method to centralize mapping and clean up controller clutter
    private static PolicyResponseDto MapToResponseDto(AiGent.Core.Entities.Policy policy)
    {
        return new PolicyResponseDto
        {
            Id = policy.Id,
            PolicyNumber = policy.PolicyNumber,
            Type = policy.Type,
            Status = policy.Status,
            Premium = policy.Premium,
            CoverageAmount = policy.CoverageAmount,
            StartDate = policy.StartDate,
            EndDate = policy.EndDate,
            CancelledAt = policy.CancelledAt,
            Customer = policy.Customer == null ? null : new CustomerMinDto
            {
                Id = policy.Customer.Id,
                FirstName = policy.Customer.FirstName,
                LastName = policy.Customer.LastName,
                Email = policy.Customer.Email,
                PhoneNumber = policy.Customer.PhoneNumber
            }
        };
    }
}