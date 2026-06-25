using AiGent.Core.Entities;

namespace AiGent.Core.Interfaces;

public interface ICustomerService
{
    Task<Customer> OnboardCustomerAsync(string firstName, string lastName, string email, string phoneNumber);
    Task<Customer?> GetCustomerProfileAsync(Guid id);
    Task<Customer?> UpdateCustomerAsync(Guid id, string firstName, string lastName, string email, string phoneNumber);
    Task<bool> DeleteCustomerAsync(Guid id);
}