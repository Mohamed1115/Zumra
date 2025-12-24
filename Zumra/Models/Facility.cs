namespace Zumra.Models;

public class Facility
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string Type { get; set; }
    public string ImageUrl { get; set; }
    
    public ICollection<UserFacility> UserFacilities { get; set; }
}