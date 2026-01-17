using AutoMapper;
using CleanArchitecture.Application.Common;
using CleanArchitecture.Application.DTOs.Customer;
using CleanArchitecture.Application.Interfaces;
using CleanArchitecture.Domain.Entities;
using CleanArchitecture.Domain.Exceptions;
using CleanArchitecture.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace CleanArchitecture.Application.Services;

public class CustomerService : ICustomerService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<CustomerService> _logger;

    public CustomerService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<CustomerService> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<CustomerDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting customer with ID: {CustomerId}", id);

        var customer = await _unitOfWork.Customers.GetByIdAsync(id, cancellationToken);

        if (customer == null)
        {
            _logger.LogWarning("Customer with ID {CustomerId} not found", id);
            throw new NotFoundException(nameof(Customer), id);
        }

        return _mapper.Map<CustomerDto>(customer);
    }

    public async Task<CustomerDto?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting customer by email: {Email}", email);

        var customer = await _unitOfWork.Customers.GetByEmailAsync(email, cancellationToken);

        if (customer == null)
        {
            _logger.LogWarning("Customer with email {Email} not found", email);
            throw new NotFoundException(nameof(Customer), email);
        }

        return _mapper.Map<CustomerDto>(customer);
    }

    public async Task<IEnumerable<CustomerDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting all customers");

        var customers = await _unitOfWork.Customers.GetAllAsync(cancellationToken);
        return _mapper.Map<IEnumerable<CustomerDto>>(customers);
    }

    public async Task<PagedResult<CustomerDto>> GetPagedAsync(CustomerFilterDto filter, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting paged customers. Page: {PageNumber}, Size: {PageSize}",
            filter.PageNumber, filter.PageSize);

        System.Linq.Expressions.Expression<Func<Customer, bool>>? filterExpression = null;

        if (!string.IsNullOrWhiteSpace(filter.SearchTerm) ||
            !string.IsNullOrWhiteSpace(filter.Country) ||
            !string.IsNullOrWhiteSpace(filter.City) ||
            filter.IsActive.HasValue)
        {
            filterExpression = c =>
                (string.IsNullOrWhiteSpace(filter.SearchTerm) ||
                    c.FirstName.Contains(filter.SearchTerm) ||
                    c.LastName.Contains(filter.SearchTerm) ||
                    c.Email.Contains(filter.SearchTerm)) &&
                (string.IsNullOrWhiteSpace(filter.Country) || c.Country == filter.Country) &&
                (string.IsNullOrWhiteSpace(filter.City) || c.City == filter.City) &&
                (!filter.IsActive.HasValue || c.IsActive == filter.IsActive.Value);
        }

        System.Linq.Expressions.Expression<Func<Customer, object>>? orderBy = filter.SortBy?.ToLower() switch
        {
            "firstname" => c => c.FirstName,
            "lastname" => c => c.LastName,
            "email" => c => c.Email,
            "country" => c => c.Country,
            "createdat" => c => c.CreatedAt,
            _ => c => c.Id
        };

        var (items, totalCount) = await _unitOfWork.Customers.GetPagedAsync(
            filter.PageNumber,
            filter.PageSize,
            filterExpression,
            orderBy,
            !filter.SortDescending,
            cancellationToken);

        var dtos = _mapper.Map<IEnumerable<CustomerDto>>(items);

        return new PagedResult<CustomerDto>(dtos, totalCount, filter.PageNumber, filter.PageSize);
    }

    public async Task<IEnumerable<CustomerDto>> GetByCountryAsync(string country, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting customers by country: {Country}", country);

        var customers = await _unitOfWork.Customers.GetByCountryAsync(country, cancellationToken);
        return _mapper.Map<IEnumerable<CustomerDto>>(customers);
    }

    public async Task<CustomerDto> CreateAsync(CreateCustomerDto dto, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating new customer: {Email}", dto.Email);

        var emailExists = await _unitOfWork.Customers.EmailExistsAsync(dto.Email, null, cancellationToken);
        if (emailExists)
        {
            _logger.LogWarning("Customer with email {Email} already exists", dto.Email);
            throw new ConflictException(nameof(Customer), "Email", dto.Email);
        }

        var customer = _mapper.Map<Customer>(dto);

        var createdCustomer = await _unitOfWork.Customers.AddAsync(customer, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Customer created with ID: {CustomerId}", createdCustomer.Id);

        return _mapper.Map<CustomerDto>(createdCustomer);
    }

    public async Task<CustomerDto> UpdateAsync(int id, UpdateCustomerDto dto, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Updating customer with ID: {CustomerId}", id);

        var customer = await _unitOfWork.Customers.GetByIdAsync(id, cancellationToken);

        if (customer == null)
        {
            _logger.LogWarning("Customer with ID {CustomerId} not found for update", id);
            throw new NotFoundException(nameof(Customer), id);
        }

        var emailExists = await _unitOfWork.Customers.EmailExistsAsync(dto.Email, id, cancellationToken);
        if (emailExists)
        {
            _logger.LogWarning("Another customer with email {Email} already exists", dto.Email);
            throw new ConflictException(nameof(Customer), "Email", dto.Email);
        }

        _mapper.Map(dto, customer);
        customer.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Customers.UpdateAsync(customer, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Customer with ID {CustomerId} updated successfully", id);

        return _mapper.Map<CustomerDto>(customer);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Deleting customer with ID: {CustomerId}", id);

        var customer = await _unitOfWork.Customers.GetByIdAsync(id, cancellationToken);

        if (customer == null)
        {
            _logger.LogWarning("Customer with ID {CustomerId} not found for deletion", id);
            throw new NotFoundException(nameof(Customer), id);
        }

        await _unitOfWork.Customers.DeleteAsync(customer, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Customer with ID {CustomerId} deleted successfully", id);
    }
}
