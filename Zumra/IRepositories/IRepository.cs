using Zumra.Models;

namespace Zumra.IRepositories;

public interface IRepository<T> where T : class
{
    Task<T> CreatAsync(T entity, CancellationToken cn = default);
    Task UpdateAsync(T entity, CancellationToken cn = default);
    Task DeleteAsync(int id, CancellationToken cn = default);
    Task<List<T>> GetAllAsync(CancellationToken cn = default);
    Task<T?> GetByIdAsync(int id);
    Task<bool> IsOtpExpiredAsync(string email, string code);
    Task<Otp?> GetOtpAsync(string email, string code);
}