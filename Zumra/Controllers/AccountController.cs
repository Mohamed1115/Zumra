using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
// using BookAPI.ModelView;
using Zumra;
using Zumra.Data;
using Zumra.IRepositories;
using Zumra.Models;
using Mapster;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Zumra.Data;
using Zumra.IRepositories;
using Zumra.Models;
using Microsoft.IdentityModel.Tokens;
using Zumra.DTOs.Request;
// using LoginRequest = Zumra.ModelView.LoginRequest;
// using RegisterRequest = Zumra.ModelView.RegisterRequest;

namespace Zumra.Controllers;
[Route("Auth/[controller]/[action]")]
[ApiController]

public class AccountController:ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IEmailSender _emailSender;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IRepository<Otp> _iRepositories;
    private readonly IConfiguration _configuration;
    private readonly IWebHostEnvironment _webHostEnvironment;

    public AccountController(IRepository<Otp> iRepositories, SignInManager<ApplicationUser> signInManager, IEmailSender emailSender, UserManager<ApplicationUser> userManager, IConfiguration configuration, IWebHostEnvironment webHostEnvironment)
    {
        _iRepositories = iRepositories;
        _signInManager = signInManager;
        _emailSender = emailSender;
        _userManager = userManager;
        _configuration = configuration;
        _webHostEnvironment = webHostEnvironment;
    }
    
    [HttpPost]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new
            {
                success = false,
                message = "Invalid input data.",
                errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
            });
        }
        
        var user = await _userManager.FindByNameAsync(request.UserName)
                   ?? await _userManager.FindByEmailAsync(request.UserName);
                   
        if (user == null)
        {
            return BadRequest(new
            {
                success = false,
                message = "Invalid username or password."
            });
        }

        var userName = user.UserName ?? user.Email ?? request.UserName;
        var result = await _signInManager.PasswordSignInAsync(
            userName, 
            request.Password, 
            isPersistent: request.RememberMe, 
            lockoutOnFailure: true
        );

       
        if (result.Succeeded)
        {
            var token = await GenerateJwtTokenAsync(user);
            return Ok(new
            {
                success = true,
                message = "Login successful.",
                token = token
            });
        }

       
        if (result.IsLockedOut)
        {
            var lockoutEnd = user.LockoutEnd?.LocalDateTime;
            if (lockoutEnd.HasValue && lockoutEnd.Value > DateTime.Now)
            {
                var remainingTime = lockoutEnd.Value - DateTime.Now;
                return BadRequest(new
                {
                    success = false,
                    message = $"Your account is locked. Please try again after {remainingTime.Minutes} minutes and {remainingTime.Seconds} seconds.",
                    isLockedOut = true,
                    lockoutEnd = lockoutEnd.Value
                });
            }
            
            return BadRequest(new
            {
                success = false,
                message = "Your account is locked. Please try again later.",
                isLockedOut = true
            });
        }
    
        if (result.IsNotAllowed)
        {
            return BadRequest(new
            {
                success = false,
                message = "Please confirm your email first.",
                requiresEmailConfirmation = true
            });
        }

        // Default error للـ invalid credentials
        return BadRequest(new
        {
            success = false,
            message = "Invalid username or password."
        });
    }


    private async Task<string> GenerateJwtTokenAsync(ApplicationUser user)
    {
        var Key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
        var cred = new SigningCredentials(Key,SecurityAlgorithms.HmacSha256);
        var userRoles= await _userManager.GetRolesAsync(user);
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Name, user.UserName),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role,String.Join(",", userRoles)),
            new Claim(JwtRegisteredClaimNames.Jti,DateTime.Now.ToString("yy-MM-dd"))

        };
        
        
        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.Now.AddDays(1),
            signingCredentials:cred
            );
        return new JwtSecurityTokenHandler().WriteToken(token);
        
    }
    
    
     [HttpPost]
    public async Task<IActionResult> Register(RegisterRequest vm)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new
            {
                success = false,
                message = "Invalid input data.",
                errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
            });
        }
        
        var user = vm.Adapt<ApplicationUser>();
        user.UserName = vm.Email;
        var Don = await _userManager.CreateAsync(user,vm.Password);
        
        
        if (Don.Succeeded)
        {
            _userManager.AddToRoleAsync(user!, SD.UserRole).GetAwaiter().GetResult();
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var link = Url.Action(nameof(Confirm),"Account",new{token=token,id=user.Id},Request.Scheme);
            
            var templatePath = Path.Combine(_webHostEnvironment.ContentRootPath, "Templates", "ConfirmEmail.html");
            var template = await System.IO.File.ReadAllTextAsync(templatePath);
            var htmlMessage = template.Replace("{link}", link);

            await _emailSender.SendEmailAsync(vm.Email, "Confirm your email",htmlMessage);
            return Ok(new
            {
                success = true,
                message = "Registration successful. Please check your email to confirm your account."
            });
            
        }
        
        return BadRequest(new
        {
            success = false,
            message = "User registration failed.",
            errors = Don.Errors.Select(e => e.Description)
        });
        
    }
    
    [HttpGet]
    public async Task<IActionResult> Confirm(string token, string id)
    {
        var user = await _userManager.FindByIdAsync(id);

        if (user == null)
            return BadRequest(new
            {
                success = false,
                message = "Invalid user ID."
            });

        var result = await _userManager.ConfirmEmailAsync(user, token);

        if (!result.Succeeded)
            return BadRequest(new
            {
                success = false,
                message = "Invalid or expired token."
            });

        return Ok(new
        {
            success = true,
            message = "Email confirmed successfully. You can now log in."
        });
    }


    [HttpPost]
    public async Task<IActionResult> ForgotPassword(string Email)
    {
        var user = await _userManager.FindByEmailAsync(Email);
        
        if (user != null)
        {
            
            string otpCode = new Random().Next(100000, 999999).ToString();
            var otp = new Otp(Email, otpCode);
            await _iRepositories.CreatAsync(otp);
            
            // var link = Url.Action(nameof(Confirm),"Account",new{token=token,id=user.Id},Request.Scheme);
             var templatePath = Path.Combine(_webHostEnvironment.ContentRootPath, "Templates", "ResetPassword.html");
             var template = await System.IO.File.ReadAllTextAsync(templatePath);
             var htmlMessage = template
                 .Replace("{0}", otpCode[0].ToString())
                 .Replace("{1}", otpCode[1].ToString())
                 .Replace("{2}", otpCode[2].ToString())
                 .Replace("{3}", otpCode[3].ToString())
                 .Replace("{4}", otpCode[4].ToString())
                 .Replace("{5}", otpCode[5].ToString());

            await _emailSender.SendEmailAsync(Email, "Reset Password",htmlMessage);
            return Ok(new
            {
                success = true,
                message = "If the email exists, a password reset code has been sent.",
                email = Email
            });
            
        }
        
        // Return same message for security (avoid user enumeration)
        return Ok(new
        {
            success = true,
            message = "If the email exists, a password reset code has been sent."
        });

    }

    [HttpPost]
    public async Task<IActionResult> VerifyOTP(string OtpCode,string Email)
    {
        var otp = await _iRepositories.GetOtpAsync(Email, OtpCode);
        if (otp == null)
        {
            return BadRequest(new
            {
                success = false,
                message = "Invalid OTP code."
            });
        }
        
        bool isValid = await _iRepositories.IsOtpExpiredAsync(Email, OtpCode);
        if (!isValid)
        {
            return BadRequest(new
            {
                success = false,
                message = "OTP code has expired or is invalid."
            });
        }
        
        otp.IsUsed = true;
        await _iRepositories.UpdateAsync(otp);
        
        return Ok(new
        {
            success = true,
            message = "OTP verified successfully. You can now reset your password.",
            email = Email
        });
    }

    [HttpPost]
    public async Task<IActionResult> ResetPassword(ResetPasswordRequest vm)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new
            {
                success = false,
                message = "Invalid input data.",
                errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
            });
        }
        
        var user = await _userManager.FindByEmailAsync(vm.Email);
        if (user == null)
        {
            return BadRequest(new
            {
                success = false,
                message = "User not found."
            });
        }
        
        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var result = await _userManager.ResetPasswordAsync(user, token, vm.Password);
        
        if (!result.Succeeded)
        {
            return BadRequest(new
            {
                success = false,
                message = "Password reset failed.",
                errors = result.Errors.Select(e => e.Description)
            });
        }
        
        return Ok(new
        {
            success = true,
            message = "Password has been reset successfully. You can now log in with your new password."
        });
    }
    
    [HttpPost]
    public async Task<IActionResult> ResendconfirmEmail(string Email)
    {
        
        var user = await _userManager.FindByEmailAsync(Email);
        if (user == null)
            return NotFound();
        
        
        if (!user.EmailConfirmed)
        {
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var link = Url.Action(nameof(Confirm),"Account",new{token=token,id=user.Id},Request.Scheme);
            var templatePath = Path.Combine(_webHostEnvironment.ContentRootPath, "Templates", "ConfirmEmail.html");
            var template = await System.IO.File.ReadAllTextAsync(templatePath);
            var htmlMessage = template.Replace("{link}", link);

            await _emailSender.SendEmailAsync(Email, "Confirm your email",htmlMessage);
            return Ok(new
            {
                success = true,
                message = "Registration successful. Please check your email to confirm your account."
            });
            
        }
        
        return BadRequest(new
        {
            success = false,
            message = "Email Already Confirmed",
            // errors = Don.Errors.Select(e => e.Description)
        });
        
    }
    
    
}