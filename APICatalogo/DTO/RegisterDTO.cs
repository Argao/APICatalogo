using System.ComponentModel.DataAnnotations;

namespace APICatalogo.DTO;

public class RegisterDTO
{
    [Required(ErrorMessage = "User name is required")]
    public string? UserName { get; set; }
    
    [EmailAddress]
    [Required(ErrorMessage = "Email is required")]
    public string? Email { get; set; }
    
    
    [Required(ErrorMessage = "Password is required")] 
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters long")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{6,}$", 
        ErrorMessage = "Password must contain at least one lowercase letter, one uppercase letter, one digit, and one special character")]
    public string? Password { get; set; }
}