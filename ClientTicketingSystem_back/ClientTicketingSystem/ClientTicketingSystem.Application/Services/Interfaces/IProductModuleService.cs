using ClientTicketingSystem.Application.Helpers;
using ClientTicketingSystem.CORE.Dtos;
using ClientTicketingSystem.CORE.Dtos.ProductDtos;
namespace ClientTicketingSystem.Application.Services.Interfaces;
public interface IProductModuleService
{
    Task<ApiResponse<CreateModuleDto>> CreateModuleAsync(CreateModuleDto dto, Guid UserId);
    Task<ApiResponse<ModuleDto>> GetModuleByIdAsync(Guid Id);
    Task<ApiResponse<PaginationDto<ModuleDto>>> GetAllModulesAsync(string? search, string? sort, Guid? productId, int pageIndex, int pageSize);
    Task<ApiResponse<UpdateProductDto>> UpdateModule(UpdateProductDto Itemdto,Guid UserdId);
    Task<ApiResponse<bool>> DeleteModule(Guid Id);
}
