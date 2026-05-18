using ClientTicketingSystem.Application.Helpers;
using ClientTicketingSystem.CORE.Dtos;
using ClientTicketingSystem.CORE.Dtos.ProductDtos;

namespace ClientTicketingSystem.Application.Services.Interfaces;
public interface IProductService
{
    Task<ApiResponse<ProductDto>> GetProductById(Guid ProductId);
    Task<ApiResponse<PaginationDto<ProductWithCountDto>>> GetAllProducts(string? search, string? sort, int pageIndex, int pageSize);
    Task<ApiResponse<CreateProductDto>> CreateProduct(CreateProductDto CatDto, Guid UserId);
    Task<ApiResponse<UpdateProductDto>> UpdateProduct(UpdateProductDto CatDto, Guid UserId);
    Task<ApiResponse<bool>> DeleteProduct(Guid Id);
}
