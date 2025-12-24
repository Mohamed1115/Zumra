using Zumra.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Zumra.Utilites.DBInitializer;

public class DBInitializer:IDBInitializer
{
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    public DBInitializer(RoleManager<IdentityRole> roleManager, ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _roleManager = roleManager;
        _context = context;
        _userManager = userManager;
    }
    public async Task Initialize()
    {
        
        if(_context.Database.GetPendingMigrations().Any())
            _context.Database.Migrate();
        
        
        if (_roleManager.Roles == null || !_roleManager.Roles.Any())
        {
            _roleManager.CreateAsync(new IdentityRole(SD.SuperAdminRole)).GetAwaiter().GetResult();
            _roleManager.CreateAsync(new IdentityRole(SD.AdminRole)).GetAwaiter().GetResult();
            _roleManager.CreateAsync(new IdentityRole(SD.UserRole)).GetAwaiter().GetResult();
            // _roleManager.CreateAsync(new IdentityRole("User3"));
            _userManager.CreateAsync(new()
            {
                Name = "Super Admin",
                Email = "SuperAdmin@gmail.com",
                EmailConfirmed = true,
                UserName = "SuperAdmin",
            }, "Admin$1234").GetAwaiter().GetResult();
            var user = _userManager.FindByEmailAsync("SuperAdmin@gmail.com").GetAwaiter().GetResult();
            _userManager.AddToRoleAsync(user!, SD.SuperAdminRole).GetAwaiter().GetResult();
        }       
        
        
    }
}