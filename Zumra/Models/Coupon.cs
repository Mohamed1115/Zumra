using System.ComponentModel.DataAnnotations;

namespace Zumra.Models;

public class Coupon
{
    public int Id { get; set; }
    
    [Required]
    public string Code { get; set; }
    
    public int DiscountAmount { get; set; } // Fixed amount discount
    
    public DateTime ExpiryDate { get; set; }
    
    public bool IsActive { get; set; } = true;
}
