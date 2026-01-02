using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using Stripe;
using Zumra.Data;
using Zumra.IRepositories;
using Zumra.Models;
using Zumra.Repositories;
using Zumra.Utilites;
using Zumra.Utilites.DBInitializer;

namespace Zumra;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.

        builder.Services.AddControllers();
        // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
        builder.Services.AddOpenApi();
        
         builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(
                builder.Configuration.GetConnectionString("DefaultConnection"),
                sqlServerOptions => sqlServerOptions.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    errorNumbersToAdd: null)));

        // Identity
        builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                // Password settings
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequiredLength = 6;
            
                // Lockout settings - تعطيل أو تقليل القفل
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5); // مدة القفل 5 دقائق فقط
                options.Lockout.MaxFailedAccessAttempts = 10; // 10 محاولات قبل القفل
                options.Lockout.AllowedForNewUsers = true;
            
                // User settings
                options.User.RequireUniqueEmail = true;
                options.SignIn.RequireConfirmedEmail = true; // تعطيل تأكيد البريد للاختبار
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();
        builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultSignInScheme = IdentityConstants.ExternalScheme; // For external login flow
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = builder.Configuration["Jwt:Issuer"],
                ValidAudience = builder.Configuration["Jwt:Audience"],
                // ClockSkew = TimeSpan.Zero,
                // RequireExpirationTime = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
            };
        })
        .AddGoogle("google", opt =>
        {
            var googleAuth = builder.Configuration.GetSection("Authentication:Google");
            opt.ClientId = googleAuth["ClientId"]??"";
            opt.ClientSecret = googleAuth["ClientSecret"]??"";
            opt.SignInScheme = IdentityConstants.ExternalScheme;
        });

        // Email Sender
        builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
        builder.Services.AddTransient<Microsoft.AspNetCore.Identity.UI.Services.IEmailSender, EmailSender>();
        builder.Services.AddTransient<Zumra.IRepositories.IEmailSender, EmailSender>();

        builder.Services.AddScoped<IRepository<Otp>, Repository<Otp>>();
        
        builder.Services.AddScoped<ICartRepository, CartRepository>();
        builder.Services.AddScoped<ICouponRepository, CouponRepository>();
        builder.Services.AddScoped<IDBInitializer, DBInitializer>();

        StripeConfiguration.ApiKey = builder.Configuration["Stripe:SecretKey"];

        builder.Services.AddAuthorization(options =>
        {
            options.AddPolicy("FacilitySuperAdmin", policy =>
                policy.Requirements.Add(new FacilityRequirement(FacilityRole.SuperAdmin)));
            // Policy للـ Leader فقط
            options.AddPolicy("FacilityLeader", policy =>
                policy.Requirements.Add(new FacilityRequirement(FacilityRole.Leader)));

            // Policy للـ Instructor وأعلى
            options.AddPolicy("FacilityInstructor", policy =>
                policy.Requirements.Add(new FacilityRequirement(FacilityRole.Instructor)));

            // Policy للـ Member وأعلى (أي عضو)
            options.AddPolicy("FacilityMember", policy =>
                policy.Requirements.Add(new FacilityRequirement(FacilityRole.Member)));
        });

        // تسجيل الـ Handler
        builder.Services.AddScoped<IAuthorizationHandler, FacilityAuthorizationHandler>();
        


        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            app.MapScalarApiReference();
        }

        app.UseHttpsRedirection();
        
        app.UseStaticFiles(); // Enable serving static files from wwwroot
        
        app.UseAuthentication();

        app.UseAuthorization();
        using (var  scope = app.Services.CreateScope())
        {
            var initializer = scope.ServiceProvider.GetRequiredService<IDBInitializer>();
            initializer.Initialize();
        }


        app.MapControllers();

        app.Run();
    }
}