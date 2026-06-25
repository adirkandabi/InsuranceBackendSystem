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

        // 2. Create the domain entity
        var customer = new Customer
        {
            Id = Guid.NewGuid(),
            FirstName = firstName,
            LastName = lastName,
            Email = email,
            PhoneNumber = phoneNumber,
            CreatedAt = DateTime.UtcNow
        };

        // 3. Save to database via repository
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
}