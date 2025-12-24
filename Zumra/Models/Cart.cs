using Zumra.Data;

namespace Zumra.Models;

public class Cart
{
    public int Id { get; set; }
    public string UserId { get; set; }
    public ApplicationUser User { get; set; }
    public int BookId { get; set; }
    // public Book Book { get; set; }
    
    public int Quantity { get; set; }
    public int TotalPrice { get; set; }
    
    public int? CouponId { get; set; }
    public Coupon? Coupon { get; set; }
}