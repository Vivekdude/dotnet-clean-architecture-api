using CleanArchitecture.Application.Common;

namespace CleanArchitecture.Application.DTOs.Customer;

public class CustomerFilterDto : PaginationParameters
{
    public string? SearchTerm { get; set; }
    public string? Country { get; set; }
    public string? City { get; set; }
    public bool? IsActive { get; set; }
    public string? SortBy { get; set; }
    public bool SortDescending { get; set; }
}
