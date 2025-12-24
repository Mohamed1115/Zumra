using Microsoft.EntityFrameworkCore;
using Zumra.Data;
using Zumra.IRepositories;
using Zumra.Models;

namespace Zumra.Repositories;

public class CartRepository:Repository<Cart>, ICartRepository
{
    public CartRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<List<Cart>> GetByIdUserAsync(string userId)
    {
        return await _context.Carts
            // .Include(c =>c.Book)
            .Include(c => c.Coupon)
            .Where(c => c.UserId == userId)
            .ToListAsync();
    }
    
    
    public async Task<Cart?> GetByBookAndUserAsync(int BookId, string userId)
    {
        return await _context.Carts
            .FirstOrDefaultAsync(c => c.BookId == BookId && c.UserId == userId);
    }

    public async Task DeleteByUserAsync(string userId)
    {
        await _context.Carts
            .Where(c => c.UserId == userId)
            .ExecuteDeleteAsync();
        return;
    }

    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
}