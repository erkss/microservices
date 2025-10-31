using Microsoft.EntityFrameworkCore;
using StockService.Data;
using StockService.Models;
using StockService.Dtos;

namespace StockService.Services
{
    public class ProductService(StockContext context) : IProductService
    {
        private readonly StockContext _context = context;

        public async Task<IEnumerable<Product>> GetAllProductsAsync()
        {
            return await _context.Products!.ToListAsync();
        }

        public async Task<Product?> GetProductByIdAsync(int id)
        {
            return await _context.Products!.FindAsync(id);
        }

        public async Task<Product?> CreateProductAsync(Product product)
        {

            try
            {
                if (product == null)
                    return null;
           
                _context.Products!.Add(product);
                await _context.SaveChangesAsync();

                return product;
            }
            catch(DbUpdateException)
            {
                return null;
            }
        }

        public async Task<Product?> UpdateProductAsync(int id, UpdateProductDto dto)
        {
            var product = await _context.Products!.FindAsync(id);

            if (product == null)
                return null;

            if (dto.Price.HasValue && dto.Price.Value <= 0)
                throw new ArgumentException("Price must be positive.");

            if (dto.Quantity.HasValue && dto.Quantity.Value < 0)
                throw new ArgumentException("Quantity cannot be negative.");

            try
            {
                if (!string.IsNullOrWhiteSpace(dto.Name))
                    product.Name = dto.Name;

                if (!string.IsNullOrWhiteSpace(dto.Description))
                    product.Description = dto.Description;

                if (dto.Quantity.HasValue)
                    product.Quantity = dto.Quantity.Value;

                if (dto.Price.HasValue)
                    product.Price = dto.Price.Value;

                await _context.SaveChangesAsync();

                return product;
            }
            catch(DbUpdateException)
            {
                return null;
            }
        }

        public async Task<bool> DeleteProductAsync(int id)
        {
            try
            {
                var product = await _context.Products!.FindAsync(id);

                if (product == null)
                    return false;
         
                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
                
                return true;
            }
            catch(DbUpdateException)
            {
                return false;
            }
        }
    }
}