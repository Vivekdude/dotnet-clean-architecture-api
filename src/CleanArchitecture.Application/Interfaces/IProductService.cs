using CleanArchitecture.Application.Common;
using CleanArchitecture.Application.DTOs.Product;

namespace CleanArchitecture.Application.Interfaces;

public interface IProductService
{
    Task<ProductDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IEnumerable<ProductDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<PagedResult<ProductDto>> GetPagedAsync(ProductFilterDto filter, CancellationToken cancellationToken = default);
    Task<IEnumerable<ProductDto>> GetByCategoryAsync(string category, CancellationToken cancellationToken = default);
    Task<ProductDto> CreateAsync(CreateProductDto dto, CancellationToken cancellationToken = default);
    Task<ProductDto> UpdateAsync(int id, UpdateProductDto dto, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}
