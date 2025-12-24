using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Zumra.Data;
using Zumra.Models;

namespace Zumra.Utilites;

public class FacilityAuthorizationHandler : 
    AuthorizationHandler<FacilityRequirement, Facility>
{
    private readonly ApplicationDbContext _context;

    public FacilityAuthorizationHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        FacilityRequirement requirement,
        Facility facility)
    {
        // الحصول على User ID من Claims
        var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        if (string.IsNullOrEmpty(userId))
        {
            return;
        }

        // البحث عن role اليوزر في الـ facility
        var userFacility = await _context.UserFacilities
            .FirstOrDefaultAsync(uf => 
                uf.UserId == userId && 
                uf.FacilityId == facility.Id);

        if (userFacility == null)
        {
            return; // اليوزر مش عضو في الـ facility
        }

        // التحقق من الصلاحيات
        // بما أن 0 هو SuperAdmin و 1 هو Leader، فالأصغر هو الأعلى رتبة
        if (userFacility.Role <= requirement.MinimumRole)
        {
            context.Succeed(requirement);
        }
    }
}