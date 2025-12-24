using Zumra.Models;

namespace Zumra.IRepositories;

public interface ICartRepository:IRepository<Cart>
{
    Task<List<Cart>> GetByIdUserAsync(string userId);
    Task<Cart?> GetByBookAndUserAsync(int movieId, string userId);
    Task DeleteByUserAsync(string userId);

}