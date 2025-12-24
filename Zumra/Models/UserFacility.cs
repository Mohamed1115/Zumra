using Zumra.Data;

namespace Zumra.Models;

public class UserFacility
{
    public int UserId { get; set; }
    public ApplicationUser User { get; set; }
    public int FacilityId { get; set; }
    public Facility Facility { get; set; }
    public FacilityRole Role { get; set; }
    public DateTime CreatedAt { get; set; }
    
    
    
}