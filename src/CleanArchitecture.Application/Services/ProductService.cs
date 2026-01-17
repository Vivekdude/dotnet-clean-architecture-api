using AutoMapper;
using CleanArchitecture.Application.Common;
using CleanArchitecture.Application.DTOs.Product;
using CleanArchitecture.Application.Interfaces;
using CleanArchitecture.Domain.Entities;
using CleanArchitecture.Domain.Exceptions;
using CleanArchitecture.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace CleanArchitecture.Application.Services;

public class ProductService : IProductService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<ProductService> _logger;

    public ProductService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<ProductService> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<ProductDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting product with ID: {ProductId}", id);

        var product = await _unitOfWork.Products.GetByIdAsync(id, cancellationToken);

        if (product == null)
        {
            _logger.LogWarning("Product with ID {ProductId} not found", id);
            throw new NotFoundException(nameof(Product), id);
        }

        return _mapper.Map<ProductDto>(product);
    }

    public async Task<IEnumerable<ProductDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting all products");

        var products = await _unitOfWork.Products.GetAllAsync(cancellationToken);
        return _mapper.Map<IEnumerable<ProductDto>>(products);
    }

    public async Task<PagedResult<ProductDto>> GetPagedAsync(ProductFilterDto filter, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting paged products. Page: {PageNumber}, Size: {PageSize}",
            filter.PageNumber, filter.PageSize);

        System.Linq.Expressions.Expression<Func<Product, bool>>? filterExpression = null;

        if (!string.IsNullOrWhiteSpace(filter.SearchTerm) ||
            !string.IsNullOrWhiteSpace(filter.Category) ||
            filter.MinPrice.HasValue ||
            filter.MaxPrice.HasValue ||
            filter.IsActive.HasValue)
        {
            filterExpression = p =>
                (string.IsNullOrWhiteSpace(filter.SearchTerm) ||
                    p.Name.Contains(filter.SearchTerm) ||
                    p.Description.Contains(filter.SearchTerm)) &&
                (string.IsNullOrWhiteSpace(filter.Category) || p.Category == filter.Category) &&
                (!filter.MinPrice.HasValue || p.Price >= filter.MinPrice.Value) &&
                (!filter.MaxPrice.HasValue || p.Price <= filter.MaxPrice.Value) &&
                (!filter.IsActive.HasValue || p.IsActive == filter.IsActive.Value);
        }

        System.Linq.Expressions.Expression<Func<Product, object>>? orderBy = filter.SortBy?.ToLower() switch
        {
            "name" => p => p.Name,
            "price" => p => p.Price,
            "category" => p => p.Category,
            "createdat" => p => p.CreatedAt,
            _ => p => p.Id
        };

        var (items, totalCount) = await _unitOfWork.Products.GetPagedAsync(
            filter.PageNumber,
            filter.PageSize,
            filterExpression,
            orderBy,
            !filter.SortDescending,
            cancellationToken);

        var dtos = _mapper.Map<IEnumerable<ProductDto>>(items);

        return new PagedResult<ProductDto>(dtos, totalCount, filter.PageNumber, filter.PageSize);
    }

    public async Task<IEnumerable<ProductDto>> GetByCategoryAsync(string category, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting products by category: {Category}", category);

        var products = await _unitOfWork.Products.GetByCategoryAsync(category, cancellationToken);
        return _mapper.Map<IEnumerable<ProductDto>>(products);
    }

    public async Task<ProductDto> CreateAsync(CreateProductDto dto, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating new product: {ProductName}", dto.Name);

        var product = _mapper.Map<Product>(dto);

        var createdProduct = await _unitOfWork.Products.AddAsync(product, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Product created with ID: {ProductId}", createdProduct.Id);

        return _mapper.Map<ProductDto>(createdProduct);
    }

    public async Task<ProductDto> UpdateAsync(int id, UpdateProductDto dto, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Updating product with ID: {ProductId}", id);

        var product = await _unitOfWork.Products.GetByIdAsync(id, cancellationToken);

        if (product == null)
        {
            _logger.LogWarning("Product with ID {ProductId} not found for update", id);
            throw new NotFoundException(nameof(Product), id);
        }

        _mapper.Map(dto, product);
        product.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Products.UpdateAsync(product, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Product with ID {ProductId} updated successfully", id);

        return _mapper.Map<ProductDto>(product);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Deleting product with ID: {ProductId}", id);

        var product = await _unitOfWork.Products.GetByIdAsync(id, cancellationToken);

        if (product == null)
        {
            _logger.LogWarning("Product with ID {ProductId} not found for deletion", id);
            throw new NotFoundException(nameof(Product), id);
        }

        await _unitOfWork.Products.DeleteAsync(product, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Product with ID {ProductId} deleted successfully", id);
    }
}
