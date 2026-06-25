using System;
using System.Linq;
using System.Threading.Tasks;
using AiGent.Core.Entities;
using AiGent.Core.Interfaces;
using AiGent.Core.Enums;

namespace AiGent.Core.Services;

public class CustomerService : ICustomerService
{
    private readonly ICustomerRepository _customerRepository;

    // Injecting the repository interface via Constructor Injection
    public CustomerService(ICustomerRepository customerRepository)
    {
        _customerRepository = customerRepository;
    }

    public async Task<Customer> OnboardCustomerAsync(string firstName, string lastName, string email, string phoneNumber)
    {
        // Avoid duplicate customers by checking email or phone number
        var existingCustomer = await _customerRepository.GetByEmailOrPhoneAsync(email, phoneNumber);
        if (existingCustomer != null)
        {
            throw new InvalidOperationException("A customer with this email or phone number already exists.");
        }

        // Create the domain entity
        var customer = new Customer
        {
            Id = Guid.NewGuid(),
            FirstName = firstName,
            LastName = lastName,
            Email = email,
            PhoneNumber = phoneNumber,
            CreatedAt = DateTime.UtcNow
        };

        // Save to database via repository
        await _customerRepository.AddAsync(customer);
        return customer;
    }

    public async Task<Customer?> GetCustomerProfileAsync(Guid id)
    {
        return await _customerRepository.GetByIdAsync(id);
    }

    public async Task<bool> DeleteCustomerAsync(Guid id)
    {
        // 1. Check if the customer exists
        var customer = await _customerRepository.GetByIdAsync(id);
        if (customer == null)
        {
            return false;
        }

        // 2. Business Rule: Prevent deletion if the customer has any ACTIVE insurance policies
        var hasActivePolicies = customer.Policies.Any(p => p.Status == PolicyStatus.Active);
        if (hasActivePolicies)
        {
            throw new InvalidOperationException("Cannot delete a customer with active insurance policies.");
        }

        // 3. Perform hard delete safely since no active policies restrict it
        await _customerRepository.DeleteAsync(customer);
        return true;
    }
    public async Task<Customer?> UpdateCustomerAsync(Guid id, string firstName, string lastName, string email, string phoneNumber)
    {
        // Check if the customer actually exists
        var customer = await _customerRepository.GetByIdAsync(id);
        if (customer == null) return null;

        // If email or phone changes, verify they aren't stolen by ANOTHER customer
        if (customer.Email != email || customer.PhoneNumber != phoneNumber)
        {
            var existingWithSameDetails = await _customerRepository.GetByEmailOrPhoneAsync(email, phoneNumber);

            // If someone else has this email/phone (meaning their ID is different) -> Block it!
            if (existingWithSameDetails != null && existingWithSameDetails.Id != id)
            {
                throw new InvalidOperationException("Another customer with this email or phone number already exists.");
            }
        }

        // Update fields
        customer.FirstName = firstName;
        customer.LastName = lastName;
        customer.Email = email;
        customer.PhoneNumber = phoneNumber;

        // Save to DB
        await _customerRepository.UpdateAsync(customer);
        return customer;
    }
}