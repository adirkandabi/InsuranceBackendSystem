using System;
using System.Threading.Tasks;
using AiGent.Core.Entities;

namespace AiGent.Core.Interfaces;

public interface ICustomerRepository
{
    Task<Customer?> GetByIdAsync(Guid id);
    Task<Customer?> GetByEmailOrPhoneAsync(string email, string phone);
    Task AddAsync(Customer customer);
    Task UpdateAsync(Customer customer);
    Task DeleteAsync(Customer customer);
    Task<bool> ExistsAsync(Guid id);
}