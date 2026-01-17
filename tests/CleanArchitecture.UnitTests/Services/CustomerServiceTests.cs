using AutoMapper;
using CleanArchitecture.Application.DTOs.Customer;
using CleanArchitecture.Application.Mappings;
using CleanArchitecture.Application.Services;
using CleanArchitecture.Domain.Entities;
using CleanArchitecture.Domain.Exceptions;
using CleanArchitecture.Domain.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using System.Linq.Expressions;

namespace CleanArchitecture.UnitTests.Services;

public class CustomerServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ICustomerRepository> _customerRepositoryMock;
    private readonly IMapper _mapper;
    private readonly Mock<ILogger<CustomerService>> _loggerMock;
    private readonly CustomerService _customerService;

    public CustomerServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _customerRepositoryMock = new Mock<ICustomerRepository>();
        _loggerMock = new Mock<ILogger<CustomerService>>();

        var mapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<MappingProfile>();
        });
        _mapper = mapperConfig.CreateMapper();

        _unitOfWorkMock.Setup(u => u.Customers).Returns(_customerRepositoryMock.Object);

        _customerService = new CustomerService(_unitOfWorkMock.Object, _mapper, _loggerMock.Object);
    }

    [Fact]
    public async Task GetByIdAsync_WithValidId_ReturnsCustomer()
    {
        // Arrange
        var customerId = 1;
        var customer = new Customer
        {
            Id = customerId,
            FirstName = "John",
            LastName = "Doe",
            Email = "john@example.com",
            Phone = "+1-555-0101",
            Address = "123 Main St",
            City = "New York",
            Country = "USA",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _customerRepositoryMock
            .Setup(r => r.GetByIdAsync(customerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(customer);

        // Act
        var result = await _customerService.GetByIdAsync(customerId);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(customerId);
        result.FirstName.Should().Be("John");
        result.Email.Should().Be("john@example.com");
    }

    [Fact]
    public async Task GetByIdAsync_WithInvalidId_ThrowsNotFoundException()
    {
        // Arrange
        var customerId = 999;

        _customerRepositoryMock
            .Setup(r => r.GetByIdAsync(customerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Customer?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _customerService.GetByIdAsync(customerId));
    }

    [Fact]
    public async Task GetByEmailAsync_WithValidEmail_ReturnsCustomer()
    {
        // Arrange
        var email = "john@example.com";
        var customer = new Customer
        {
            Id = 1,
            FirstName = "John",
            LastName = "Doe",
            Email = email,
            CreatedAt = DateTime.UtcNow
        };

        _customerRepositoryMock
            .Setup(r => r.GetByEmailAsync(email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(customer);

        // Act
        var result = await _customerService.GetByEmailAsync(email);

        // Assert
        result.Should().NotBeNull();
        result!.Email.Should().Be(email);
    }

    [Fact]
    public async Task CreateAsync_WithValidData_ReturnsCreatedCustomer()
    {
        // Arrange
        var createDto = new CreateCustomerDto
        {
            FirstName = "Jane",
            LastName = "Smith",
            Email = "jane@example.com",
            Phone = "+1-555-0102",
            Address = "456 Oak Ave",
            City = "Los Angeles",
            Country = "USA"
        };

        _customerRepositoryMock
            .Setup(r => r.EmailExistsAsync(createDto.Email, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _customerRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Customer>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Customer c, CancellationToken _) =>
            {
                c.Id = 1;
                return c;
            });

        _unitOfWorkMock
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _customerService.CreateAsync(createDto);

        // Assert
        result.Should().NotBeNull();
        result.FirstName.Should().Be("Jane");
        result.Email.Should().Be("jane@example.com");
        result.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task CreateAsync_WithDuplicateEmail_ThrowsConflictException()
    {
        // Arrange
        var createDto = new CreateCustomerDto
        {
            FirstName = "Jane",
            LastName = "Smith",
            Email = "existing@example.com"
        };

        _customerRepositoryMock
            .Setup(r => r.EmailExistsAsync(createDto.Email, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act & Assert
        await Assert.ThrowsAsync<ConflictException>(() =>
            _customerService.CreateAsync(createDto));
    }

    [Fact]
    public async Task UpdateAsync_WithValidData_ReturnsUpdatedCustomer()
    {
        // Arrange
        var customerId = 1;
        var existingCustomer = new Customer
        {
            Id = customerId,
            FirstName = "John",
            LastName = "Doe",
            Email = "john@example.com",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var updateDto = new UpdateCustomerDto
        {
            FirstName = "Johnny",
            LastName = "Doe",
            Email = "johnny@example.com",
            Phone = "+1-555-0103",
            Address = "789 Pine St",
            City = "Chicago",
            Country = "USA",
            IsActive = true
        };

        _customerRepositoryMock
            .Setup(r => r.GetByIdAsync(customerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingCustomer);

        _customerRepositoryMock
            .Setup(r => r.EmailExistsAsync(updateDto.Email, customerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _unitOfWorkMock
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _customerService.UpdateAsync(customerId, updateDto);

        // Assert
        result.Should().NotBeNull();
        result.FirstName.Should().Be("Johnny");
        result.Email.Should().Be("johnny@example.com");
    }

    [Fact]
    public async Task UpdateAsync_WithDuplicateEmail_ThrowsConflictException()
    {
        // Arrange
        var customerId = 1;
        var existingCustomer = new Customer
        {
            Id = customerId,
            FirstName = "John",
            LastName = "Doe",
            Email = "john@example.com",
            CreatedAt = DateTime.UtcNow
        };

        var updateDto = new UpdateCustomerDto
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "existing@example.com",
            IsActive = true
        };

        _customerRepositoryMock
            .Setup(r => r.GetByIdAsync(customerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingCustomer);

        _customerRepositoryMock
            .Setup(r => r.EmailExistsAsync(updateDto.Email, customerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act & Assert
        await Assert.ThrowsAsync<ConflictException>(() =>
            _customerService.UpdateAsync(customerId, updateDto));
    }

    [Fact]
    public async Task DeleteAsync_WithValidId_DeletesCustomer()
    {
        // Arrange
        var customerId = 1;
        var customer = new Customer
        {
            Id = customerId,
            FirstName = "John",
            LastName = "Doe",
            Email = "john@example.com",
            CreatedAt = DateTime.UtcNow
        };

        _customerRepositoryMock
            .Setup(r => r.GetByIdAsync(customerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(customer);

        _unitOfWorkMock
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        await _customerService.DeleteAsync(customerId);

        // Assert
        _customerRepositoryMock.Verify(r => r.DeleteAsync(customer, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetByCountryAsync_ReturnsCustomersInCountry()
    {
        // Arrange
        var country = "USA";
        var customers = new List<Customer>
        {
            new() { Id = 1, FirstName = "John", LastName = "Doe", Email = "john@example.com", Country = country, CreatedAt = DateTime.UtcNow },
            new() { Id = 2, FirstName = "Jane", LastName = "Smith", Email = "jane@example.com", Country = country, CreatedAt = DateTime.UtcNow }
        };

        _customerRepositoryMock
            .Setup(r => r.GetByCountryAsync(country, It.IsAny<CancellationToken>()))
            .ReturnsAsync(customers);

        // Act
        var result = await _customerService.GetByCountryAsync(country);

        // Assert
        result.Should().HaveCount(2);
        result.Should().AllSatisfy(c => c.Country.Should().Be(country));
    }

    [Fact]
    public async Task GetPagedAsync_ReturnsPagedResult()
    {
        // Arrange
        var filter = new CustomerFilterDto
        {
            PageNumber = 1,
            PageSize = 10
        };

        var customers = new List<Customer>
        {
            new() { Id = 1, FirstName = "John", LastName = "Doe", Email = "john@example.com", CreatedAt = DateTime.UtcNow },
            new() { Id = 2, FirstName = "Jane", LastName = "Smith", Email = "jane@example.com", CreatedAt = DateTime.UtcNow }
        };

        _customerRepositoryMock
            .Setup(r => r.GetPagedAsync(
                filter.PageNumber,
                filter.PageSize,
                It.IsAny<Expression<Func<Customer, bool>>>(),
                It.IsAny<Expression<Func<Customer, object>>>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((customers, 2));

        // Act
        var result = await _customerService.GetPagedAsync(filter);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(2);
        result.PageNumber.Should().Be(1);
        result.PageSize.Should().Be(10);
    }
}
