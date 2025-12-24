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
    public AccountController(IRepository<Otp> iRepositories, SignInManager<ApplicationUser> signInManager, IEmailSender emailSender, UserManager<ApplicationUser> userManager)
    {
        _iRepositories = iRepositories;
        _signInManager = signInManager;
        _emailSender = emailSender;
        _userManager = userManager;
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
        var Key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("AlksqozSj1zWgZgAwWF8cwz8nQCSfYsH"));
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
            issuer:"https://localhost:7042;http://localhost:5142",
            audience:"https://localhost:7042;http://localhost:5142,https://localhost:5000;http://localhost:4200",
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
            var htmlMessage = $@"
                        <html>
                            <body style='margin: 0; padding: 0; background-color: #f3f6fb; font-family: Arial, sans-serif;'>
                                <table width='100%' cellspacing='0' cellpadding='0' style='background-color: #f3f6fb; padding: 40px 0;'>
                                    <tr>
                                        <td align='center'>
                                            <!-- Main Card -->
                                            <table width='600' cellspacing='0' cellpadding='0' 
                                                   style='background: #ffffff; border-radius: 10px; padding: 40px; box-shadow: 0px 4px 15px rgba(0,0,0,0.1);'>
                                                <tr>
                                                    <td align='center'>
                                                        <h2 style='color: #1e3a8a; margin-bottom: 10px; font-size: 26px;'>
                                                            Welcome to Our Website!
                                                        </h2>
                                                        <p style='color: #555; font-size: 16px; margin-bottom: 30px;'>
                                                            Thanks for joining us! Please confirm your email address by clicking the button below.
                                                        </p>
                                                        
                                                        <!-- Button -->
                                                        <a href='{link}' style='display: inline-block; padding: 14px 28px; background: linear-gradient(90deg, #2563eb, #1e40af); color: #ffffff; text-decoration: none; font-size: 16px; border-radius: 6px; font-weight: bold; box-shadow: 0px 3px 8px rgba(37, 99, 235, 0.4);'>
                                                            Confirm Email
                                                        </a>
                                                        
                                                        <p style='color: #777; margin-top: 35px; font-size: 14px; line-height: 1.6;'>
                                                            If you did not request this email, simply ignore it.<br>
                                                            This link will expire shortly for your security.
                                                        </p>
                                                    </td>
                                                </tr>
                                            </table>
                                            
                                            <!-- Footer -->
                                            <p style='color: #888; margin-top: 25px; font-size: 12px;'>
                                                © 2025 Cinema — All Rights Reserved
                                            </p>
                                        </td>
                                    </tr>
                                </table>
                            </body>
                        </html>";

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
             var htmlMessage = $@"
            <html>
                <body style='margin: 0; padding: 0; background-color: #f3f6fb; font-family: Arial, sans-serif;'>
                    <table width='100%' cellspacing='0' cellpadding='0' style='background-color: #f3f6fb; padding: 40px 0;'>
                        <tr>
                            <td align='center'>
                                <table width='600' cellspacing='0' cellpadding='0' 
                                       style='background: #ffffff; border-radius: 10px; padding: 40px; box-shadow: 0px 4px 15px rgba(0,0,0,0.1);'>
                                    <tr>
                                        <td align='center'>
                                            <h2 style='color: #1e3a8a; margin-bottom: 10px; font-size: 26px;'>
                                                Reset Your Password
                                            </h2>
                                            <p style='color: #555; font-size: 16px; margin-bottom: 30px;'>
                                                We received a request to reset your password.<br>
                                                Please use the OTP code below:
                                            </p>

                                            <div style='font-size: 32px; font-weight: bold; color: #1e40af; letter-spacing: 3px; margin-bottom: 30px;'>
                                                {otpCode}
                                            </div>

                                            <p style='color: #777; margin-top: 20px; font-size: 14px; line-height: 1.6;'>
                                                This OTP is valid for a short time only.<br>
                                                If you did not request a password reset, please ignore this email.
                                            </p>
                                        </td>
                                    </tr>
                                </table>

                                <p style='color: #888; margin-top: 25px; font-size: 12px;'>
                                    © 2025 Cinema — All Rights Reserved
                                </p>
                            </td>
                        </tr>
                    </table>
                </body>
            </html>";

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
            var htmlMessage = $@"
                        <html>
                            <body style='margin: 0; padding: 0; background-color: #f3f6fb; font-family: Arial, sans-serif;'>
                                <table width='100%' cellspacing='0' cellpadding='0' style='background-color: #f3f6fb; padding: 40px 0;'>
                                    <tr>
                                        <td align='center'>
                                            <!-- Main Card -->
                                            <table width='600' cellspacing='0' cellpadding='0' 
                                                   style='background: #ffffff; border-radius: 10px; padding: 40px; box-shadow: 0px 4px 15px rgba(0,0,0,0.1);'>
                                                <tr>
                                                    <td align='center'>
                                                        <h2 style='color: #1e3a8a; margin-bottom: 10px; font-size: 26px;'>
                                                            Welcome to Our Website!
                                                        </h2>
                                                        <p style='color: #555; font-size: 16px; margin-bottom: 30px;'>
                                                            Thanks for joining us! Please confirm your email address by clicking the button below.
                                                        </p>
                                                        
                                                        <!-- Button -->
                                                        <a href='{link}' style='display: inline-block; padding: 14px 28px; background: linear-gradient(90deg, #2563eb, #1e40af); color: #ffffff; text-decoration: none; font-size: 16px; border-radius: 6px; font-weight: bold; box-shadow: 0px 3px 8px rgba(37, 99, 235, 0.4);'>
                                                            Confirm Email
                                                        </a>
                                                        
                                                        <p style='color: #777; margin-top: 35px; font-size: 14px; line-height: 1.6;'>
                                                            If you did not request this email, simply ignore it.<br>
                                                            This link will expire shortly for your security.
                                                        </p>
                                                    </td>
                                                </tr>
                                            </table>
                                            
                                            <!-- Footer -->
                                            <p style='color: #888; margin-top: 25px; font-size: 12px;'>
                                                © 2025 Cinema — All Rights Reserved
                                            </p>
                                        </td>
                                    </tr>
                                </table>
                            </body>
                        </html>";

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