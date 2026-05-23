using ClientTicketingSystem.CORE.Dtos.ProductDtos;
using ClientTicketingSystem.CORE.Models;
using ClientTicketingSystem.CORE.Specifications;
namespace SupportHub.DATA.Repositories.Interfaces
{
    public interface IProductRepository : IGenericRepository<Product>
    {
       Task<Product> GetProductWithItemsAsync(Guid id);
       Task<List<ProductWithCountDto>> GetProductsAsync(ISpecification<Product> spec);
    }
}