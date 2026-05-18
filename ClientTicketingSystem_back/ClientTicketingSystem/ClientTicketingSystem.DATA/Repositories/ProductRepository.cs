using ClientTicketingSystem.CORE.Models;
using ClientTicketingSystem.DATA.Data;
using Microsoft.EntityFrameworkCore;
using SupportHub.DATA.Repositories.Interfaces;
using ClientTicketingSystem.DATA.Specification;
using ClientTicketingSystem.CORE.Specifications;
using ClientTicketingSystem.CORE.Dtos.ProductDtos;

namespace SupportHub.DATA.Repositories;
public class ProductRepository : GenericRepository<Product>,IProductRepository
{
    public ProductRepository(AppDbContext context) : base(context) { }
    public async Task<Product> GetProductWithItemsAsync(Guid id)
    {
        return (await _context.Set<Product>()
        .Include(c => c.ProductModules)
        .FirstOrDefaultAsync(c => c.Id == id))!;
    }
    public async Task<List<ProductWithCountDto>> GetProductsWithModulesCountAsync(ISpecification<Product> spec)
    {
        var query = SpecificationEvaluator<Product>.GetQuery(_context.Products.AsQueryable(), spec);

        return await query
            .Select(p => new ProductWithCountDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                ModulesCount = p.ProductModules!.Count()
            })
            .ToListAsync();
    }
}