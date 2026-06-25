using Microsoft.AspNetCore.Mvc;
using AiGent.Core.Interfaces;
using AiGent.API.DTOs;
using System;
using System.Threading.Tasks;

namespace AiGent.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CustomersController : ControllerBase
{
    private readonly ICustomerService _customerService;

    // Injecting the service via constructor
    public CustomersController(ICustomerService customerService)
    {
        _customerService = customerService;
    }

    /// <summary>
    /// Creates a new customer in the system (Onboarding).
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Onboard([FromBody] CreateCustomerDto dto)
    {
        try
        {
            var customer = await _customerService.OnboardCustomerAsync(
                dto.FirstName, dto.LastName, dto.Email, dto.PhoneNumber);

            // Returns 201 Created with the location of the new resource
            return CreatedAtAction(nameof(GetProfile), new { id = customer.Id }, customer);
        }
        catch (InvalidOperationException ex)
        {
            // Caught business rule violation (e.g., duplicate email/phone)
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Retrieves a customer profile along with their policies.
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetProfile(Guid id)
    {
        var customer = await _customerService.GetCustomerProfileAsync(id);
        if (customer == null)
        {
            return NotFound(new { message = "Customer not found." });
        }
        return Ok(customer);
    }

    /// <summary>
    /// Updates an existing customer's information.
    /// </summary>
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCustomerDto dto)
    {
        try
        {
            var updatedCustomer = await _customerService.UpdateCustomerAsync(
                id, dto.FirstName, dto.LastName, dto.Email, dto.PhoneNumber);

            if (updatedCustomer == null)
            {
                return NotFound(new { message = "Customer not found." });
            }

            return Ok(updatedCustomer);
        }
        catch (InvalidOperationException ex)
        {
            // Caught business rule violation (e.g., updating to an email already owned by another customer)
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Deletes a customer if they have no active insurance policies.
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            var deleted = await _customerService.DeleteCustomerAsync(id);
            if (!deleted)
            {
                return NotFound(new { message = "Customer not found." });
            }

            // Returns 204 No Content upon successful deletion
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            // Caught business rule violation (e.g., customer has active policies)
            return BadRequest(new { message = ex.Message });
        }
    }
}