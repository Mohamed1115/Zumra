using Microsoft.EntityFrameworkCore;
using Zumra.Data;
using Zumra.IRepositories;
using Zumra.Models;

namespace Zumra.Repositories;

public class CouponRepository : Repository<Coupon>, ICouponRepository
{
    public CouponRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<Coupon?> GetByCodeAsync(string code)
    {
        return await _context.Coupons
            .FirstOrDefaultAsync(c => c.Code == code && c.IsActive && c.ExpiryDate > DateTime.Now);
    }
}
