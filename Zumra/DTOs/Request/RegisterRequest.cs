using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Zumra.DTOs.Request;

public class RegisterRequest
{
    [Required(ErrorMessage = "Name is required")]
    public string Name { get; set; }
    

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    public string Email { get; set; }
    
    [Required(ErrorMessage = "Phone is required")]
    // [EmailAddress(ErrorMessage = "Invalid email format")]
    public string PhoneNumber { get; set; }

    [Required(ErrorMessage = "Password is required")]
    [DataType(DataType.Password)]
    [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
    public string Password { get; set; }

    [Required(ErrorMessage = "Please confirm your password")]
    [DataType(DataType.Password)]
    [Compare("Password", ErrorMessage = "Passwords do not match")]
    public string ConfirmPassword { get; set; }

    [Required(ErrorMessage = "You must accept the terms")]
    [Range(typeof(bool), "true", "true", ErrorMessage = "You must accept the terms")]
    public bool AcceptTerms { get; set; }
}