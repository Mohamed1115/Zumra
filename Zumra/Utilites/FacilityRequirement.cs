using Microsoft.AspNetCore.Authorization;
using Zumra.Models;

namespace Zumra.Utilites;

public class FacilityRequirement : IAuthorizationRequirement
{
    public FacilityRole MinimumRole { get; }

    public FacilityRequirement(FacilityRole minimumRole)
    {
        MinimumRole = minimumRole;
    }
}