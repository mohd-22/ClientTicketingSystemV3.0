using ClientTicketingSystem.Application.Helpers;
using ClientTicketingSystem.CORE.Dtos;
using ClientTicketingSystem.CORE.Dtos.ProductDtos;

namespace ClientTicketingSystem.Application.Services.Interfaces;
public interface IProductService
{
    Task<ApiResponse<PaginationDto<ProductDto>>> GetAllProducts(
        string? search, 
        string? sort,
        int pageIndex,
        int pageSize
        );
    }
