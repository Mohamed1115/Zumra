using Zumra.Models;

namespace Zumra.IRepositories;

public interface ICouponRepository : IRepository<Coupon>
{
    Task<Coupon?> GetByCodeAsync(string code);
}
