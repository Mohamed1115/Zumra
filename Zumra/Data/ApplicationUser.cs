using Microsoft.AspNetCore.Identity;
using Zumra.Models;

namespace Zumra.Data;

public class ApplicationUser:IdentityUser
{
    public string Name { get; set; }
    public int TotalCarts { get; set; }
    public int PhoneNumber{ get; set; }
    public int ImageId { get; set; }
    public UserImage? Image { get; set; }

    public ICollection<UserFacility> UserFacilities { get; set; }

    
}