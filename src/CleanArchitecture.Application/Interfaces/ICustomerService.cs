using CleanArchitecture.Application.Common;
using CleanArchitecture.Application.DTOs.Customer;

namespace CleanArchitecture.Application.Interfaces;

public interface ICustomerService
{
    Task<CustomerDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<CustomerDto?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<IEnumerable<CustomerDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<PagedResult<CustomerDto>> GetPagedAsync(CustomerFilterDto filter, CancellationToken cancellationToken = default);
    Task<IEnumerable<CustomerDto>> GetByCountryAsync(string country, CancellationToken cancellationToken = default);
    Task<CustomerDto> CreateAsync(CreateCustomerDto dto, CancellationToken cancellationToken = default);
    Task<CustomerDto> UpdateAsync(int id, UpdateCustomerDto dto, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}
