namespace Zumra.Models;

public class Otp
{
    public Otp(string email, string otpCode)
    {
        Email = email;
        OtpCode = otpCode;
        Expiration = DateTime.UtcNow.AddMinutes(5);
    }

    public int Id { get; set; }
    public string Email { get; set; }
    public string OtpCode { get; set; }
    public DateTime Expiration { get; set; } 
    public bool IsUsed { get; set; } = false;
    
}