using CleanArchitecture.Application.Common;
using CleanArchitecture.Application.DTOs.Customer;
using CleanArchitecture.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace CleanArchitecture.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class CustomersController : ControllerBase
{
    private readonly ICustomerService _customerService;
    private readonly ILogger<CustomersController> _logger;

    public CustomersController(ICustomerService customerService, ILogger<CustomersController> logger)
    {
        _customerService = customerService;
        _logger = logger;
    }

    /// <summary>
    /// Get all customers with pagination and filtering
    /// </summary>
    [HttpGet]
    [SwaggerOperation(Summary = "Get all customers", Description = "Retrieve a paginated list of customers with optional filtering")]
    [SwaggerResponse(200, "Customers retrieved successfully", typeof(ApiResponse<PagedResult<CustomerDto>>))]
    public async Task<ActionResult<ApiResponse<PagedResult<CustomerDto>>>> GetAll(
        [FromQuery] CustomerFilterDto filter,
        CancellationToken cancellationToken)
    {
        var result = await _customerService.GetPagedAsync(filter, cancellationToken);
        return Ok(ApiResponse<PagedResult<CustomerDto>>.SuccessResponse(result, "Customers retrieved successfully"));
    }

    /// <summary>
    /// Get a customer by ID
    /// </summary>
    [HttpGet("{id:int}")]
    [SwaggerOperation(Summary = "Get customer by ID", Description = "Retrieve a single customer by their ID")]
    [SwaggerResponse(200, "Customer retrieved successfully", typeof(ApiResponse<CustomerDto>))]
    [SwaggerResponse(404, "Customer not found")]
    public async Task<ActionResult<ApiResponse<CustomerDto>>> GetById(int id, CancellationToken cancellationToken)
    {
        var customer = await _customerService.GetByIdAsync(id, cancellationToken);
        return Ok(ApiResponse<CustomerDto>.SuccessResponse(customer!, "Customer retrieved successfully"));
    }

    /// <summary>
    /// Get a customer by email
    /// </summary>
    [HttpGet("email/{email}")]
    [SwaggerOperation(Summary = "Get customer by email", Description = "Retrieve a customer by their email address")]
    [SwaggerResponse(200, "Customer retrieved successfully", typeof(ApiResponse<CustomerDto>))]
    [SwaggerResponse(404, "Customer not found")]
    public async Task<ActionResult<ApiResponse<CustomerDto>>> GetByEmail(
        string email,
        CancellationToken cancellationToken)
    {
        var customer = await _customerService.GetByEmailAsync(email, cancellationToken);
        return Ok(ApiResponse<CustomerDto>.SuccessResponse(customer!, "Customer retrieved successfully"));
    }

    /// <summary>
    /// Get customers by country
    /// </summary>
    [HttpGet("country/{country}")]
    [SwaggerOperation(Summary = "Get customers by country", Description = "Retrieve all customers from a specific country")]
    [SwaggerResponse(200, "Customers retrieved successfully", typeof(ApiResponse<IEnumerable<CustomerDto>>))]
    public async Task<ActionResult<ApiResponse<IEnumerable<CustomerDto>>>> GetByCountry(
        string country,
        CancellationToken cancellationToken)
    {
        var customers = await _customerService.GetByCountryAsync(country, cancellationToken);
        return Ok(ApiResponse<IEnumerable<CustomerDto>>.SuccessResponse(customers, "Customers retrieved successfully"));
    }

    /// <summary>
    /// Create a new customer
    /// </summary>
    [HttpPost]
    [SwaggerOperation(Summary = "Create a customer", Description = "Create a new customer")]
    [SwaggerResponse(201, "Customer created successfully", typeof(ApiResponse<CustomerDto>))]
    [SwaggerResponse(400, "Invalid request")]
    [SwaggerResponse(409, "Customer with email already exists")]
    public async Task<ActionResult<ApiResponse<CustomerDto>>> Create(
        [FromBody] CreateCustomerDto dto,
        CancellationToken cancellationToken)
    {
        var customer = await _customerService.CreateAsync(dto, cancellationToken);
        return CreatedAtAction(
            nameof(GetById),
            new { id = customer.Id },
            ApiResponse<CustomerDto>.SuccessResponse(customer, "Customer created successfully"));
    }

    /// <summary>
    /// Update an existing customer
    /// </summary>
    [HttpPut("{id:int}")]
    [SwaggerOperation(Summary = "Update a customer", Description = "Update an existing customer by ID")]
    [SwaggerResponse(200, "Customer updated successfully", typeof(ApiResponse<CustomerDto>))]
    [SwaggerResponse(404, "Customer not found")]
    [SwaggerResponse(400, "Invalid request")]
    [SwaggerResponse(409, "Customer with email already exists")]
    public async Task<ActionResult<ApiResponse<CustomerDto>>> Update(
        int id,
        [FromBody] UpdateCustomerDto dto,
        CancellationToken cancellationToken)
    {
        var customer = await _customerService.UpdateAsync(id, dto, cancellationToken);
        return Ok(ApiResponse<CustomerDto>.SuccessResponse(customer, "Customer updated successfully"));
    }

    /// <summary>
    /// Delete a customer
    /// </summary>
    [HttpDelete("{id:int}")]
    [SwaggerOperation(Summary = "Delete a customer", Description = "Delete a customer by ID")]
    [SwaggerResponse(200, "Customer deleted successfully")]
    [SwaggerResponse(404, "Customer not found")]
    public async Task<ActionResult<ApiResponse>> Delete(int id, CancellationToken cancellationToken)
    {
        await _customerService.DeleteAsync(id, cancellationToken);
        return Ok(ApiResponse.SuccessResponse("Customer deleted successfully"));
    }
}
