// using Microsoft.AspNetCore.Identity;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Zumra.Models;

namespace Zumra.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options):base(options)
    {
        
    }

    public DbSet<Otp> Otps { get; set; }
    // public DbSet<Book> Books { get; set; }
    // public DbSet<Author> Authors { get; set; }
    // public DbSet<Category> Categories { get; set; }
    // public DbSet<Comment> Comments  { get; set; }
    public DbSet<Cart> Carts { get; set; }
    public DbSet<Coupon> Coupons { get; set; }
    public DbSet<Facility> Facilities { get; set; }
    public DbSet<UserFacility> UserFacilities { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // تكوين Many-to-Many
        modelBuilder.Entity<UserFacility>()
            .HasKey(uf => new { uf.UserId, uf.FacilityId });

        modelBuilder.Entity<UserFacility>()
            .HasOne(uf => uf.User)
            .WithMany(u => u.UserFacilities)
            .HasForeignKey(uf => uf.UserId);

        modelBuilder.Entity<UserFacility>()
            .HasOne(uf => uf.Facility)
            .WithMany(f => f.UserFacilities)
            .HasForeignKey(uf => uf.FacilityId);
    }

    
}