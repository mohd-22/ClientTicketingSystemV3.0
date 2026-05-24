using ClientTicketingSystem.CORE.Models;
using ClientTicketingSystem.DATA.Data;
using Microsoft.EntityFrameworkCore;
using SupportHub.DATA.Repositories.Interfaces;
using ClientTicketingSystem.DATA.Specification;
using ClientTicketingSystem.CORE.Specifications;
using ClientTicketingSystem.CORE.Dtos.ProductDtos;
using AutoMapper;

namespace SupportHub.DATA.Repositories;
public class ProductRepository : GenericRepository<Product>,IProductRepository
{
    private readonly IMapper _mapper;

    public ProductRepository(AppDbContext context, IMapper mapper) : base(context)
    {
        _mapper = mapper;
    }
    public async Task<Product> GetProductWithItemsAsync(Guid id)
    {
        return (await _context.Set<Product>()
        .FirstOrDefaultAsync(c => c.Id == id))!;
    }
    public async Task<List<ProductWithCountDto>> GetProductsAsync(ISpecification<Product> spec)
    {
        var query = SpecificationEvaluator<Product>.GetQuery(_context.Products.AsQueryable(), spec);

        var products = await query.ToListAsync();
        /*
        // Previous manual mapping (kept commented):
        var productsDto = products.Select(p => new ProductWithCountDto
        {
            Id = p.Id,
            Name = p.Name,
            Description = p.Description,
            ItemsCount = p.Items == null ? 0 : p.Items.Count
        }).ToList();
        return productsDto;
        */

        return _mapper.Map<List<ProductWithCountDto>>(products);
    }
}